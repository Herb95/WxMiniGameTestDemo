#region 注释
/*
 *     @File:          Launcher
 *     @NameSpace:     MiniGame
 *     @Description:   启动界面
 *     @Author:        GrayWolfZ
 *     @Version:       0.1版本
 *     @Time:          2024年12月26日-15:22:41
 *     @Copyright  Copyright (c) 2023
 */
#endregion


using System;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using WeChatWASM;
using ResourceMgr;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using VersionMgr;

namespace MiniGame
{
    public class Launcher : MonoSingleton<Launcher>
    {
        public Button btnLogin;
        public Button btnLog;
        public Button btnTestPrefab;
        public Button btnLoadingScene;
        public SetEnableDebugOption debugOption;
        public GameObject loadingScenePrefab;

        public void Awake()
        {
            this.btnLogin = this.transform.Find("Btn_Login").GetComponent<Button>();
            this.btnLog = this.transform.Find("Btn_Log").GetComponent<Button>();
            this.btnTestPrefab = this.transform.Find("Btn_TestPrefab").GetComponent<Button>();
            this.btnLoadingScene = this.transform.Find("Btn_LoadingScene").GetComponent<Button>();
            Debug.Log("Launcher Awake");
            debugOption = new SetEnableDebugOption()
            {
                enableDebug = false,
            };
            ResourceLoaderMgr.Instance.CleanDownloadCache();
            VersionLoaderMgr.Instance.Init();
        }

        public void SetBtnEvent(Button btn, Action action)
        {
            btn?.onClick.RemoveAllListeners();
            btn?.onClick.AddListener(() => action?.Invoke());
        }

        private void OnEnable()
        {
            SetBtnEvent(this.btnLogin, this.OnLoginGameHandler);
            SetBtnEvent(this.btnLog, () =>
            {
                debugOption.enableDebug = !this.debugOption.enableDebug;
                WX.SetEnableDebug(debugOption);
            });
            SetBtnEvent(this.btnTestPrefab, this.OnTestPrefabHandler);
            SetBtnEvent(this.btnLoadingScene, this.OnLoadingSceneHandler);
        }

        private void OnLoadingSceneHandler()
        {
            // ResourceLoaderMgr.Instance.LoadAssetBundleFormNormal("Scenes/rpgpp_lt_scene_1.0.unity", (ab) =>
            // {
            //     if (ab != null)
            //     {
            //         SceneManager.LoadSceneAsync(ab.GetAssetsName, LoadSceneMode.Additive);
            //     }
            //     else
            //     {
            //         Debug.LogError("ERROR 未找到资源,请检查路径: Scenes/rpgpp_lt_scene_1.0.unity");
            //     }
            // });
            ResourceLoaderMgr.Instance.LoadAssetBundleFormNormal("Prefabs/LoadingScreen.prefab", (ab) =>
            {
                if (ab != null)
                {
                    GameObject go = ab.LoadAsset<GameObject>("LoadingScreen");
                    if (go != null)
                    {
                        this.loadingScenePrefab = Instantiate(go, this.transform);
                        RectTransform rt = this.loadingScenePrefab.GetComponent<RectTransform>();
                        this.loadingScenePrefab.transform.localScale = new Vector3(1, 1, 1);
                        rt.anchoredPosition = new Vector2(0, 0);
                    }
                    else
                    {
                        Debug.LogError("ERROR 未找到资源,请检查路径: Prefabs/LoadingScreen.prefab");
                    }
                }
                else
                {
                    Debug.LogError("ERROR 未找到资源,请检查路径: Prefabs/LoadingScreen.prefab");
                }
            });
        }

        private void OnTestPrefabHandler()
        {
            ResourceLoaderMgr.Instance.LoadAssetBundle("Prefabs/LoadingScreen.prefab", (ab) =>
            {
                if (ab != null)
                {
                    GameObject go = ab.LoadAsset<GameObject>("LoadingScreen");
                    if (go != null)
                    {
                        string trackStr = new System.Diagnostics.StackTrace().ToString();
                        this.loadingScenePrefab = Instantiate(go, this.transform);
                        Debug.Log($"{this.GetType()}\t{trackStr}\t{go}\t{this.transform}");
                        if (this.loadingScenePrefab == null)
                        {
                            Debug.LogError($"Error Instantiate failed:\t{go}");
                            return;
                        }
                        RectTransform rt = this.loadingScenePrefab.GetComponent<RectTransform>();
                        this.loadingScenePrefab.transform.localScale = new Vector3(1, 1, 1);
                        rt.anchoredPosition = new Vector2(0, 0);
                    }
                    else
                    {
                        Debug.LogError($"ERROR 加载Ab异常,{ab.GetAbName}");
                    }
                }
                else
                {
                    Debug.LogError($"ERROR 未找到资源,请检查路径: ab==Null");
                }
            });
        }

        private void OnLoginGameHandler()
        {
            WX.Login(new LoginOption()
            {
            });
            Debug.Log("Launcher OnStarGameHandler");
        }
    }
}