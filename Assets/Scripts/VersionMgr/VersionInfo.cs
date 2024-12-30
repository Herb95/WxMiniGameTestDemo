#region 注释
/*
 *     @File:          VersionInfo
 *     @NameSpace:     VersionMgr
 *     @Description:   DES
 *     @Author:        GrayWolfZ
 *     @Version:       0.1版本
 *     @Time:          2024年12月27日-13:32:41
 *     @Copyright  Copyright (c) 2023
 */
#endregion


using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace VersionMgr
{
    [Serializable]
    public class VersionInfo
    {
        public string version;
        public List<AssetBundleInfo> abInfoList = new List<AssetBundleInfo>();

        public AssetBundleInfo GetAssetBundleInfo(string assetsPath)
        {
            // 通过路径查找 忽略大小写
            return abInfoList.Find(x => x.MatchInto(assetsPath));
        }


        public AssetBundleInfo GetAssetBundleInfoFromAbNameNoHash(string abNameNoHash)
        {
            // 通过路径查找 忽略大小写
            return abInfoList.Find(x => x.MatchInfoAbNoHash(abNameNoHash));
        }

        public void AddAssetBundleInfo(AssetBundleInfo info)
        {
            abInfoList.Add(info);
        }

        public void SaveToBuildApPath(string buildApPath)
        {
            string content = JsonConvert.SerializeObject(this, Formatting.Indented);
            Debug.Log("SaveToBuildApPath: " + buildApPath);
            File.WriteAllText(buildApPath, content);
        }

        public void Clean()
        {
            this.abInfoList.Clear();
        }

        public List<AssetBundleInfo> GetDepListFormAbName(string abName)
        {
            return abInfoList.FindAll(x => !string.IsNullOrEmpty(x.parentAbName) && x.parentAbName.Equals(abName));
        }
    }

    [Serializable]
    public class AssetBundleInfo
    {
        public string abName;
        public string abNameNoHash;
        public string hash;
        public string GetAbName => this.abNameNoHash;

        /// <summary>
        /// 父包名
        /// </summary>
        public string parentAbName;

        public List<string> assetsPathList = new List<string>();

        public bool MatchInto(string assetsPath)
        {
            if (this.assetsPathList == null || this.assetsPathList.Count == 0)
            {
                return false;
            }
            return this.assetsPathList.Find(x => x.Equals(assetsPath, StringComparison.OrdinalIgnoreCase)) != null;
        }

        public bool MatchInfoAbNoHash(string abNoHash)
        {
            if (string.IsNullOrEmpty(this.abNameNoHash) || string.IsNullOrEmpty(abNoHash))
            {
                return false;
            }
            return this.abNameNoHash.Equals(abNoHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}