#region 注释
/*
 *     @File:          DownloadMgr
 *     @NameSpace:     ResourceMgr
 *     @Description:   DES
 *     @Author:        GrayWolfZ
 *     @Version:       0.1版本
 *     @Time:          2024年12月27日-10:06:35
 *     @Copyright  Copyright (c) 2023
 */
#endregion


using System;
using System.Collections;
using System.Collections.Generic;
using Config;
using UnityEngine;
using UnityEngine.Networking;
using Utils;
using VersionMgr;
using WeChatWASM;

namespace ResourceMgr
{
    public class DownloadMgr : MonoSingleton<DownloadMgr>
    {
        public int timeout = 5;

        public (Queue<ResourceInfo>, ResourceInfo) GetDownloadQueue(string abPath, Action<ResourceInfo> callback)
        {
            ResourceInfo resInfo = GetDownloadAbInfo(abPath);
            if (resInfo == null)
            {
                Debug.LogError("ERROR 未找到资源,请检查路径: " + abPath);
                callback?.Invoke(null);
                return (null, null);
            }
            List<AssetBundleInfo> depList = VersionLoaderMgr.Instance.GetDepListFormAbName(resInfo.GetAssetsFullName);
            Queue<ResourceInfo> downloadQueue = new Queue<ResourceInfo>();
            if (depList == null || depList.Count <= 0)
            {
                return (downloadQueue, resInfo);
            }
            foreach (AssetBundleInfo t in depList)
            {
                //检测是否已经下载过了
                ResourceInfo depResInfo = ResourceLoaderMgr.Instance.GetAbInfo(t.abNameNoHash);
                if (depResInfo != null)
                {
                    continue;
                }
                depResInfo = GetDownloadAbInfoFromAbNoHash(t.abNameNoHash);
                if (depResInfo != null)
                {
                    downloadQueue.Enqueue(depResInfo);
                }
            }
            return (downloadQueue, resInfo);
        }

        #region 常规下载Bundle
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <param name="progress"></param>
        public void DownloadFile(string url, Action<string, byte[]> callback = null, Action<ulong, float> progress = null)
        {
            StartCoroutine(DownloadToBytes(url, callback, progress));
        }

        public IEnumerator DownloadToBytes(string url, Action<string, byte[]> callback = null, Action<ulong, float> progress = null)
        {
            if (!url.Contains("file://") && !url.Contains("https:") && !url.Contains("http"))
            {
                url = $"file://{url}";
            }
            using UnityWebRequest web = UnityWebRequest.Get(url);
            web.timeout = timeout;
            if (url.Contains("https:"))
            {
                web.certificateHandler = new IgnoreHttps();
            }
            web.downloadHandler = new DownloadHandlerBuffer();
            UnityWebRequestAsyncOperation req = web.SendWebRequest();
            while (!req.isDone)
            {
                progress?.Invoke(web.downloadedBytes, web.downloadProgress);
                yield return null;
            }
            if (web.result == UnityWebRequest.Result.ConnectionError || web.result == UnityWebRequest.Result.ProtocolError)
            {
                callback?.Invoke(web.error, null);
                yield break;
            }
            byte[] downloadedData = web.downloadHandler.data;
            if (downloadedData == null || downloadedData.Length == 0)
            {
                callback?.Invoke("下载失败", null);
                yield break;
            }
            callback?.Invoke(null, downloadedData);
        }


        public void LoadAbFormNormal(string abPath, Action<ResourceInfo> callback)
        {
            AssetBundleInfo versionBundle = VersionLoaderMgr.Instance.GetDownloadUrl(abPath);
            if (versionBundle == null)
            {
                callback?.Invoke(null);
                return;
            }
            ResourceLoaderMgr.Instance.GetAssetBundle(versionBundle.GetAbName, (ab) =>
            {
                if (ab != null)
                {
                    callback?.Invoke(ab);
                }
                else
                {
                    (Queue<ResourceInfo>, ResourceInfo) queueInfo = GetDownloadQueue(abPath, callback);
                    Debug.Log("下载资源数量: " + queueInfo.Item1?.Count + "\t" + queueInfo.Item2?.GetAbName);
                    if (queueInfo.Item2 == null)
                    {
                        return;
                    }
                    if (queueInfo.Item1 == null)
                    {
                        StartCoroutine(DownloadAbFormNormal(queueInfo.Item2, callback));
                        return;
                    }
                    StartCoroutine(ProcessQueueNormal(queueInfo.Item1,
                        () => { StartCoroutine(DownloadAbFormNormal(queueInfo.Item2, callback)); }));
                }
            });
        }

        private IEnumerator ProcessQueueNormal(Queue<ResourceInfo> downloadQueue, Action callback)
        {
            while (downloadQueue.Count > 0)
            {
                ResourceInfo currentInfo = downloadQueue.Dequeue();
                yield return StartCoroutine(DownloadAbFormNormal(currentInfo));
            }
            callback?.Invoke();
        }

        public IEnumerator DownloadAbFormNormal(ResourceInfo resInfo, Action<ResourceInfo> callback = null, Action<ulong, float> progress = null)
        {
            string fullUrlPath = ProjectConfig.ResourceDownloadFullUrl + resInfo.GetAssetsFullName;
            Debug.Log("下载地址: " + fullUrlPath);
            using UnityWebRequest webRequest = new UnityWebRequest(fullUrlPath);
            webRequest.certificateHandler = new IgnoreHttps();
            DownloadHandlerAssetBundle handler = new DownloadHandlerAssetBundle(webRequest.uri.ToString(), 0);
            webRequest.downloadHandler = handler;
            if (fullUrlPath.Contains("https:"))
            {
                webRequest.certificateHandler = new IgnoreHttps();
            }
            UnityWebRequestAsyncOperation sendWebRequest = webRequest.SendWebRequest();
            while (!sendWebRequest.isDone)
            {
                progress?.Invoke(webRequest.downloadedBytes, webRequest.downloadProgress);
                yield return null;
            }
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(GetType() + "\t请求异常\t 状态:" + webRequest.result + "\t错误:" + webRequest.error);
                callback?.Invoke(null);
                yield break;
            }
            yield return SetResourceInfoHandler(webRequest, resInfo, callback);
        }
        #endregion

        #region Wx 资源加载Bundle
        public void LoadAb(string abPath, Action<ResourceInfo> callback)
        {
            AssetBundleInfo versionBundle = VersionLoaderMgr.Instance.GetDownloadUrl(abPath);
            if (versionBundle == null)
            {
                callback?.Invoke(null);
                return;
            }
            ResourceLoaderMgr.Instance.GetAssetBundle(versionBundle.GetAbName, (result) =>
            {
                if (result != null)
                {
                    callback?.Invoke(result);
                }
                else
                {
                    (Queue<ResourceInfo>, ResourceInfo) queueInfo = GetDownloadQueue(abPath, callback);
                    Debug.Log("下载资源数量: " + queueInfo.Item1?.Count + "\t" + queueInfo.Item2?.GetAbName);
                    if (queueInfo.Item2 == null)
                    {
                        return;
                    }
                    if (queueInfo.Item1 == null)
                    {
                        StartCoroutine(DownloadAb(queueInfo.Item2, callback));
                        return;
                    }
                    StartCoroutine(ProcessQueue(queueInfo.Item1, () => { StartCoroutine(DownloadAb(queueInfo.Item2, callback)); }));
                }
            });
        }

        private IEnumerator ProcessQueue(Queue<ResourceInfo> downloadQueue, Action callback)
        {
            while (downloadQueue.Count > 0)
            {
                ResourceInfo currentInfo = downloadQueue.Dequeue();
                yield return StartCoroutine(DownloadAb(currentInfo));
            }
            // 所有资源加载完成后调用回调
            callback?.Invoke();
        }

        public ResourceInfo GetDownloadAbInfoFromAbNoHash(string url)
        {
            AssetBundleInfo versionBundle = VersionLoaderMgr.Instance.GetDownloadUrlFromAbNameNoHash(url);
            if (versionBundle == null)
            {
                Debug.LogError("ERROR 未找到资源,请检查路径: " + url);
                return null;
            }
            ResourceInfo resourceInfo = new ResourceInfo();
            resourceInfo.SetVersionInfo(versionBundle);
            return resourceInfo;
        }

        public ResourceInfo GetDownloadAbInfo(string url)
        {
            AssetBundleInfo versionBundle = VersionLoaderMgr.Instance.GetDownloadUrl(url);
            if (versionBundle == null)
            {
                Debug.LogError("ERROR 未找到资源,请检查路径: " + url);
                return null;
            }
            ResourceInfo resourceInfo = new ResourceInfo();
            resourceInfo.SetVersionInfo(versionBundle);
            return resourceInfo;
        }


        public IEnumerator DownloadAb(ResourceInfo resInfo, Action<ResourceInfo> callback = null, Action<ulong, float> progress = null)
        {
            string fullUrlPath = ProjectConfig.ResourceDownloadFullUrl + resInfo.GetAssetsFullName;
            Debug.Log("下载地址: " + fullUrlPath);
            using UnityWebRequest webRequest = new UnityWebRequest(fullUrlPath);
            webRequest.timeout = timeout;
            DownloadHandlerWXAssetBundle handlerWxAssetBundle = new DownloadHandlerWXAssetBundle(webRequest.uri.ToString(), 0);
            webRequest.downloadHandler = handlerWxAssetBundle;
            if (fullUrlPath.Contains("https:"))
            {
                webRequest.certificateHandler = new IgnoreHttps();
            }
            UnityWebRequestAsyncOperation sendWebRequest = webRequest.SendWebRequest();
            while (!sendWebRequest.isDone)
            {
                progress?.Invoke(webRequest.downloadedBytes, webRequest.downloadProgress);
                yield return null;
            }
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(GetType() + "\t请求异常\t 状态:" + webRequest.result + "\t错误:" + webRequest.error);
                callback?.Invoke(null);
                yield break;
            }
            yield return SetResourceInfoHandler(webRequest, resInfo, callback);
        }
        #endregion


        public bool LoadFromMemory(DownloadHandler handler, ResourceInfo resInfo, Action<ResourceInfo> callback)
        {
            if (handler.data != null && handler.data.Length > 0)
            {
                resInfo.SetBytes(handler.data);
                resInfo.LoadFormMemory();
                ResourceLoaderMgr.Instance.SetToDict(resInfo.GetAbName, resInfo);
                callback?.Invoke(resInfo);
                return true;
            }
            return false;
        }


        public IEnumerator SetResourceInfoHandler(UnityWebRequest webRequest, ResourceInfo resInfo, Action<ResourceInfo> callback)
        {
            DownloadHandlerAssetBundle defaultBundle = null;
            DownloadHandlerWXAssetBundle wxBundle = null;
            if (webRequest.downloadHandler is DownloadHandlerAssetBundle assetBundle)
            {
                defaultBundle = assetBundle;
            }
            else if (webRequest.downloadHandler is DownloadHandlerWXAssetBundle wxAssetBundle)
            {
                wxBundle = wxAssetBundle;
            }
            if (null == defaultBundle && null == wxBundle)
            {
                Debug.Log(GetType() + "\t请求异常 is not DownloadHandlerWXAssetBundle\t" + webRequest.error);
                callback?.Invoke(null);
                yield break;
            }
            Debug.Log($"{GetType()}\t下载成功\t{webRequest.url}");
            AssetBundle ab = null;
            if (defaultBundle != null)
            {
                ab = DownloadHandlerAssetBundle.GetContent(webRequest);
                Debug.Log($"{GetType()}\t WxBundle \tab:{ab}\tName: {ab?.name}\t DataLength: {webRequest.downloadedBytes}");
                if (ab == null)
                {
                    ab = AssetBundle.LoadFromMemory(defaultBundle.data);
                }
                resInfo.IsDefault = true;
            }
            else
            {
                ab = wxBundle.assetBundle;
                Debug.Log($"{GetType()}\t WxBundle \tab:{ab}\tName: {ab?.name}\t DataLength: {webRequest.downloadedBytes}");
                if (ab == null)
                {
                    wxBundle.isDone = true;
                    ab = AssetBundle.LoadFromMemory(wxBundle.data);
                }
                resInfo.IsDefault = false;
            }
            Debug.Log($"{GetType()}\t当前 Bundle\t{ab?.name}\t");
            resInfo.SetAssetBundle(ab);
            ResourceLoaderMgr.Instance.SetToDict(resInfo.GetAbName, resInfo);
            callback?.Invoke(resInfo);
            yield return null;
        }
    }
}