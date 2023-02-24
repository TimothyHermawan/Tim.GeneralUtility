using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Tim.GeneralUtility
{
    public abstract class SingleInstance<T> : SingleInstance where T : MonoBehaviour
    {
        #region  Fields
        [CanBeNull]
        private static T _instance;

        [NotNull]
        // ReSharper disable once StaticMemberInGenericType
        private static readonly object Lock = new object();

        [SerializeField]
        private bool _persistent = true;
        #endregion

        #region  Properties
        [NotNull]
        public static T Instance
        {
            get
            {
                if (Quitting)
                {
                    Debug.LogWarning($"[{nameof(SingleInstance)}<{typeof(T)}>] Instance will not be returned because the application is quitting.");
                    // ReSharper disable once AssignNullToNotNullAttribute
                    return null;
                }
                lock (Lock)
                {
                    if (_instance != null)
                        return _instance;
                    var instances = FindObjectsOfType<T>();
                    var count = instances.Length;
                    if (count > 0)
                    {
                        if (count == 1)
                            return _instance = instances[0];
                        Debug.LogWarning($"[{nameof(SingleInstance)}<{typeof(T)}>] There should never be more than one {nameof(SingleInstance)} of type {typeof(T)} in the scene, but {count} were found. The first instance found will be used, and all others will be destroyed.");
                        for (var i = 1; i < instances.Length; i++)
                            Destroy(instances[i]);
                        return _instance = instances[0];
                    }

                    //Debug.Log($"[{nameof(SingleInstance)}<{typeof(T)}>] An instance is needed in the scene and no existing instances were found, so a new instance will be created.");
                    //_instance = new GameObject($"---------- {typeof(T)} ---------- ")
                    //           .AddComponent<T>();
                    //DontDestroyOnLoad(_instance.gameObject);
                    //_instance.transform.SetAsFirstSibling();
                    //return _instance;

                    return null;
                }
            }
        }
        #endregion

        #region  Methods
        private void ChangeGameObjectName()
        {
#if UNITY_EDITOR
            string longName = typeof(T).ToString();

            // SPLIT THE NAMESPACE AND FETCH THE ORIGINAL CLASS TYPE
            longName = longName.Split('.').ToList().Last();
            longName = GeneralUtility.AddSpaceBetweenCapitalLetters(longName);

            string newName = string.Format($"======== {longName}");
            gameObject.name = newName;
#endif
        }

        protected virtual void Awake()
        {
            if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (_persistent)
            {
                DontDestroyOnLoad(gameObject);
                ChangeGameObjectName();
            }

            PostAwake();
        }

        protected virtual void PostAwake()
        {
        }
        #endregion
    }

    public abstract class SingleInstance : MonoBehaviour
    {
        #region  Properties
        public static bool Quitting { get; private set; }
        #endregion

        #region  Methods
        private void OnApplicationQuit()
        {
            Quitting = true;
        }
        #endregion
    }
}