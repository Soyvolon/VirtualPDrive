using PDriveUtility.Services.Arma;
using PDriveUtility.Services.Local;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDriveUtility.Services.Files;
public class FileManagerService : IFileManagerService
{
    private enum NodeIcon : int
    {
        Folder = 0,
        FolderOpen,
        Config,
        Text,
        Image,
    }

    private enum NodeStatus : int
    {
        None = 0,
        Refresh,
        Check,
        Downloading,
        Error
    }

    private static string[] ConfigExts = new string[] { ".bin", ".cpp" };
    private static string[] TextureExts = new string[] { ".png", ".paa" };

    private readonly IArmaService _armaService;
    private readonly ILocalFileService _localFileService;

    private ConcurrentDictionary<string, TreeNode> NodeMap { get; set; } = new();

    public FileManagerService(IArmaService armaService, ILocalFileService localFileService)
    {
        _armaService = armaService;
        _localFileService = localFileService;
    }

    public void Initalize()
    {
        NodeMap.Clear();

        LoadFiles();
    }

    private void LoadFiles()
    {
        foreach (var dir in _armaService.FileSystem.ChildMap)
        {
            if (!NodeMap.TryGetValue(dir.Key, out var node))
            {
                var rootParts = dir.Key.Split(Path.DirectorySeparatorChar);

                int i = rootParts.Length - 1;
                for (/* i */; i >= 0; i--)
                {
                    if (NodeMap.TryGetValue(string.Join(Path.DirectorySeparatorChar, rootParts[..i]), out node))
                        break;
                }

                for (/* i */; i < rootParts.Length; i++)
                {
                    var next = new TreeNode(rootParts[i], (int)NodeIcon.Folder, 0)
                    {
                        StateImageIndex = (int)NodeStatus.None
                    };

                    if (node is null)
                    {
                        if (!NodeMap.TryGetValue("", out var rootNode))
                        {
                            rootNode = new TreeNode("Root");
                            NodeMap[""] = rootNode;
                        }

                        node = rootNode;
                    }

                    node.Nodes.Add(next);

                    node = next;
                }
            }

            foreach (var item in dir.Value)
            {


            }
        }
    }

    public TreeNode[] GetTopLevelNodes(string dirFilter = "")
    {
        if (NodeMap.TryGetValue(dirFilter, out var root))
            return root.Nodes.Cast<TreeNode>().ToArray();

        return Array.Empty<TreeNode>();
    }
}
