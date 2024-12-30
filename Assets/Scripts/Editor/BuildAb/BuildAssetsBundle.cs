#region 注释
/*
 *     @File:          BuildAssetsBundle
 *     @NameSpace:     Editor.BuildAb
 *     @Description:   DES
 *     @Author:        GrayWolfZ
 *     @Version:       0.1版本
 *     @Time:          2024年12月26日-13:34:08
 *     @Copyright  Copyright (c) 2023
 */
#endregion


using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Utils;
using VersionMgr;

namespace Editor.BuildAb
{
    public class BuildAssetsBundle : UnityEditor.Editor
    {
        public static string AbPath => DirectoryUtils.GetCurProjectPath() + "AssetsBundle/src/";

        [MenuItem("Build/打包Ab", false, 0)]
        public static void Build()
        {
            GenerateAssetsBundleTag.GenerateAbTag();
            var buildAbList = AssetsBundleBuildMap.GetBuildAbBuilds();
            if (!Directory.Exists(AbPath))
            {
                Directory.CreateDirectory(AbPath);
            }
            else
            {
                DirectoryUtils.DeleteFolder(AbPath);
                Directory.CreateDirectory(AbPath);
            }
            BuildAssetBundleOptions buildOptions = BuildAssetBundleOptions.AppendHashToAssetBundleName
                                                   | BuildAssetBundleOptions.ChunkBasedCompression
                                                   | BuildAssetBundleOptions.DisableWriteTypeTree
                                                   | BuildAssetBundleOptions.None;
            //打包资源采集
            AssetBundleManifest bundleManifest = BuildPipeline.BuildAssetBundles(AbPath, buildAbList, buildOptions, BuildTarget.WebGL);
            GenerateVersionJson(bundleManifest, AbPath);
        }

        private static void GenerateVersionJson(AssetBundleManifest bundleManifest, string saveFolder)
        {
            string versionJson = saveFolder + "VersionInfo.json";
            if (bundleManifest == null)
            {
                Debug.LogError("[AbTag]-[GenerateVersionJson] bundleManifest is null");
                return;
            }
            VersionInfo info = new VersionInfo
            {
                version = "1"
            };
            string[] assetsBundleList = bundleManifest.GetAllAssetBundles();
            foreach (string assetsBundle in assetsBundleList)
            {
                Hash128 temp = bundleManifest.GetAssetBundleHash(assetsBundle);
                Debug.Log(assetsBundle + "\t " + temp);
                string abNameNoHash = assetsBundle.Replace("_" + temp.ToString(), "");
                Debug.Log(assetsBundle + "\t " + temp + "\t" + abNameNoHash);
                List<string> assetsList = GetFormatAssetsListFormAbName(abNameNoHash);
                AssetBundleInfo bundleInfo = new AssetBundleInfo
                {
                    hash = temp.ToString(),
                    abName = assetsBundle,
                    abNameNoHash = abNameNoHash,
                    assetsPathList = assetsList
                };
                info.AddAssetBundleInfo(bundleInfo);
                var depList = bundleManifest.GetDirectDependencies(assetsBundle);
                if (depList == null || depList.Length == 0)
                {
                    continue;
                }
                foreach (string depAssetBundle in depList)
                {
                    Hash128 depAb = bundleManifest.GetAssetBundleHash(depAssetBundle);
                    string depAbNoHash = depAssetBundle.Replace("_" + depAb.ToString(), "");
                    Debug.Log(depAssetBundle + "\t " + depAb + "\t" + depAbNoHash);
                    List<string> depAbAssetsList = GetFormatAssetsListFormAbName(depAbNoHash);
                    AssetBundleInfo depBundleInfo = new AssetBundleInfo
                    {
                        hash = temp.ToString(),
                        abName = assetsBundle,
                        parentAbName = bundleInfo.abName,
                        abNameNoHash = depAbNoHash,
                        assetsPathList = depAbAssetsList,
                    };
                    info.AddAssetBundleInfo(depBundleInfo);
                }
            }
            info.SaveToBuildApPath(versionJson);
        }

        /// <summary>
        /// 获取格式化后的资源列表
        /// </summary>
        /// <param name="abNameNoHash"></param>
        /// <returns></returns>
        public static List<string> GetFormatAssetsListFormAbName(string abNameNoHash)
        {
            List<string> list = AssetsBundleBuildMap.GetAssetsPathFromAbName(abNameNoHash);
            if (list == null || list.Count == 0)
            {
                return new List<string>(); // 返回空列表
            }
            return list.Select(t => t.Replace("\\", "/").Replace(GenerateAssetsBundleTag.ResPath, "")).ToList();
        }


        [MenuItem("Build/拷贝资源", false, 1)]
        public static void CopyResToStreamingAssets()
        {
            string dstPath = DirectoryUtils.GetCurProjectPath() + "Assets/StreamingAssets/src/";
            if (!Directory.Exists(dstPath))
            {
                Directory.CreateDirectory(dstPath);
            }
            else
            {
                DirectoryUtils.DeleteFolder(dstPath);
            }
            DirectoryUtils.CopyFolderTo(AbPath, dstPath, true);
        }
    }
}