#region Single
/*
 *         Title: Single : Utils
 *         Description:
 *                功能：       单例基类
 *         Author:            GrayWolfZ
 *         Time:              2022年5月17日15:03:26
 *         Version:           0.1版本
 *         Modify Recode:
 */
#endregion

namespace Utils
{
    public class Single<T> where T : class, new()
    {
        private static T _instance;
        private static readonly object Padlock = new object();

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Padlock)
                    {
                        if (_instance == null)
                        {
                            _instance = new T();
                        }
                    }
                }
                return _instance;
            }
        }

        public virtual void OnDispose()
        {
            _instance = null;
        }
    }
}