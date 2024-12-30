#region 注释
/*
 *     @File:          GenerateAssetsBundleDefine
 *     @NameSpace:     Editor.BuildAb
 *     @Description:   DES
 *     @Author:        GrayWolfZ
 *     @Version:       0.1版本
 *     @Time:          2024年12月26日-16:00:32
 *     @Copyright  Copyright (c) 2023
 */
#endregion


using System.Collections.Generic;

namespace Editor.BuildAb
{
    public partial class GenerateAssetsBundleTag
    {
        public static string ReadlyRootPath = "";

        public static string ResPath = "Assets/Res/";

        //AbBundle 后缀
        public const string AbBundleSuffix = ".bundle";

        /// <summary>
        /// [filePath,abTag]
        /// </summary>
        public static Dictionary<string, string> TagUpdate = new Dictionary<string, string>(2048);

        private static readonly IReadOnlyList<string> SingleList = new List<string>
        {
            ".prefab", ".playable", ".shadergraph"
        };

        private static readonly IReadOnlyList<string> IgnoreSearchFolderName = new[]
        {
            ".vscode", "DS_Store",
        };

        private static readonly IReadOnlyDictionary<string, string> FileTypeAbTag = new Dictionary<string, string>
        {
            { "None", "_base" },
            { ".mat", "_mat" },
            { ".png", "_pic" },
            { ".jpg", "_pic" },
            { ".tga", "_pic" },
            { ".bmp", "_pic" },
            { ".controller", "_controller" },
            { ".anim", "_anim" },
            { ".fbx", "_fbx" },
            { ".hdr", "_hdr" },
            { ".mesh", "_mesh" },
            { ".asset", "_asset" },
            { ".mp3", "_audio" },
            { ".wav", "_audio" }
        };

        private static readonly IReadOnlyList<string> IgnoreExtensions = new[] { ".meta", ".tpsheet", ".spine", ".mp4", ".webm", "version.json" };

        private static readonly IReadOnlyList<string> ShaderFileExtension = new List<string>
        {
            ".shader", ".shadergraph"
        };

        private const string TexturePackageExtension = ".tpsheet";

        private const int MaxBundleNumber = 50;

        public struct SplitAssetsInfo
        {
            /// <summary>
            /// 资产后缀
            /// </summary>
            public string fileExtensions;

            /// <summary>
            /// 拆分计数下表
            /// </summary>
            public int splitIndex;

            /// <summary>
            /// 当前计数下表
            /// </summary>
            public int index;

            /// <summary>
            /// 总数量
            /// </summary>
            public int totalCount;

            /// <summary>
            /// 分裂资产规则标识
            /// </summary>
            public bool isOpenSplitBundle => totalCount > MaxBundleNumber;

            public override string ToString()
            {
                return $"{fileExtensions}\t{splitIndex}\t{totalCount}";
            }

            public bool IsEmpty()
            {
                return string.IsNullOrEmpty(fileExtensions) && totalCount == 0;
            }

            /// <summary>
            /// 根据规则计算分裂下表
            /// </summary>
            public void CalcSplitIndexFromMatch()
            {
                if (splitIndex * MaxBundleNumber == index)
                {
                    splitIndex++;
                }
            }
        }

        private static readonly IReadOnlyList<string> FileExtensionList = new List<string>
        {
            ".png",
            ".jpg",
            ".bmp",
            ".tga",
            ".mat",
            ".prefab",
            ".anim",
            ".controller",
            ".asset",
            ".txt",
            ".json",
            ".fbx",
            ".mp3",
            ".wav",
            ".hdr",
        };
    }
}