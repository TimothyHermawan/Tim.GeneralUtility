using UnityEngine;

#if UNITY_ADDRESSABLE
using UnityEngine.AddressableAssets;
#endif

namespace Tim.GeneralUtility
{
    public class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
    {
        private static T _instance;


#if UNITY_ADDRESSABLE

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Addressables.LoadAssetAsync<T>(typeof(T).Name).Result;
            }
            return _instance;
        }
    }

#else

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.LoadAsync<T>(typeof(T).Name).asset as T;
                }
                return _instance;
            }
        }

#endif
    }
}