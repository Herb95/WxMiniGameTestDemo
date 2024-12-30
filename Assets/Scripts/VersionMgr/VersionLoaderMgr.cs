#region 注释
/*
 *     @File:          VersionLoaderMgr
 *     @NameSpace:     VersionMgr
 *     @Description:   版本管理器
 *     @Author:        GrayWolfZ
 *     @Version:       0.1版本
 *     @Time:          2024年12月27日-14:43:52
 *     @Copyright  Copyright (c) 2023
 */
#endregion


using System.Collections.Generic;
using Config;
using Newtonsoft.Json;
using ResourceMgr;
using UnityEngine;
using Utils;

namespace VersionMgr
{
    public class VersionLoaderMgr : MonoSingleton<VersionLoaderMgr>
    {
        public VersionInfo versionInfo = new VersionInfo();

        /// <summary>
        /// 初始化本吧呢
        /// </summary>
        public void Init()
        {
            versionInfo.Clean();
            DownloadMgr.Instance.DownloadFile(ProjectConfig.VersionInfoUrl, (result, downloadedData) =>
            {
                if (!string.IsNullOrEmpty(result))
                {
                    Debug.Log("下载版本信息失败,请检查日志: " + ProjectConfig.VersionInfoUrl);
                    return;
                }
                string resultString = System.Text.Encoding.UTF8.GetString(downloadedData);
                versionInfo = JsonConvert.DeserializeObject<VersionInfo>(resultString);
                Debug.Log("下载版本信息成功: " + versionInfo?.version);
            });
        }

        public AssetBundleInfo GetDownloadUrl(string assetsPath)
        {
            return versionInfo.GetAssetBundleInfo(assetsPath);
        }

        public AssetBundleInfo GetDownloadUrlFromAbNameNoHash(string assetsPath)
        {
            return versionInfo.GetAssetBundleInfoFromAbNameNoHash(assetsPath);
        }

        public List<AssetBundleInfo> GetDepListFormAbName(string abName)
        {
            return versionInfo.GetDepListFormAbName(abName);
        }
    }
}