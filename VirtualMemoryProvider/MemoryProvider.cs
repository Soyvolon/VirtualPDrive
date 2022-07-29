// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using Microsoft.Windows.ProjFS;
using System.Diagnostics.CodeAnalysis;
using PDriveFileSystem.FileSystem;

namespace PDriveFileSystem
{
    /// <summary>
    /// This is a simple file system "reflector" provider.  It projects files and directories from
    /// a directory called the "layer root" into the virtualization root, also called the "scratch root".
    /// </summary>
    public class MemoryProvider : IDisposable
    {
        // These variables hold the layer and scratch paths.
        // This is the virtual root - hold virtual data.
        private readonly string scratchRoot;

        public MemoryFileSystem MemorySystem { get; init; }

        private readonly VirtualizationInstance virtualizationInstance;
        private readonly ConcurrentDictionary<Guid, ActiveEnumeration> activeEnumerations;

        private NotificationCallbacks notificationCallbacks;

        private bool isSymlinkSupportAvailable;

        public MemoryProviderOptions Options { get; }

        public MemoryProvider(MemoryProviderOptions options)
        {
            this.scratchRoot = options.VirtRoot;
            this.MemorySystem = new(options.VirtRoot, options.ReadableExtensions, options.Whitelist, options.InitRunners, options.Local);

            this.Options = options;

            // Enable notifications if the user requested them.
            List<NotificationMapping> notificationMappings;
            if (this.Options.EnableNotifications)
            {
                string rootName = string.Empty;
                notificationMappings = new List<NotificationMapping>()
                {
                    new NotificationMapping(
                        NotificationType.FileOpened
                        | NotificationType.NewFileCreated
                        | NotificationType.FileOverwritten
                        | NotificationType.PreDelete
                        | NotificationType.PreRename
                        | NotificationType.PreCreateHardlink
                        | NotificationType.FileRenamed
                        | NotificationType.HardlinkCreated
                        | NotificationType.FileHandleClosedNoModification
                        | NotificationType.FileHandleClosedFileModified
                        | NotificationType.FileHandleClosedFileDeleted
                        | NotificationType.FilePreConvertToFull,
                        rootName)
                };
            }
            else
            {
                notificationMappings = new List<NotificationMapping>();
            }

            try
            {
                // This will create the virtualization root directory if it doesn't already exist.
                this.virtualizationInstance = new VirtualizationInstance(
                    this.scratchRoot,
                    poolThreadCount: 0,
                    concurrentThreadCount: 0,
                    enableNegativePathCache: false,
                    notificationMappings: notificationMappings);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to create VirtualizationInstance.");
                throw;
            }

            // Set up notifications.
            notificationCallbacks = new NotificationCallbacks(
                this,
                this.virtualizationInstance,
                notificationMappings);

            Log.Debug("Created instance. Memory System [{Layer}], Scratch [{Scratch}]", this.MemorySystem.ToString(), this.scratchRoot);

            this.activeEnumerations = new ConcurrentDictionary<Guid, ActiveEnumeration>();
            this.isSymlinkSupportAvailable = EnvironmentHelper.IsFullSymlinkSupportAvailable();
        }

        public bool StartVirtualization()
        {
            // Optional callbacks
            this.virtualizationInstance.OnQueryFileName = QueryFileNameCallback;

            RequiredCallbacks requiredCallbacks = new RequiredCallbacks(this);
            try
            {
                HResult hr = this.virtualizationInstance.StartVirtualizing(requiredCallbacks);
                if (hr != HResult.Ok)
                {
                    Log.Error("Failed to start virtualization instance: {Result}", hr);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to start virtualization instance: {res}", ex);
                return false;
            }

            return true;
        }

        private static bool IsEnumerationFilterSet(
            string filter)
        {
            if (string.IsNullOrWhiteSpace(filter) || filter == "*")
            {
                return false;
            }

            return true;
        }

        protected string GetFullPathInLayer(string relativePath) => Path.Combine(this.MemorySystem._rootPath, relativePath);

        protected bool DirectoryExistsInLayer(string relativePath)
            => this.MemorySystem.DirectoryExists(relativePath, false);

        protected bool FileExistsInLayer(string relativePath)
            => this.MemorySystem.FileExists(relativePath, false);

        protected ProjectedFileInfo? GetFileInfoInLayer(string relativePath)
        {
            _ = this.FileOrDirectoryExistsInLayer(relativePath, out var fileInfo);
            return fileInfo;
        }

        protected IEnumerable<ProjectedFileInfo> GetChildItemsInLayer(string relativePath)
        {
            if (this.MemorySystem.TryGetDirectory(relativePath, false, out _))
            {
                foreach (var item in MemorySystem.GetChildEntries(relativePath, false))
                {
                    yield return new(Path.GetFileName(item.Item1), item.Item1, 0, item.Item2);
                }
            }

            yield break;
        }

        protected HResult HydrateFile(string relativePath, uint bufferSize, Func<byte[], uint, bool> tryWriteBytes)
        {
            string layerPath = this.GetFullPathInLayer(relativePath);
            if (!MemorySystem.TryGetFile(layerPath, false, out var file))
            {
                return HResult.FileNotFound;
            }

            // Open the file in the layer for read.
            using var fs = file.GetDataStream();

            if (fs is null)
                return HResult.Ok;

            long remainingDataLength = fs.Length;
            byte[] buffer = new byte[bufferSize];

            while (remainingDataLength > 0)
            {
                // Read from the file into the read buffer.
                int bytesToCopy = (int)Math.Min(remainingDataLength, buffer.Length);
                if (fs.Read(buffer, 0, bytesToCopy) != bytesToCopy)
                {
                    return HResult.InternalError;
                }

                // Write the bytes we just read into the scratch.
                if (!tryWriteBytes(buffer, (uint)bytesToCopy))
                {
                    return HResult.InternalError;
                }

                remainingDataLength -= bytesToCopy;
            }

            return HResult.Ok;
        }

        private bool FileOrDirectoryExistsInLayer(string path,
            [NotNullWhen(true)] out ProjectedFileInfo? fileInfo)
        {
            var now = DateTime.Now;
            if (this.MemorySystem.TryGetFile(path, false, out var file))
            {
                fileInfo = new(file.Name, path, 0, false, now, now, now, now, FileAttributes.ReadOnly);
                return true;
            }
            else if (this.MemorySystem.TryGetDirectory(path, false, out var dir))
            {
                fileInfo = new(dir, path, 0, true, now, now, now, now, FileAttributes.ReadOnly | FileAttributes.Directory);
                return true;
            }

            fileInfo = null;
            return false;
        }

        #region Callback implementations

        // To keep all the callback implementations together we implement the required callbacks in
        // the SimpleProvider class along with the optional QueryFileName callback.  Then we have the
        // IRequiredCallbacks implementation forward the calls to here.

        internal HResult StartDirectoryEnumerationCallback(
            int commandId,
            Guid enumerationId,
            string relativePath,
            uint triggeringProcessId,
            string triggeringProcessImageFileName)
        {
            Log.Debug("----> StartDirectoryEnumerationCallback Path [{Path}]", relativePath);

            // Enumerate the corresponding directory in the layer and ensure it is sorted the way
            // ProjFS expects.
            ActiveEnumeration activeEnumeration = new ActiveEnumeration(
                GetChildItemsInLayer(relativePath)
                .OrderBy(file => file.Name, new ProjFSSorter())
                .ToList());

            // Insert the layer enumeration into our dictionary of active enumerations, indexed by
            // enumeration ID.  GetDirectoryEnumerationCallback will be able to find this enumeration
            // given the enumeration ID and return the contents to ProjFS.
            if (!this.activeEnumerations.TryAdd(enumerationId, activeEnumeration))
            {
                return HResult.InternalError;
            }

            Log.Debug("<---- StartDirectoryEnumerationCallback {Result}", HResult.Ok);

            return HResult.Ok;
        }

        internal HResult GetDirectoryEnumerationCallback(
            int commandId,
            Guid enumerationId,
            string filterFileName,
            bool restartScan,
            IDirectoryEnumerationResults enumResult)
        {
            Log.Debug("----> GetDirectoryEnumerationCallback filterFileName [{Filter}]", filterFileName);

            // Find the requested enumeration.  It should have been put there by StartDirectoryEnumeration.
            if (!this.activeEnumerations.TryGetValue(enumerationId, out ActiveEnumeration enumeration))
            {
                Log.Fatal("      GetDirectoryEnumerationCallback {Result}", HResult.InternalError);
                return HResult.InternalError;
            }

            if (restartScan)
            {
                // The caller is restarting the enumeration, so we reset our ActiveEnumeration to the
                // first item that matches filterFileName.  This also saves the value of filterFileName
                // into the ActiveEnumeration, overwriting its previous value.
                enumeration.RestartEnumeration(filterFileName);
            }
            else
            {
                // The caller is continuing a previous enumeration, or this is the first enumeration
                // so our ActiveEnumeration is already at the beginning.  TrySaveFilterString()
                // will save filterFileName if it hasn't already been saved (only if the enumeration
                // is restarting do we need to re-save filterFileName).
                enumeration.TrySaveFilterString(filterFileName);
            }

            int numEntriesAdded = 0;
            HResult hr = HResult.Ok;

            while (enumeration.IsCurrentValid)
            {
                ProjectedFileInfo fileInfo = enumeration.Current;

                if (!TryGetTargetIfReparsePoint(fileInfo, fileInfo.FullName, out string targetPath))
                {
                    hr = HResult.InternalError;
                    break;
                }

                // A provider adds entries to the enumeration buffer until it runs out, or until adding
                // an entry fails. If adding an entry fails, the provider remembers the entry it couldn't
                // add. ProjFS will call the GetDirectoryEnumerationCallback again, and the provider
                // must resume adding entries, starting at the last one it tried to add. SimpleProvider
                // remembers the entry it couldn't add simply by not advancing its ActiveEnumeration.
                if (AddFileInfoToEnum(enumResult, fileInfo, targetPath))
                {
                    Log.Verbose("----> GetDirectoryEnumerationCallback Added {Entry} {Kind} {Target}", fileInfo.Name, fileInfo.IsDirectory, targetPath);

                    ++numEntriesAdded;
                    enumeration.MoveNext();
                }
                else
                {
                    Log.Verbose("----> GetDirectoryEnumerationCallback NOT added {Entry} {Kind} {Target}", fileInfo.Name, fileInfo.IsDirectory, targetPath);

                    if (numEntriesAdded == 0)
                    {
                        hr = HResult.InsufficientBuffer;
                    }

                    break;
                }
            }

            if (hr == HResult.Ok)
            {
                Log.Debug("<---- GetDirectoryEnumerationCallback {Result} [Added entries: {EntryCount}]", hr, numEntriesAdded);
            }
            else
            {
                Log.Error("<---- GetDirectoryEnumerationCallback {Result} [Added entries: {EntryCount}]", hr, numEntriesAdded);
            }

            return hr;
        }

        private bool AddFileInfoToEnum(IDirectoryEnumerationResults enumResult, ProjectedFileInfo fileInfo, string targetPath)
        {
            if (this.isSymlinkSupportAvailable)
            {
                return enumResult.Add(
                    fileName: fileInfo.Name,
                    fileSize: fileInfo.Size,
                    isDirectory: fileInfo.IsDirectory,
                    fileAttributes: fileInfo.Attributes,
                    creationTime: fileInfo.CreationTime,
                    lastAccessTime: fileInfo.LastAccessTime,
                    lastWriteTime: fileInfo.LastWriteTime,
                    changeTime: fileInfo.ChangeTime,
                    symlinkTargetOrNull: targetPath);
            }
            else
            {
                return enumResult.Add(
                    fileName: fileInfo.Name,
                    fileSize: fileInfo.Size,
                    isDirectory: fileInfo.IsDirectory,
                    fileAttributes: fileInfo.Attributes,
                    creationTime: fileInfo.CreationTime,
                    lastAccessTime: fileInfo.LastAccessTime,
                    lastWriteTime: fileInfo.LastWriteTime,
                    changeTime: fileInfo.ChangeTime);
            }
        }

        internal HResult EndDirectoryEnumerationCallback(
            Guid enumerationId)
        {
            Log.Debug("----> EndDirectoryEnumerationCallback");

            if (!this.activeEnumerations.TryRemove(enumerationId, out ActiveEnumeration? enumeration))
            {
                return HResult.InternalError;
            }

            Log.Debug("<---- EndDirectoryEnumerationCallback {Result}", HResult.Ok);

            return HResult.Ok;
        }

        internal HResult GetPlaceholderInfoCallback(
            int commandId,
            string relativePath,
            uint triggeringProcessId,
            string triggeringProcessImageFileName)
        {
            Log.Debug("----> GetPlaceholderInfoCallback [{Path}]", relativePath);
            Log.Debug("  Placeholder creation triggered by [{ProcName} {PID}]", triggeringProcessImageFileName, triggeringProcessId);

            HResult hr = HResult.Ok;
            ProjectedFileInfo fileInfo = this.GetFileInfoInLayer(relativePath);
            if (fileInfo == null)
            {
                hr = HResult.FileNotFound;
            }
            else
            {
                string layerPath = this.GetFullPathInLayer(relativePath);
                if (!TryGetTargetIfReparsePoint(fileInfo, layerPath, out string targetPath))
                {
                    hr = HResult.InternalError;
                }
                else
                {
                    hr = WritePlaceholderInfo(relativePath, fileInfo, targetPath);
                }
            }

            Log.Debug("<---- GetPlaceholderInfoCallback {Result}", hr);
            return hr;
        }

        private HResult WritePlaceholderInfo(string relativePath, ProjectedFileInfo fileInfo, string targetPath)
        {
            if (this.isSymlinkSupportAvailable)
            {
                return this.virtualizationInstance.WritePlaceholderInfo2(
                        relativePath: Path.Combine(Path.GetDirectoryName(relativePath), fileInfo.Name),
                        creationTime: fileInfo.CreationTime,
                        lastAccessTime: fileInfo.LastAccessTime,
                        lastWriteTime: fileInfo.LastWriteTime,
                        changeTime: fileInfo.ChangeTime,
                        fileAttributes: fileInfo.Attributes,
                        endOfFile: fileInfo.Size,
                        isDirectory: fileInfo.IsDirectory,
                        symlinkTargetOrNull: targetPath,
                        contentId: new byte[] { 0 },
                        providerId: new byte[] { 1 });
            }
            else
            {
                return this.virtualizationInstance.WritePlaceholderInfo(
                        relativePath: Path.Combine(Path.GetDirectoryName(relativePath), fileInfo.Name),
                        creationTime: fileInfo.CreationTime,
                        lastAccessTime: fileInfo.LastAccessTime,
                        lastWriteTime: fileInfo.LastWriteTime,
                        changeTime: fileInfo.ChangeTime,
                        fileAttributes: fileInfo.Attributes,
                        endOfFile: fileInfo.Size,
                        isDirectory: fileInfo.IsDirectory,
                        contentId: new byte[] { 0 },
                        providerId: new byte[] { 1 });
            }
        }

        internal HResult GetFileDataCallback(
            int commandId,
            string relativePath,
            ulong byteOffset,
            uint length,
            Guid dataStreamId,
            byte[] contentId,
            byte[] providerId,
            uint triggeringProcessId,
            string triggeringProcessImageFileName)
        {
            Log.Debug("----> GetFileDataCallback relativePath [{Path}]", relativePath);
            Log.Debug("  triggered by [{ProcName} {PID}]", triggeringProcessImageFileName, triggeringProcessId);

            HResult hr = HResult.Ok;
            if (!this.FileExistsInLayer(relativePath))
            {
                hr = HResult.FileNotFound;
            }
            else
            {
                // We'll write the file contents to ProjFS no more than 64KB at a time.
                uint desiredBufferSize = Math.Min(64 * 1024, length);
                try
                {
                    // We could have used VirtualizationInstance.CreateWriteBuffer(uint), but this 
                    // illustrates how to use its more complex overload.  This method gets us a 
                    // buffer whose underlying storage is properly aligned for unbuffered I/O.
                    using (IWriteBuffer writeBuffer = this.virtualizationInstance.CreateWriteBuffer(
                        byteOffset,
                        desiredBufferSize,
                        out ulong alignedWriteOffset,
                        out uint alignedBufferSize))
                    {
                        // Get the file data out of the layer and write it into ProjFS.
                        hr = this.HydrateFile(
                            relativePath,
                            alignedBufferSize,
                            (readBuffer, bytesToCopy) =>
                            {
                                // readBuffer contains what HydrateFile() read from the file in the
                                // layer.  Now seek to the beginning of the writeBuffer and copy the
                                // contents of readBuffer into writeBuffer.
                                writeBuffer.Stream.Seek(0, SeekOrigin.Begin);
                                writeBuffer.Stream.Write(readBuffer, 0, (int)bytesToCopy);

                                // Write the data from the writeBuffer into the scratch via ProjFS.
                                HResult writeResult = this.virtualizationInstance.WriteFileData(
                                    dataStreamId,
                                    writeBuffer,
                                    alignedWriteOffset,
                                    bytesToCopy);

                                if (writeResult != HResult.Ok)
                                {
                                    Log.Error("VirtualizationInstance.WriteFileData failed: {Result}", writeResult);
                                    return false;
                                }

                                alignedWriteOffset += bytesToCopy;
                                return true;
                            });

                        if (hr != HResult.Ok)
                        {
                            return HResult.InternalError;
                        }
                    }
                }
                catch (OutOfMemoryException e)
                {
                    Log.Error(e, "Out of memory");
                    hr = HResult.OutOfMemory;
                }
                catch (Exception e)
                {
                    Log.Error(e, "Exception");
                    hr = HResult.InternalError;
                }
            }

            Log.Debug("<---- return status {Result}", hr);
            return hr;
        }

        private HResult QueryFileNameCallback(
            string relativePath)
        {
            Log.Debug("----> QueryFileNameCallback relativePath [{Path}]", relativePath);

            HResult hr = HResult.Ok;
            string parentDirectory = Path.GetDirectoryName(relativePath);
            string childName = Path.GetFileName(relativePath);
            if (this.GetChildItemsInLayer(parentDirectory).Any(child => Utils.IsFileNameMatch(child.Name, childName)))
            {
                hr = HResult.Ok;
            }
            else
            {
                hr = HResult.FileNotFound;
            }

            Log.Debug("<---- QueryFileNameCallback {Result}", hr);
            return hr;
        }

        private bool TryGetTargetIfReparsePoint(ProjectedFileInfo fileInfo, string fullPath, out string targetPath)
        {
            targetPath = null;

            if ((fileInfo.Attributes & FileAttributes.ReparsePoint) != 0 /* TODO: Check for reparse point type */)
            {
                if (!FileSystemApi.TryGetReparsePointTarget(fullPath, out targetPath))
                {
                    return false;
                }
                else if (Path.IsPathRooted(targetPath))
                {
                    string targetRelativePath = FileSystemApi.TryGetPathRelativeToRoot(this.MemorySystem._rootPath, targetPath, fileInfo.IsDirectory);
                    // GetFullPath is used to get rid of relative path components (such as .\)
                    targetPath = Path.GetFullPath(Path.Combine(this.scratchRoot, targetRelativePath));

                    return true;
                }
            }

            return true;
        }

        #endregion


        private class RequiredCallbacks : IRequiredCallbacks
        {
            private readonly MemoryProvider provider;

            public RequiredCallbacks(MemoryProvider provider) => this.provider = provider;

            // We implement the callbacks in the SimpleProvider class.

            public HResult StartDirectoryEnumerationCallback(
                int commandId,
                Guid enumerationId,
                string relativePath,
                uint triggeringProcessId,
                string triggeringProcessImageFileName)
            {
                return this.provider.StartDirectoryEnumerationCallback(
                    commandId,
                    enumerationId,
                    relativePath,
                    triggeringProcessId,
                    triggeringProcessImageFileName);
            }

            public HResult GetDirectoryEnumerationCallback(
                int commandId,
                Guid enumerationId,
                string filterFileName,
                bool restartScan,
                IDirectoryEnumerationResults enumResult)
            {
                return this.provider.GetDirectoryEnumerationCallback(
                    commandId,
                    enumerationId,
                    filterFileName,
                    restartScan,
                    enumResult);
            }

            public HResult EndDirectoryEnumerationCallback(
                Guid enumerationId)
            {
                return this.provider.EndDirectoryEnumerationCallback(enumerationId);
            }

            public HResult GetPlaceholderInfoCallback(
                int commandId,
                string relativePath,
                uint triggeringProcessId,
                string triggeringProcessImageFileName)
            {
                return this.provider.GetPlaceholderInfoCallback(
                    commandId,
                    relativePath,
                    triggeringProcessId,
                    triggeringProcessImageFileName);
            }

            public HResult GetFileDataCallback(
                int commandId,
                string relativePath,
                ulong byteOffset,
                uint length,
                Guid dataStreamId,
                byte[] contentId,
                byte[] providerId,
                uint triggeringProcessId,
                string triggeringProcessImageFileName)
            {
                return this.provider.GetFileDataCallback(
                    commandId,
                    relativePath,
                    byteOffset,
                    length,
                    dataStreamId,
                    contentId,
                    providerId,
                    triggeringProcessId,
                    triggeringProcessImageFileName);
            }
        }

        public void Dispose()
        {
            MemorySystem.Dispose();
            virtualizationInstance.StopVirtualizing();
        }
    }
}