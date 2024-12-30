#region 注释
/*
 *     @File:          ResourceConfig
 *     @NameSpace:     ResourceMgr
 *     @Description:   DES
 *     @Author:        GrayWolfZ
 *     @Version:       0.1版本
 *     @Time:          2024年12月27日-10:43:40
 *     @Copyright  Copyright (c) 2023
 */
#endregion


using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using VersionMgr;
using WeChatWASM;
using Object = UnityEngine.Object;

namespace ResourceMgr
{
    public class IgnoreHttps : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }

    public class ResourceInfo
    {
        private AssetBundleInfo mVersionBundleInfo;
        private AssetBundle mAssetBundle;
        private byte[] mAbBytes;
        public List<Object> mObjects = new List<Object>();

        public bool IsDefault { get; set; } = true;
        public string GetAssetsName => this.mVersionBundleInfo?.assetsPathList.FirstOrDefault();
        public string GetAssetsFullName => this.mVersionBundleInfo?.abName;
        public string GetAbName => this.mVersionBundleInfo?.abNameNoHash;

        public void SetVersionInfo(AssetBundleInfo versionBundle)
        {
            this.mVersionBundleInfo = versionBundle;
        }

        public void SetAssetBundle(AssetBundle ab)
        {
            this.mAssetBundle = ab;
            LoadAb();
        }

        private void LoadAb()
        {
            if (this.mAssetBundle == null)
            {
                return;
            }
            this.mObjects = this.mAssetBundle.LoadAllAssets().ToList();
            Debug.Log("加载资源数量: " + this.mObjects.Count);
        }

        public void LoadFormMemory()
        {
            this.mAssetBundle = AssetBundle.LoadFromMemory(this.mAbBytes);
            this.mObjects = this.mAssetBundle.LoadAllAssets().ToList();
            Debug.Log("加载资源数量: " + this.mObjects.Count);
        }

        private void Release()
        {
            if (IsDefault)
            {
                this.mAssetBundle?.Unload(false);
            }
            else
            {
                this.mAssetBundle?.WXUnload(false);
            }
            this.mVersionBundleInfo = null;
        }

        public void SetBytes(byte[] downloadHandlerData)
        {
            this.mAbBytes = downloadHandlerData;
        }

        public T LoadAsset<T>(string assetsName) where T : Object
        {
            if (this.mAssetBundle == null || this.mObjects == null || this.mObjects.Count == 0)
            {
                Debug.Log($"[加载] 未加载资源 \t{this.mAssetBundle == null}\t{this.mObjects?.Count}");
                return null;
            }
            foreach (Object obj in this.mObjects)
            {
                Debug.Log($"[加载]  {assetsName}\t {obj.name}\t{obj.GetType()}");
                if (obj.name.Equals(assetsName, StringComparison.CurrentCultureIgnoreCase) && obj.GetType().IsAssignableFrom(typeof(T)))
                {
                    return obj as T;
                }
            }
            T newObj = this.mAssetBundle.LoadAsset(assetsName, typeof(T)) as T;
            if (newObj != null)
            {
                this.mObjects.Add(newObj);
                return newObj;
            }
            Debug.LogError($"[加载] {assetsName}---->{typeof(T)}");
            return null;
        }
    }
}