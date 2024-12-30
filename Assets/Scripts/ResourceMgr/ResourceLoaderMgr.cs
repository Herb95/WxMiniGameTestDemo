#region 注释
/*
 *     @File:          ResourceLoaderMgr
 *     @NameSpace:     ResourceMgr
 *     @Description:   DES
 *     @Author:        GrayWolfZ
 *     @Version:       0.1版本
 *     @Time:          2024年12月26日-14:54:13
 *     @Copyright  Copyright (c) 2023
 */
#endregion


using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using WeChatWASM;

namespace ResourceMgr
{
    public class ResourceLoaderMgr : MonoSingleton<ResourceLoaderMgr>
    {
        public Dictionary<string, ResourceInfo> mAssetBundles = new Dictionary<string, ResourceInfo>();


        public void SetToDict(string abPath, ResourceInfo ab)
        {
            Debug.Log($"SetToDict: {abPath}\t {ab.GetAbName}");
            mAssetBundles[abPath] = ab;
        }

        /// <summary>
        /// 获取缓存资源包
        /// </summary>
        /// <param name="abPath">资源地址(无hash)</param>
        /// <returns></returns>
        public ResourceInfo GetAbInfo(string abPath)
        {
            return mAssetBundles.TryGetValue(abPath, out ResourceInfo bundle) ? bundle : null;
        }

        public void GetAssetBundle(string abPath, Action<ResourceInfo> callback = null)
        {
            callback?.Invoke(GetAbInfo(abPath));
        }


        /// <summary>
        /// 游戏异常时，使用本接口清理资源缓存
        /// </summary>
        /// <param name="action"></param>
        public void CleanDownloadCache(Action<bool> action = null)
        {
            WXBase.CleanAllFileCache(action);
        }

        public void LoadAssetBundle(string abPath, Action<ResourceInfo> callback)
        {
            DownloadMgr.Instance.LoadAb(abPath, callback);
        }

        public void LoadAssetBundleFormNormal(string abPath, Action<ResourceInfo> callback)
        {
            DownloadMgr.Instance.LoadAbFormNormal(abPath, callback);
        }
    }
}