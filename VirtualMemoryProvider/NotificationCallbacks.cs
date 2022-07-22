// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.Windows.ProjFS;
using Serilog;
using System;
using System.Collections.Generic;

namespace MemoryFS
{
    class NotificationCallbacks
    {
        private readonly MemoryProvider provider;

        public NotificationCallbacks(
            MemoryProvider provider,
            VirtualizationInstance virtInstance,
            IReadOnlyCollection<NotificationMapping> notificationMappings)
        {
            this.provider = provider;

            // Look through notificationMappings for all the set notification bits.  Supply a callback
            // for each set bit.
            NotificationType notification = NotificationType.None;
            foreach (NotificationMapping mapping in notificationMappings)
            {
                notification |= mapping.NotificationMask;
            }

            if ((notification & NotificationType.FileOpened) == NotificationType.FileOpened)
            {
                virtInstance.OnNotifyFileOpened = NotifyFileOpenedCallback;
            }

            if ((notification & NotificationType.NewFileCreated) == NotificationType.NewFileCreated)
            {
                virtInstance.OnNotifyNewFileCreated = NotifyNewFileCreatedCallback;
            }

            if ((notification & NotificationType.FileOverwritten) == NotificationType.FileOverwritten)
            {
                virtInstance.OnNotifyFileOverwritten = NotifyFileOverwrittenCallback;
            }

            if ((notification & NotificationType.PreDelete) == NotificationType.PreDelete)
            {
                virtInstance.OnNotifyPreDelete = NotifyPreDeleteCallback;
            }

            if ((notification & NotificationType.PreRename) == NotificationType.PreRename)
            {
                virtInstance.OnNotifyPreRename = NotifyPreRenameCallback;
            }

            if ((notification & NotificationType.PreCreateHardlink) == NotificationType.PreCreateHardlink)
            {
                virtInstance.OnNotifyPreCreateHardlink = NotifyPreCreateHardlinkCallback;
            }

            if ((notification & NotificationType.FileRenamed) == NotificationType.FileRenamed)
            {
                virtInstance.OnNotifyFileRenamed = NotifyFileRenamedCallback;
            }

            if ((notification & NotificationType.HardlinkCreated) == NotificationType.HardlinkCreated)
            {
                virtInstance.OnNotifyHardlinkCreated = NotifyHardlinkCreatedCallback;
            }

            if ((notification & NotificationType.FileHandleClosedNoModification) == NotificationType.FileHandleClosedNoModification)
            {
                virtInstance.OnNotifyFileHandleClosedNoModification = NotifyFileHandleClosedNoModificationCallback;
            }

            if (((notification & NotificationType.FileHandleClosedFileModified) == NotificationType.FileHandleClosedFileModified) ||
                ((notification & NotificationType.FileHandleClosedFileDeleted) == NotificationType.FileHandleClosedFileDeleted))
            {
                virtInstance.OnNotifyFileHandleClosedFileModifiedOrDeleted = NotifyFileHandleClosedFileModifiedOrDeletedCallback;
            }

            if ((notification & NotificationType.FilePreConvertToFull) == NotificationType.FilePreConvertToFull)
            {
                virtInstance.OnNotifyFilePreConvertToFull = NotifyFilePreConvertToFullCallback;
            }
        }

        public bool NotifyFileOpenedCallback(
            string relativePath,
            bool isDirectory,
            uint triggeringProcessId,
            string triggeringProcessImageFileName,
            out NotificationType notificationMask)
        {
            Log.Information("NotifyFileOpenedCallback [{relativePath}]", relativePath);
            Log.Information("  Notification triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]",
                triggeringProcessImageFileName, triggeringProcessId);

            notificationMask = NotificationType.UseExistingMask;
            return true;
        }


        public void NotifyNewFileCreatedCallback(
            string relativePath,
            bool isDirectory,
            uint triggeringProcessId,
            string triggeringProcessImageFileName,
            out NotificationType notificationMask)
        {
            Log.Information("NotifyNewFileCreatedCallback [{relativePath}]", relativePath);
            Log.Information("  Notification triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]",
                triggeringProcessImageFileName, triggeringProcessId);

            notificationMask = NotificationType.UseExistingMask;
        }

        public void NotifyFileOverwrittenCallback(
            string relativePath,
            bool isDirectory,
            uint triggeringProcessId,
            string triggeringProcessImageFileName,
            out NotificationType notificationMask)
        {
            Log.Information("NotifyFileOverwrittenCallback [{relativePath}]", relativePath);
            Log.Information("  Notification triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]",
                triggeringProcessImageFileName, triggeringProcessId);

            notificationMask = NotificationType.UseExistingMask;
        }

        public bool NotifyPreDeleteCallback(
            string relativePath,
            bool isDirectory,
            uint triggeringProcessId,
            string triggeringProcessImageFileName)
        {
            Log.Information("NotifyPreDeleteCallback [{relativePath}]", relativePath);
            Log.Information("  Notification triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]",
                triggeringProcessImageFileName, triggeringProcessId);

            return provider.Options.DenyDeletes ? false : true;
        }

        public bool NotifyPreRenameCallback(
            string relativePath,
            string destinationPath,
            uint triggeringProcessId,
            string triggeringProcessImageFileName)
        {
            Log.Information("NotifyPreRenameCallback [{relativePath}] [{destinationPath}]", relativePath, destinationPath);
            Log.Information("  Notification triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]",
                triggeringProcessImageFileName, triggeringProcessId);

            return true;
        }

        public bool NotifyPreCreateHardlinkCallback(
            string relativePath,
            string destinationPath,
            uint triggeringProcessId,
            string triggeringProcessImageFileName)
        {
            Log.Information("NotifyPreCreateHardlinkCallback [{relativePath}] [{destinationPath}]", relativePath, destinationPath);
            Log.Information("  Notification triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]",
                triggeringProcessImageFileName, triggeringProcessId);

            return true;
        }

        public void NotifyFileRenamedCallback(
            string relativePath,
            string destinationPath,
            bool isDirectory,
            uint triggeringProcessId,
            string triggeringProcessImageFileName,
            out NotificationType notificationMask)
        {
            Log.Information("NotifyFileRenamedCallback [{relativePath}] [{destinationPath}]", relativePath, destinationPath);
            Log.Information("  Notification triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]",
                triggeringProcessImageFileName, triggeringProcessId);

            notificationMask = NotificationType.UseExistingMask;
        }

        public void NotifyHardlinkCreatedCallback(
            string relativePath,
            string destinationPath,
            uint triggeringProcessId,
            string triggeringProcessImageFileName)
        {
            Log.Information("NotifyHardlinkCreatedCallback [{relativePath}] [{destinationPath}]", relativePath, destinationPath);
            Log.Information("  Notification triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]",
                triggeringProcessImageFileName, triggeringProcessId);

        }

        public void NotifyFileHandleClosedNoModificationCallback(
            string relativePath,
            bool isDirectory,
            uint triggeringProcessId,
            string triggeringProcessImageFileName)
        {
            Log.Information("NotifyFileHandleClosedNoModificationCallback [{relativePath}]", relativePath);
            Log.Information("  Notification triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]",
                triggeringProcessImageFileName, triggeringProcessId);

        }

        public void NotifyFileHandleClosedFileModifiedOrDeletedCallback(
            string relativePath,
            bool isDirectory,
            bool isFileModified,
            bool isFileDeleted,
            uint triggeringProcessId,
            string triggeringProcessImageFileName)
        {
            Log.Information("NotifyFileHandleClosedFileModifiedOrDeletedCallback [{relativePath}]", relativePath);
            Log.Information("  Modified: {isFileModified}, Deleted: {isFileDeleted} ", isFileModified, isFileDeleted);
            Log.Information("  Notification triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]",
                triggeringProcessImageFileName, triggeringProcessId);

        }

        public bool NotifyFilePreConvertToFullCallback(
            string relativePath,
            uint triggeringProcessId,
            string triggeringProcessImageFileName)
        {
            Log.Information("NotifyFilePreConvertToFullCallback [{relativePath}]", relativePath);
            Log.Information("  Notification triggered by [{triggeringProcessImageFileName} {triggeringProcessId}]",
                triggeringProcessImageFileName, triggeringProcessId);
            return true;
        }

    }
}
