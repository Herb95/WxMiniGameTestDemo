#region DirectoryUtils
/*
 *    Title: Disciple: U3dFishingNewResManager
 *    Description    : IO 工具类
 *    Author         : GrayWolfZ
 *    Time           : 2022/05/18 14:29:08
 *    Version        : 0.1
 *    Modify Recorder: 
 *
 */
#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Utils
{
    public class DirectoryUtils
    {
        //--------------------------------------------------------
        //编辑器相关

        public static string GetExePath()
        {
            return UniformPath(GetFolderPath(typeof(DirectoryUtils).Assembly.Location));
        }

        /// <summary>
        /// 获取Unity当前工程目录
        /// 例 : F:/xxx/xxx/
        /// </summary>
        /// <returns></returns>
        public static string GetCurProjectPath()
        {
            return UniformPath(Directory.GetCurrentDirectory() + "/");
        }

        /// <summary>
        /// 例 : F:/xxx/xxx/Assets/ 完整路径
        /// </summary>
        /// <returns></returns>
        public static string GetEditorAssetsPath()
        {
#if UNITY_EDITOR
            return UniformPath(Directory.GetCurrentDirectory() + "/Assets/");
#else
            return "";
#endif
        }

        /// <summary>
        /// 通过文件名,获取Assets下的路径的具体路径
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="folderPath">添加匹配文件夹</param>
        /// <returns></returns>
        public static string GetFilePathFromFileName(string fileName, string folderPath)
        {
#if UNITY_EDITOR
            List<string> searchList = GetFileListInPath(GetCurProjectPath() + folderPath, "*", SearchOption.AllDirectories);
            string searchPath = string.Empty;
            if (searchList.Count == 0)
            {
                Debug.LogError($" 匹配目录 {folderPath} 下无文件");
                return string.Empty;
            }
            string fileName1 = fileName.ToLower();
            for (int i = searchList.Count - 1; i >= 0; i--)
            {
                if (searchList[i].Contains(".meta"))
                {
                    continue;
                }
                if (!searchList[i].ToLower().Contains(fileName1))
                {
                    continue;
                }
                searchPath = searchList[i];
                break;
            }
            if (string.IsNullOrEmpty(searchPath))
            {
                Debug.LogError($"未查询与 {folderPath} 匹配的文件{fileName}");
                return string.Empty;
            }
            searchPath = searchPath.Replace(GetCurProjectPath(), "");
            return UniformPath(searchPath);
#else
            return string.Empty;
#endif
        }

        /// <summary>
        /// 通过文件名,获取Assets下的路径的文件夹
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="folderPath">添加匹配文件夹</param>
        /// <returns></returns>
        public static string GetPathFromFileName(string fileName, string folderPath = "")
        {
#if UNITY_EDITOR
            string[] path = UnityEditor.AssetDatabase.FindAssets(fileName);
            if (path.Length > 1 && string.IsNullOrEmpty(folderPath))
            {
                Debug.LogWarning($"有同名文件{fileName},获取路径失败");
                return string.Empty;
            }
            string searchPathGUI = string.Empty;
            for (int i = 0; i < path.Length; i++)
            {
                if (UnityEditor.AssetDatabase.GUIDToAssetPath(path[i]).ToLower().Contains(folderPath.ToLower()))
                {
                    searchPathGUI = path[i];
                }
            }
            if (string.IsNullOrEmpty(searchPathGUI))
            {
                Debug.LogError($"未查询与 {folderPath} 匹配的文件{fileName}");
                return string.Empty;
            }
            return UniformPath(Path.GetDirectoryName(UnityEditor.AssetDatabase.GUIDToAssetPath(searchPathGUI)));
#else
            return string.Empty;
#endif
        }

        /// <summary>
        /// 获取Unity编辑器下相对Assets路径
        /// </summary>
        /// <param name="absPath"></param>
        /// <returns></returns>
        public static string GetRelPathToAssets(string absPath)
        {
            string cur = GetCurProjectPath();
            if (Directory.Exists(absPath))
            {
                absPath += "/";
            }
            if (absPath.StartsWith(cur))
            {
                absPath = absPath.Replace(cur, "");
                absPath = absPath.TrimStart('/');
            }
            return absPath;
        }

        /// <summary>
        /// 获取Unity资源文件夹下所有文件
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static List<string> GetAllFilesInAssets(string pattern = "*")
        {
            return GetFileListInPath(GetEditorAssetsPath(), pattern, SearchOption.AllDirectories);
        }

        //--------------------------------------------------------
        //Unity 平台相关
        public static string GetPlatformName()
        {
#if UNITY_EDITOR
            return GetPlatformName(UnityEditor.EditorUserBuildSettings.activeBuildTarget);
#else
            return GetPlatformName(Application.platform);
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器下的平台路径
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private static string GetPlatformName(UnityEditor.BuildTarget target)
        {
            switch (target)
            {
                case UnityEditor.BuildTarget.Android:
                    return "android";
                case UnityEditor.BuildTarget.iOS:
                    return "ios";
                case UnityEditor.BuildTarget.StandaloneWindows:
                case UnityEditor.BuildTarget.StandaloneWindows64:
                    return "windows";
                case UnityEditor.BuildTarget.StandaloneOSX:
                    return "osx";
                default:
                    return null;
            }
        }
#endif

        /// <summary>
        /// 发布的平台路径
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        private static string GetPlatformName(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return "android";
                case RuntimePlatform.IPhonePlayer:
                    return "ios";
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return "windows";
                case RuntimePlatform.OSXPlayer:
                    return "osx";
                default:
                    return null;
            }
        }

        //--------------------------------------------------------
        //文件、目录通用操作

        /// <summary>
        /// 大小写敏感的进行判定,win专属
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static bool FileExistsWithDifferentCase(string filePath)
        {
            if (File.Exists(filePath))
            {
                string dir = Path.GetDirectoryName(filePath);
                string fileTitle = Path.GetFileName(filePath);
                string[] files = Directory.GetFiles(dir ?? string.Empty, fileTitle);
                string realFilePath = UniformPath(files[0]);
                filePath = UniformPath(filePath).Replace("//", "/");
                return string.CompareOrdinal(realFilePath, filePath) == 0;
            }
            return false;
        }

        /// <summary>
        /// 在 Windows 上，文件协议有一个奇怪的规则，它多了一个斜杠
        /// </summary>
        public static string GetFileProtocol
        {
            get
            {
                string fileProtocol = "file://";
                if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    fileProtocol = "file:///";
                }
                return fileProtocol;
            }
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static long GetFileSize(string file)
        {
            if (File.Exists(file))
            {
                return new FileInfo(file).Length;
            }
            return 0;
        }

        /// <summary>
        /// 无视锁文件,直接读bytes
        /// </summary>
        /// <param name="resPath"></param>
        /// <returns></returns>
        public static byte[] ReadAllBytes(string resPath)
        {
            byte[] bytes;
            using (FileStream fs = File.Open(resPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bytes = new byte[fs.Length];
                fs.Read(bytes, 0, (int)fs.Length);
            }
            return bytes;
        }

        /// <summary>
        /// 获取文件夹的所有文件
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="pattern"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static List<string> GetFileListInPath(string dirPath, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            try
            {
                string[] list = Directory.GetFiles(dirPath, pattern, option);
                List<string> r = new List<string>(list);
                for (int i = 0; i < r.Count; i++)
                {
                    r[i] = UniformPath(r[i]);
                }
                return r;
            }
            catch (DirectoryNotFoundException ex)
            {
                Debug.LogError(ex.Message);
                return new List<string>();
            }
        }

        /// <summary>
        /// 统一路径 "/" 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="enSureEnd"></param>
        /// <returns></returns>
        public static string UniformPath(string path, bool enSureEnd = false)
        {
            path = path.Replace("\\", "/");
            if (enSureEnd)
                path = path.TrimEnd('/') + "/";
            return path;
        }

        /// <summary>
        /// 统一路径"\\"
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ensureEnd"></param>
        /// <returns></returns>
        public static string UniformPathR(string path, bool ensureEnd = false)
        {
            path = path.Replace("/", "\\");
            if (ensureEnd)
            {
                path = path.TrimEnd('\\') + "\\";
            }
            return path;
        }
        /// <summary>
        /// 获取文件夹下的所有目录
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="pattern"></param>
        /// <param name="option"></param>
        /// <param name="containSelf"></param>
        /// <returns></returns>
        public static List<string> GetFolderListInPath(string dirPath, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly, bool containSelf = false)
        {
            List<string> result = new List<string>(Directory.GetDirectories(dirPath, pattern, option));
            if (containSelf)
            {
                result.Add(dirPath);
            }
            return result;
        }

        /// <summary>
        /// 获取目录下最新目录
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static string GetNewFolderNameInPath(string dirPath)
        {
            string path = GetNewFolderInPath(dirPath);
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }
            string[] pList = path.Split('\\');
            if (pList.Length > 0)
            {
                return pList[pList.Length - 1];
            }
            return string.Empty;
        }
        /// <summary>
        /// 获取目录下最新目录
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static string GetNewFolderInPath(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Debug.LogWarning($"读取的目录不存在,请检查目录: {dirPath}");
                return string.Empty;
            }
            List<string> result = new List<string>(Directory.GetDirectories(dirPath, "*", SearchOption.TopDirectoryOnly));
            List<DirectoryInfo> rDirList = new List<DirectoryInfo>();
            for (int i = 0; i < result.Count; i++)
            {
                DirectoryInfo dic = new DirectoryInfo(result[i]);
                rDirList.Add(dic);
            }
            if (rDirList.Count == 0)
            {
                return string.Empty;
            }
            DirectoryInfo newDir = rDirList[0];
            for (int i = 1; i < rDirList.Count; i++)
            {
                if (rDirList[i].CreationTime > newDir.CreationTime)
                {
                    newDir = rDirList[i];
                }
            }
            return newDir.FullName;
        }

        /// <summary>
        /// 获取文件夹下所有文件夹和文件
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="pattern"></param>
        /// <param name="option"></param>
        /// <param name="containSelf"></param>
        /// <returns></returns>
        public static List<string> GetFolderAndFile(string dirPath, string pattern = "*", SearchOption option = SearchOption.AllDirectories, bool containSelf = false)
        {
            List<string> result = GetFolderListInPath(dirPath, pattern, option, containSelf);
            result.AddRange(GetFileListInPath(dirPath, pattern, option));
            return result;
        }

        /// <summary>
        /// 获取文件夹的名字
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFolderName(string path)
        {
            path = UniformPath(path);
            path = path.TrimEnd('/');
            int index = path.LastIndexOf('/');
            if (index != -1)
            {
                return path.Substring(index + 1);
            }
            return path;
        }

        /// <summary>
        /// 获取目录的上一级目录,文件的当前目录
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFolderPath(string path)
        {
            path = UniformPath(path);
            int index = path.LastIndexOf('/');
            if (index != -1)
            {
                return path.Substring(0, index);
            }
            return path;
        }

        /// <summary>
        /// 获取不带文件扩展名的路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFilePathWithOutExt(string path)
        {
            path = UniformPath(path);
            int index = path.LastIndexOf('.');
            if (index != -1)
            {
                return path.Substring(0, index);
            }
            return path;
        }

        /// <summary>
        /// 拷贝文件到目标文件
        /// </summary>
        /// <param name="srcFile"></param>
        /// <param name="desFile"></param>
        /// <param name="overwrite"></param>
        public static void CopyFileTo(string srcFile, string desFile, bool overwrite = true)
        {
            EnsurePath(desFile);
            File.Copy(srcFile, desFile, overwrite);
        }

        /// <summary>
        /// 拷贝文件到目标目录
        /// </summary>
        /// <param name="srcFile"></param>
        /// <param name="desFolder"></param>
        public static void CopyFileToFolder(string srcFile, string desFolder)
        {
            EnsurePath(desFolder);
            desFolder = UniformPath(desFolder, true);
            string desFile = desFolder + Path.GetFileName(srcFile);
            File.Copy(srcFile, desFile);
        }

        /// <summary>
        /// 确保路径存在,不存在创建路径
        /// </summary>
        /// <param name="path"></param>
        public static void EnsurePath(string path)
        {
            string folder = GetFolderPath(path);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        /// <summary>
        /// 拷贝目录到目标目录
        /// </summary>
        /// <param name="srcFolder"></param>
        /// <param name="desFolder"></param>
        /// <param name="onlyChildren"></param>
        public static void CopyFolderTo(string srcFolder, string desFolder, bool onlyChildren)
        {
            List<string> fList = GetFileListInPath(srcFolder, "*", SearchOption.AllDirectories);
            srcFolder = UniformPath(srcFolder, true);
            desFolder = UniformPath(desFolder, true);
            if (!onlyChildren)
            {
                string folderName = GetFolderName(srcFolder);
                desFolder += folderName;
            }
            desFolder = UniformPath(desFolder, true);
            for (int i = 0; i < fList.Count; i++)
            {
                string rPath = fList[i].Replace(srcFolder, "");
                string desFile = desFolder + rPath;
                CopyFileTo(fList[i], desFile);
            }
        }

        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="dPath">删除路径</param>
        public static void DeleteFolder(string dPath)
        {
            if (Directory.Exists(dPath))
            {
                List<string> fileList = GetFileListInPath(dPath, "*", SearchOption.AllDirectories);
                for (int i = 0; i < fileList.Count; i++)
                {
                    if (!IsOccupied(fileList[i]))
                    {
                        File.Delete(fileList[i]);
                    }
                }
            }
        }

        /// <summary>
        /// 判断文件是否占用转态
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static bool IsOccupied(string filePath)
        {
            try
            {
                using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="dPath"></param>
        /// <param name="pattern"></param>
        /// <param name="option"></param>
        public static void DeleteFilesInFolder(string dPath, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            IEnumerable<string> fileList = GetFileListInPath(dPath, pattern, option);
            foreach (var filePath in fileList)
            {
                File.Delete(filePath);
            }
        }
        public static void UnityDeleteFilesInFolder(string dPath, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
        {
#if UNITY_EDITOR
            IEnumerable<string> fileList = GetFileListInPath(dPath, pattern, option);
            foreach (var filePath in fileList)
            {
                string file = filePath.Replace(GetCurProjectPath(), "");
                UnityEditor.AssetDatabase.DeleteAsset(file);
            }
#endif
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
        public struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.U4)] public int wFunc;
            public string pFrom;
            public string pTo;
            public short fFlags;
            [MarshalAs(UnmanagedType.Bool)] public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lPszProgressTitle;
        }

        private const int FO_DELETE = 3;
        private const int FOF_ALLOWUNDO = 0x40;
        private const int FOF_NOCONFIRMATION = 0x0010;

        [DllImport("shell32.dll", CharSet = CharSet.Auto)] private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

        public static void DeleteFilesInFolder_Trash(string dPath)
        {
            List<string> fileList = GetFileListInPath(dPath);
            for (int i = 0; i < fileList.Count; i++)
            {
                SHFILEOPSTRUCT fileOp = new SHFILEOPSTRUCT();
                fileOp.wFunc = FO_DELETE;
                fileOp.pFrom = fileList[i] + "\0" + "\0";
                fileOp.fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION;
                SHFileOperation(ref fileOp);
            }
        }
    }
}