#region 注释
/*
 *     @File:          MonoSingleton
 *     @NameSpace:     Utils
 *     @Description:   DES
 *     @Author:        GrayWolfZ
 *     @Version:       0.1版本
 *     @Time:          2024年12月26日-14:59:57
 *     @Copyright  Copyright (c) 2023
 */
#endregion


using UnityEngine;

namespace Utils
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T instance = null;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType(typeof(T)) as T;
                    if (instance == null)
                    {
                        instance = new GameObject("_" + typeof(T).Name).AddComponent<T>();
                        DontDestroyOnLoad(instance);
                        CreateInstance();
                    }
                    if (instance == null)
                        Debug.LogError("Failed to create instance of " + typeof(T).FullName + ".");
                }
                return instance;
            }
        }

        void OnApplicationQuit()
        {
            instance = null;
            OnAppQuit();
        }

        protected virtual void OnAppQuit()
        {
        }

        public static T CreateInstance()
        {
            if (Instance != null) Instance.OnCreate();
            return Instance;
        }

        protected virtual void OnCreate()
        {
        }
    }
}