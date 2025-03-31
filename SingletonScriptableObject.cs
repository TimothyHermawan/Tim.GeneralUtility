using UnityEngine;

using UnityEngine.AddressableAssets;

namespace Tim.GeneralUtility
{
    public class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
    {
        private static T _instance;


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

    }
}