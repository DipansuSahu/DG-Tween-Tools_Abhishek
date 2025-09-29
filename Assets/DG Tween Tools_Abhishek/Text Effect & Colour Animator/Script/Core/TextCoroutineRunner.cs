using UnityEngine;

namespace AbhishekSahu.DGTweening
{
    /// <summary>
    /// Singleton coroutine runner for non-MonoBehaviour classes
    /// </summary>
    public class TextCoroutineRunner : MonoBehaviour
    {
        private static TextCoroutineRunner _instance;

        public static TextCoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("CoroutineRunner");
                    _instance = go.AddComponent<TextCoroutineRunner>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}