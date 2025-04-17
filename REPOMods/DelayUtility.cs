using System;
using System.Collections;
using UnityEngine;

namespace OpJosModREPO
{
    public class DelayUtility : MonoBehaviour
    {
        private static DelayUtility _instance;

        public static DelayUtility Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject delayUtilityObject = new GameObject("DelayUtility");
                    _instance = delayUtilityObject.AddComponent<DelayUtility>();
                    DontDestroyOnLoad(delayUtilityObject);
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

        /// <summary>
        /// Runs a function after a delay
        /// </summary>
        /// <param name="seconds">Time in seconds</param>
        /// <param name="action">Function to execute after delay</param>
        public static void RunAfterDelay(float seconds, Action action)
        {
            Instance.StartCoroutine(Instance.DelayCoroutine(seconds, action));
        }

        private IEnumerator DelayCoroutine(float seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);
            action?.Invoke();
        }
    }
}
