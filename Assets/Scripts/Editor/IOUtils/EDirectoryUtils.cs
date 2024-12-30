#region EDirectoryUtils
/*
 *    Title: Disciple: U3dFishingNewResManager
 *    Description    : IO 工具类
 *    Author         : GrayWolfZ
 *    Time           : 2022/05/19 14:42:41
 *    Version        : 0.1
 *    Modify Recorder:
 *
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Editor.IOUtils
{
    public class EDirectoryUtils
    {
        public static bool IsFileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// 获取所有选择的资源路径
        /// </summary>
        /// <param name="selectionMode"></param>
        /// <returns></returns>
        public static List<string> GetSelectAssets(SelectionMode selectionMode = SelectionMode.TopLevel)
        {
            List<string> resultList = new List<string>();
            Object[] arrList = Selection.GetFiltered(typeof(Object), selectionMode);
            for (int i = 0; i < arrList.Length; i++)
            {
                string path = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')) + "/" +
                              AssetDatabase.GetAssetPath(arrList[i]);
                resultList.Add(path);
            }
            return resultList;
        }

        /// <summary>
        /// 获取唯一选中的文件夹
        /// </summary>
        /// <returns></returns>
        public static string GetSelectFolder()
        {
            Object[] arrList = Selection.GetFiltered(typeof(Object), SelectionMode.TopLevel);
            if (arrList == null || arrList.Length <= 0)
            {
                return "";
            }
            if (arrList.Length > 1)
            {
                Debug.Log("选中了多个项目");
                return "";
            }
            string path = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')) + "/" +
                          AssetDatabase.GetAssetPath(arrList[0]);
            return Directory.Exists(path) ? path : "";
        }

        /// <summary>
        /// 获取当前打开的场景目录
        /// </summary>
        /// <returns></returns>
        public static string GetOpenScenePath()
        {
            return SceneManager.GetActiveScene().path;
        }


        /// <summary>
        /// 获取文件夹下的所有目录
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="pattern"></param>
        /// <param name="option"></param>
        /// <param name="containSelf"></param>
        /// <returns></returns>
        public static List<FileInfo> GetFileListInPath(string dirPath, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly,
            bool containSelf = false)
        {
            DirectoryInfo d = new DirectoryInfo(dirPath);
            if (!d.Exists)
            {
                return null;
            }
            List<FileInfo> result = new List<FileInfo>(d.GetFiles(pattern, option));
            if (containSelf)
            {
                result.Add(new FileInfo(dirPath));
            }
            return result;
        }


        /// <summary>
        /// 是否是文件夹路径，如果存在则返回 true，否则返回 false。
        /// </summary>
        /// <param name="assetsPath"></param>
        /// <returns>bool</returns>
        public static bool IsAssetsIsFolder(string assetsPath)
        {
            return AssetDatabase.IsValidFolder(assetsPath);
        }

        /// <summary>
        /// 检查目录是否存在
        /// </summary>
        /// <param name="rootPath"></param>
        /// <returns></returns>
        public static bool IsFolderExists(string rootPath)
        {
            if (Directory.Exists(rootPath)) return true;
            Debug.LogWarning($"{rootPath}");
            return false;
        }

        /// <summary>
        /// 无论目录是否存在都删除所有文件重新创建一个目录
        /// </summary>
        public static void RecreateSpecificFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
            Directory.CreateDirectory(folderPath);
        }
    }
}