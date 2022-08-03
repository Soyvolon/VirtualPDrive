using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDriveUtility.Services.Files;
public interface IFileManagerService
{
    public void Initalize();
    public TreeNode[] GetTopLevelNodes(string dirFilter = "");
}
