#region AssetsBundleBuildMap
/*
 *         Title: AssetsBundleBuildMap : Ufw
 *         Description:
 *                功能：       资源映射
 *         Author:            GrayWolfZ
 *         Time:              2023年3月18日14:52:41
 *         Version:           0.1版本
 *         Modify Recode:
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Editor.IOUtils;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Editor.BuildAb
{
    public static class AssetsBundleBuildMap
    {
        private static AssetsBundleMap _assetBundleMap;

        public static AssetsBundleMap AssetsBundleMap
        {
            get
            {
                if (_assetBundleMap != null) return _assetBundleMap;
                _assetBundleMap = new AssetsBundleMap();
                _assetBundleMap = _assetBundleMap.Load($"{DirectoryUtils.GetCurProjectPath()}Logs/");
                return _assetBundleMap;
            }
        }

        public static void Clear(bool isRemoveFile = false)
        {
            if (_assetBundleMap is { })
            {
                _assetBundleMap.ClearData();
                _assetBundleMap = null;
            }
            if (!isRemoveFile)
            {
                return;
            }
            if (!Directory.Exists($"{DirectoryUtils.GetCurProjectPath()}Logs")) return;
            DirectoryUtils.DeleteFilesInFolder($"{DirectoryUtils.GetCurProjectPath()}Logs", AssetsBundleMap.MapAbFileName);
            DirectoryUtils.DeleteFilesInFolder($"{DirectoryUtils.GetCurProjectPath()}Logs", AssetsBundleMap.MapAbFormatFileName);
            DirectoryUtils.DeleteFilesInFolder($"{DirectoryUtils.GetCurProjectPath()}Logs", AssetsBundleMap.MapDepAbFileName);
            DirectoryUtils.DeleteFilesInFolder($"{DirectoryUtils.GetCurProjectPath()}Logs", AssetsBundleMap.MapDepAbFormatFileName);
            Debug.Log("[AbTag]-清理旧版本内容,删除历史存储内容.");
        }

        public static void SetAllAbTagToAbMap(Dictionary<string, string> abDict)
        {
            if (abDict == null || abDict.Count == 0)
            {
                Debug.LogWarning("[AbTag]-[SetAllAbTagToAbMap] 无内容信息");
                return;
            }
            foreach (KeyValuePair<string, string> item in abDict)
            {
                SetAssetsPathToAbName(item.Key, item.Value);
            }
            SaveData();
        }

        public static void SetAssetsPathToAbName(string assetsPath, string abName)
        {
            _assetBundleMap ??= new AssetsBundleMap();
            abName += GenerateAssetsBundleTag.AbBundleSuffix;
            _assetBundleMap.AddMap(assetsPath, abName);
        }

        public static void SaveData()
        {
            SetDepAssetsAbList();
        }

        public static void SetDepAssetsAbList()
        {
            if (AssetsBundleMap == null || AssetsBundleMap.GetAbTagList.Count == 0)
            {
                Debug.LogError("[AbTag]-[SetDepAssetsAbList]--->内容不存在");
                return;
            }
            AssetsBundleMap.SetAllDepAbList();
        }

        public static bool IsAssetsPathInAbName(string assetsPath)
        {
            return _assetBundleMap?.IsContainsAssets(assetsPath) ?? false;
        }

        public static List<string> GetAllAssetsBundleName()
        {
            if (_assetBundleMap == null)
            {
                return null;
            }
            List<string> allAbName = _assetBundleMap.GetAbTagList;
            allAbName.Sort(EditorUtility.NaturalCompare);
            return allAbName;
        }

        public static List<string> GetAssetsPathFromAbName(string abName)
        {
            return AssetsBundleMap.GetAssetsPathFromAbTag(abName);
        }

        /// <summary>
        /// 获取最终打包资源
        /// </summary>
        /// <returns></returns>
        public static AssetBundleBuild[] GetBuildAbBuilds()
        {
            List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();
            assetBundleBuilds.AddRange(AssetsBundleMap.GetAllBundle());
            return assetBundleBuilds.ToArray();
        }

        /// <summary>
        /// 获取 ab 的直接依赖
        /// </summary>
        /// <param name="abName"></param>
        /// <returns></returns>
        public static string[] GetDirectDependencies(string abName)
        {
            return AssetsBundleMap?.GetAbDirectDependencies(abName);
        }
    }

    [Serializable]
    public class AbEntry
    {
        public string assetsPath = string.Empty;
        public string aBName = string.Empty;

        /// <summary>
        /// 当前资源的直接依赖
        /// </summary>
        public List<string> AbDepList
        {
            get
            {
                return AssetDatabase.GetDependencies(assetsPath, false)
                    .Where(dep => !dep.Equals(assetsPath))
                    .ToList();
            }
        }

        /// <summary>
        /// 依赖-->abName 
        /// </summary>
        public Dictionary<string, string> depAbList = new Dictionary<string, string>();

        public void AddDepAbDict(string abPath, string abName)
        {
            if (depAbList.TryGetValue(abPath, out string curAbName))
            {
                if (!string.IsNullOrEmpty(curAbName))
                {
                    Debug.LogWarning($"AbPath: {abPath}--->当前AbName: {curAbName}--->传入abName {abName}");
                }
                depAbList[abPath] = abName;
            }
            else
            {
                depAbList[abPath] = abName;
            }
        }

        public string[] GetDepArray()
        {
            if (depAbList.Count == 0)
            {
                return Array.Empty<string>();
            }
            List<string> list = new List<string>();
            List<string> valueList = depAbList.Values.Distinct().ToList();
            foreach (string value in valueList)
            {
                if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(list.Find(c => c.Equals(value))))
                {
                    list.Add(value);
                }
            }
            return list.ToArray();
        }
    }

    public class AssetsBundleMap
    {
        private readonly HashSet<string> _assetsPathSet = new HashSet<string>();
        private Dictionary<string, List<string>> _assetsBundleDict = new Dictionary<string, List<string>>();
        private List<AbEntry> _abDepList = new List<AbEntry>();
        public static readonly string MapAbFileName = $"{nameof(AssetsBundleBuildMap)}.log";
        public static readonly string MapAbFormatFileName = $"{nameof(AssetsBundleBuildMap)}2.log";
        public static readonly string MapDepAbFileName = $"{nameof(AssetsBundleBuildMap)}_Dep.log";
        public static readonly string MapDepAbFormatFileName = $"{nameof(AssetsBundleBuildMap)}_Dep2.log";
        public List<string> GetAbTagList => _assetsBundleDict.Keys.ToList();

        public bool IsContainsAssets(string findAssets)
        {
            return _assetsPathSet.Contains(findAssets);
        }

        public override string ToString()
        {
            return "[AbMap] ";
        }


        public List<string> GetAssetsPathFromAbTag(string abTag)
        {
            _assetsBundleDict.TryGetValue(abTag, out List<string> list);
            if (list == null)
            {
                return null;
            }
            list.Sort(EditorUtility.NaturalCompare);
            return list;
        }

        /// <summary>
        /// 根据资源路径 获取Ab名
        /// </summary>
        /// <param name="assetsPath"></param>
        /// <returns></returns>
        public string GetAssetsPathToAbName(string assetsPath)
        {
            if (string.IsNullOrEmpty(assetsPath) || _assetsBundleDict == null)
            {
                return string.Empty;
            }
            foreach (var kv in _assetsBundleDict)
            {
                if (kv.Value.Find(c => c.Equals(assetsPath)) != null)
                {
                    return kv.Key;
                }
            }
            return string.Empty;
        }

        public void AddMap(string assetsPath, string assetsBundleName)
        {
            if (string.IsNullOrEmpty(assetsPath) || string.IsNullOrEmpty(assetsBundleName)) return;
            if (!_assetsPathSet.Add(assetsPath))
            {
                Debug.Log($"重复Key: {assetsPath}");
                return;
            }

            // Debug.LogInfo($"[BuildMap] --->Path:{assetsPath}--->BundleName:{assetsBundleName}");
            if (_assetsBundleDict.TryGetValue(assetsBundleName, out List<string> list))
            {
                if (!list.Contains(assetsPath))
                {
                    list.Add(assetsPath);
                }
            }
            else
            {
                _assetsBundleDict.Add(assetsBundleName, new List<string>() { assetsPath });
            }
            if (_abDepList.Find(c => c.assetsPath.Equals(assetsPath)) == null)
            {
                AbEntry abEntry = new AbEntry
                {
                    assetsPath = assetsPath,
                    aBName = assetsBundleName
                };
                _abDepList.Add(abEntry);
            }
            else
            {
                Debug.LogError($"abDepList-->重复Key: {assetsPath}");
            }
        }

        /// <summary>
        /// 获取 依赖所有的Ab
        /// </summary>
        public void SetAllDepAbList()
        {
            if (_abDepList.Count == 0)
            {
                Debug.LogError("依赖数据不存在,请检查内容!!!!");
                return;
            }
            foreach (AbEntry entry in _abDepList)
            {
                List<string> list = entry.AbDepList;
                if (list == null || list.Count == 0)
                {
                    continue;
                }
                for (int i = 0; i < list.Count; i++)
                {
                    string depPath = list[i];
                    if (depPath.Contains(".cs") || depPath.Contains(".hlsl") || depPath.Contains(".shadergraph"))
                    {
                        continue;
                    }
                    string abName = GetAssetsPathToAbName(list[i]);
                    entry.AddDepAbDict(list[i], abName);
                }
            }
        }

        public string[] GetAbDirectDependencies(string abName)
        {
            if (_abDepList.Count == 0)
            {
                Debug.LogError("依赖数据不存在,请检查内容!!!!");
                return Array.Empty<string>();
            }
            return _abDepList.Find(c => c.aBName.Equals(abName))?.GetDepArray() ?? Array.Empty<string>();
        }


        public List<AssetBundleBuild> GetAllBundle()
        {
            List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();
            foreach (var item in _assetsBundleDict)
            {
                assetBundleBuilds.Add(new AssetBundleBuild()
                {
                    assetBundleName = item.Key,
                    assetNames = item.Value.ToArray(),
                });
            }
            return assetBundleBuilds;
        }


        public AssetsBundleMap Load(string tmpPath)
        {
            string filePath = tmpPath + $"{MapAbFileName}";
            if (!EDirectoryUtils.IsFileExists(filePath))
            {
                Debug.LogError($"文件路径不存在  {filePath}");
                return new AssetsBundleMap();
            }
            AssetsBundleMap map = new AssetsBundleMap();
            try
            {
                byte[] bytes = File.ReadAllBytes(tmpPath + $"{MapAbFileName}");
                using MemoryStream stream = new MemoryStream(bytes);
                _assetsBundleDict = new BinaryFormatter().Deserialize(stream) as Dictionary<string, List<string>>;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AbMap] Path:{tmpPath}\t FileName: {MapAbFileName}\t Error: {e.Message}");
            }
            if (EDirectoryUtils.IsFileExists(tmpPath + $"{MapDepAbFileName}"))
            {
                try
                {
                    _abDepList.Clear();
                    byte[] depBytes = File.ReadAllBytes(tmpPath + $"{MapDepAbFileName}");
                    using MemoryStream depStream = new MemoryStream(depBytes);
                    _abDepList = new BinaryFormatter().Deserialize(depStream) as List<AbEntry>;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[AbMap] Path:{tmpPath}{MapDepAbFileName} {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"[AbMap] Path:{tmpPath}{MapDepAbFileName} 文件不存在");
            }
            return map;
        }

        public void ClearData()
        {
            this._assetsBundleDict.Clear();
            this._assetsPathSet.Clear();
            this._abDepList.Clear();
        }
    }
}