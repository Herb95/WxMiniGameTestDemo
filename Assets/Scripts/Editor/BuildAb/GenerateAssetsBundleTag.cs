#region 注释
/*
 *     @File:          GenerateAssetsBundleTag
 *     @NameSpace:     Editor.BuildAb
 *     @Description:   生成AbTag
 *     @Author:        GrayWolfZ
 *     @Version:       0.1版本
 *     @Time:          2024年12月26日-15:59:03
 *     @Copyright  Copyright (c) 2023
 */
#endregion

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Editor.IOUtils;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Editor.BuildAb
{
    public partial class GenerateAssetsBundleTag
    {
        public static bool IsSingleFile(string fileName)
        {
            return SingleList.Any(fileName.EndsWith);
        }

        public static bool IgnoreCoderEditor(string projectName)
        {
            return projectName.Contains(".vscode") || projectName.Contains(".DS_Store") ||
                   projectName.Contains("DS_Store") || projectName.Contains(".idea") ||
                   projectName.Contains(".svn");
        }

        public static bool IgnoreProject(string projectName)
        {
            return projectName.Contains("ufw") || projectName.Contains("platform_u3d") ||
                   projectName.Contains("updater") || projectName.Contains("/cache") ||
                   IgnoreCoderEditor(projectName);
        }

        private static bool IsIgnoreSearchFolder(string searchPath)
        {
            return IgnoreSearchFolderName.Any(searchPath.Contains);
        }

        /// <summary>
        /// 根据文件名获得相应的AbTag后缀
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string FileToAbTagExtension(string fileName)
        {
            fileName = fileName.ToLower();
            foreach (KeyValuePair<string, string> kv in FileTypeAbTag.Where(kv => fileName.EndsWith(kv.Key)))
            {
                return kv.Value;
            }
            return FileTypeAbTag["None"];
        }

        #region GenerateTagForFolder
        /// <summary>
        /// 是否是Tp文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static bool IsTexturePackageFile(string filePath)
        {
            filePath = filePath.Replace('\\', '/');
            int fileIdx = filePath.LastIndexOf('.');
            if (fileIdx > 0)
            {
                string tmp = filePath.Substring(0, fileIdx) + TexturePackageExtension;
                if (EDirectoryUtils.IsFileExists(tmp))
                {
                    return true;
                }
            }
            return false;
        }


        public static void GenerateABTag_Set(string filePath, string abName, int splitIndex = 1)
        {
            string checkFile = filePath.ToLowerInvariant();
            if (IgnoreExtensions.Any(checkFile.EndsWith))
            {
                return;
            }
            filePath = filePath.Replace('\\', '/');
            if (IsTexturePackageFile(filePath))
            {
                abName = filePath;
                splitIndex = 1;
            }
            if (ShaderFileExtension.Any(filePath.EndsWith) && filePath.Contains(ReadlyRootPath))
            {
                abName = ReadlyRootPath + "/shaders";
            }
            else
            {
                int idx = abName.LastIndexOf('.');
                if (idx > 0)
                {
                    abName = abName.Substring(0, idx);
                }
            }
            string tag = abName.Replace(ResPath, "").Replace('\\', '/').Replace(" ", "").ToLower();
            tag = GetSplitAbName(tag, splitIndex);
            if (TagUpdate.ContainsKey(filePath) && TagUpdate[filePath] != tag)
            {
                Debug.LogWarning($"{filePath} old:{TagUpdate[filePath]} new:{tag}");
            }
            TagUpdate[filePath] = tag;
        }

        private static string GetSplitAbName(string abName, int splitIndex)
        {
            return splitIndex == 1 ? abName : abName + "_" + splitIndex;
        }


        /// <summary>
        /// 根据传入的目录设置Tag
        /// AbTag: xxx/xxx/a.bundle
        /// </summary>
        /// <param name="rootPath">资源路径</param>
        /// <param name="searchPattern">匹配字符</param>
        /// <param name="searchOption">匹配模式</param>
        public static void GenerateAbTag_FileToRootPath(string rootPath, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (!EDirectoryUtils.IsFolderExists(rootPath))
            {
                return;
            }
            var files = DirectoryUtils.GetFileListInPath(rootPath, searchPattern, searchOption);
            foreach (string file in files)
            {
                GenerateABTag_Set(file, rootPath.TrimEnd('/'));
            }
        }

        /// <summary>
        /// 根据传入的目录设置Tag
        /// AbTag: xxx/xxx/fileName.bundle
        /// </summary>
        /// <param name="rootPath">资源路径</param>
        /// <param name="searchPattern">匹配字符</param>
        /// <param name="searchOption">匹配模式</param>
        public static void GenerateAbTag_FileToFile(string rootPath, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (!EDirectoryUtils.IsFolderExists(rootPath))
            {
                return;
            }
            var files = DirectoryUtils.GetFileListInPath(rootPath, searchPattern, searchOption);
            foreach (string file in files)
            {
                GenerateABTag_Set(file, file);
            }
        }

        /// <summary>
        /// 生成UI目录下的资源 
        /// </summary>
        /// <param name="rootPath"></param>
        public static void GenerateABTag_UIs(string rootPath)
        {
            if (!EDirectoryUtils.IsFolderExists(rootPath))
            {
                return;
            }
            var dirs = DirectoryUtils.GetFolderListInPath(rootPath);
            foreach (var curDir in dirs)
            {
                var dir = curDir.Replace('\\', '/');
                if (dir.Contains("/activity"))
                {
                    //ui/activity 目录
                    var activityDirArray = DirectoryUtils.GetFolderListInPath(dir);
                    foreach (var activeDir in activityDirArray)
                    {
                        GenerateABTag_SubDir(activeDir);
                    }
                }
                else if (dir.Contains("/game"))
                {
                    // ui/game 目录
                    GenerateAbTag_FileToFile(dir, "*.prefab");
                    var gameDirArray = DirectoryUtils.GetFolderListInPath(dir);
                    foreach (var gameDir in gameDirArray)
                    {
                        GenerateABTag_SubDir(gameDir);
                    }
                }
                else if (dir.Contains("/bg"))
                {
                    GenerateAbTag_FileToFile(dir, "*.*", SearchOption.AllDirectories);
                }
                else
                {
                    GenerateABTag_SubDir(dir);
                }
            }
        }

        public static void GenerateABTag_SubDir(string rootPath)
        {
            if (!EDirectoryUtils.IsFolderExists(rootPath))
            {
                return;
            }
            //处理放在根目录的资源
            GenerateAbTag_SubSingle(rootPath);
            //处理当目录的子目录
            var dirs = DirectoryUtils.GetFolderListInPath(rootPath);
            foreach (var dir in dirs)
            {
                GenerateABTag_Dir(dir);
            }
        }


        public static void GenerateABTag_Dir(string rootPath, bool isGenerateFish = false,
            string searchPattern = "*.*", SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (!EDirectoryUtils.IsFolderExists(rootPath))
            {
                return;
            }
            var files = DirectoryUtils.GetFileListInPath(rootPath, searchPattern, searchOption);
            var noMetaFileList = files.FindAll(c => !IgnoreExtensions.Any(c.EndsWith));
            List<SplitAssetsInfo> splitAssetsInfoList = GetSplitListFromCurGenerateAbFileList(noMetaFileList);
            foreach (string childFile in noMetaFileList)
            {
                SplitAssetsInfo splitInfo = default;
                string file = childFile.Replace('\\', '/');
                if (splitAssetsInfoList != null && !IsTexturePackageFile(file))
                {
                    var findIndex = splitAssetsInfoList.FindIndex(c => file.ToLower().EndsWith(c.fileExtensions));
                    if (findIndex >= 0)
                    {
                        splitInfo = splitAssetsInfoList[findIndex];
                        if (splitInfo.isOpenSplitBundle)
                        {
                            splitInfo.CalcSplitIndexFromMatch();
                        }
                        splitInfo.index++;
                        splitAssetsInfoList[findIndex] = splitInfo;
                    }
                }
                if (IsSingleFile(file))
                {
                    GenerateABTag_Set(file, file);
                    if (isGenerateFish)
                    {
                        GenerateABTag_SubDirFromFish(rootPath, file);
                    }
                }
                else
                {
                    int splitIndex = 1;
                    if (!splitInfo.IsEmpty())
                    {
                        splitIndex = splitInfo.splitIndex;
                    }
                    GenerateABTag_Set(file, rootPath + FileToAbTagExtension(file), splitIndex);
                }
            }
        }

        /// <summary>
        /// 根据当前的资源列表拆分当前的拆分列表基础项
        /// </summary>
        /// <param name="assetsFileList"></param>
        /// <returns></returns>
        private static List<SplitAssetsInfo> GetSplitListFromCurGenerateAbFileList(List<string> assetsFileList)
        {
            if (assetsFileList == null || assetsFileList.Count == 0)
            {
                return null;
            }
            return (from t in FileExtensionList
                let findAllList = assetsFileList.FindAll(c => c.ToLower().EndsWith(t))
                where findAllList != null && findAllList.Count > 0
                select new SplitAssetsInfo()
                {
                    fileExtensions = t,
                    splitIndex = 1,
                    totalCount = findAllList.Count
                }).ToList();
        }


        public static void GenerateABTag_SubDirFromFish(string rootPath, string filePath, int splitIndex = 1)
        {
            if (filePath.Contains("/fish/"))
            {
                return;
            }
            var tmp = filePath.Substring(0, filePath.LastIndexOf('/'));
            var deps = AssetDatabase.GetDependencies(filePath);
            foreach (var dep in deps)
            {
                if (dep.Contains(tmp) && dep.ToLower().Contains("material"))
                {
                    GenerateABTag_Set(dep, rootPath + FileToAbTagExtension(dep), splitIndex);
                }
            }
        }


        /// <summary>
        /// 处理当前根目录的单文件
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        public static void GenerateAbTag_SubSingle(string rootPath, string searchPattern = "*.*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (!EDirectoryUtils.IsFolderExists(rootPath))
            {
                return;
            }
            GenerateABTag_Dir(rootPath, false, searchPattern, searchOption);
        }

        /// <summary>
        /// 处理子目录的子目录
        /// </summary>
        /// <param name="rootPath"></param>
        public static void GenerateABTag_SubSubDir(string rootPath)
        {
            if (!EDirectoryUtils.IsFolderExists(rootPath))
            {
                return;
            }
            GenerateAbTag_SubSingle(rootPath);
            List<string> dirs = DirectoryUtils.GetFolderListInPath(rootPath);
            if (dirs.Count >= 10 || rootPath.ToLowerInvariant().Contains("/prop/scene_high_common"))
            {
                GenerateABTag_SubSubDir(dirs);
                return;
            }
            foreach (var dir in dirs)
            {
                GenerateABTag_SubDir(dir);
            }
        }

        /// <summary>
        /// 文件夹子级的子级
        /// </summary>
        /// <param name="folderList"></param>
        public static void GenerateABTag_SubSubDir(List<string> folderList)
        {
            if (folderList == null || folderList.Count == 0)
            {
                return;
            }
            foreach (var dir in folderList)
            {
                GenerateABTag_SubSubDir(dir);
            }
        }


        public static void GenerateABTag_SubCommon(string rootPath, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (!EDirectoryUtils.IsFolderExists(rootPath))
            {
                return;
            }
            var dirs = DirectoryUtils.GetFolderListInPath(rootPath, searchPattern, searchOption);
            foreach (var d in dirs)
            {
                GenerateABTag_SubDir(d);
            }
        }


        public static void GenerateABTag_UpdateUI(string rootPath)
        {
            if (!EDirectoryUtils.IsFolderExists(rootPath))
            {
                return;
            }
            var dirs = DirectoryUtils.GetFolderListInPath(rootPath);
            foreach (var curDir in dirs)
            {
                GenerateABTag_SubDir(curDir.Replace('\\', '/'));
            }
            GenerateAbTag_SubSingle(rootPath);
        }

        /// <summary>
        /// /Art_Res
        /// -->Character
        /// -->Effect
        /// -->Prop
        /// </summary>
        /// <param name="rootPath"></param>
        public static void GenerateABTag_Art_Res(string rootPath)
        {
            if (!EDirectoryUtils.IsFolderExists(rootPath))
            {
                return;
            }
            List<string> dirs = DirectoryUtils.GetFolderListInPath(rootPath);
            foreach (string dir in dirs)
            {
                string folderPath = DirectoryUtils.UniformPath(dir);
                GenerateAbTag_SubSingle(folderPath);
                GenerateABTag_SubSubDir(DirectoryUtils.GetFolderListInPath(folderPath));
            }
        }
        #endregion


        public static void GenerateABTag_Spine()
        {
            var ids = AssetDatabase.FindAssets("t:material", new[] { "Assets/ThirdPart/Spine/Runtime" });
            foreach (var id in ids)
            {
                var sf = AssetDatabase.GUIDToAssetPath(id).Replace('\\', '/');
                GenerateABTag_Set(sf, "ufw/res/spine_mat");
            }
        }


        /// <summary>
        /// 生成Tag
        /// </summary>
        /// <param name="rootPath"></param>
        private static void GenerateAbTag(string rootPath)
        {
            if (!EDirectoryUtils.IsFolderExists(rootPath))
            {
                return;
            }
            GenerateAbTag_FileToFile(rootPath + "/scenes/", "*.unity", SearchOption.AllDirectories);
            GenerateAbTag_FileToRootPath(rootPath + "/fonts/");
            GenerateABTag_Art_Res(rootPath + "/Art_Res/");
            GenerateABTag_Art_Res(rootPath + "/RPGPP_LT/");
            GenerateAbTag_FileToRootPath(rootPath + "/Prefabs/", "*.*", SearchOption.AllDirectories);
        }

        /// <summary>
        /// 分析 Assets/Res 下所有的资源,处理ab
        /// </summary>
        public static void GenerateResPath()
        {
            string assetFullPath = ResPath.TrimEnd('/');
            ReadlyRootPath = assetFullPath;
            GenerateAbTag(assetFullPath);
        }

        /// <summary>
        /// 生成Tag
        /// </summary>
        public static void GenerateAbTag()
        {
            GenerateResPath();
            AssetsBundleBuildMap.SetAllAbTagToAbMap(TagUpdate);
        }
    }
}