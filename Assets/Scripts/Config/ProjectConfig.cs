#region 注释
/*
 *     @File:          ProjectConfig
 *     @NameSpace:     MiniGame.Config
 *     @Description:   项目配置
 *     @Author:        GrayWolfZ
 *     @Version:       0.1版本
 *     @Time:          2024年12月27日-14:46:02
 *     @Copyright  Copyright (c) 2023
 */
#endregion


namespace Config
{
    public class ProjectConfig
    {
        public const string CdnUrlRoot = "http://10.198.50.1/webpush/cdnGame/";
        public const string ResourceDownloadUrlPrefix = "StreamingAssets/src/";

        public const string VersionInfoFillName = "VersionInfo.json";

        /// <summary>
        /// 版本信息文件地址
        /// </summary>
        public const string VersionInfoUrl = CdnUrlRoot + ResourceDownloadUrlPrefix + VersionInfoFillName;

        /// <summary>
        /// 下载文件地址
        /// </summary>
        public const string ResourceDownloadFullUrl = CdnUrlRoot + ResourceDownloadUrlPrefix;

        // 本地资源缓存目录
        public const string LocalResourceCacheDir = "CachedResources/";
    }
}