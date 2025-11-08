using System.Collections;
using UnityEngine;

namespace Game.Juice
{
    public class HitStop : MonoBehaviour
    {
        private static HitStop _instance;
        private static float _remainingRealtime;
        private static bool _running;
        private float _prevTimeScale = 1f;
        private float _prevFixedDeltaTime;

        private static HitStop Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("__HitStop");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<HitStop>();
                }
                return _instance;
            }
        }

        public static void Trigger(int milliseconds)
        {
            float duration = Mathf.Max(0f, milliseconds) / 1000f;
            if (duration <= 0f) return;

            // If already running, extend if needed
            if (_running)
            {
                _remainingRealtime = Mathf.Max(_remainingRealtime, duration);
                return;
            }

            Instance._prevTimeScale = Time.timeScale;
            Instance._prevFixedDeltaTime = Time.fixedDeltaTime;

            _remainingRealtime = duration;
            Instance.StartCoroutine(Instance.CoHitStop());
        }

        private IEnumerator CoHitStop()
        {
            _running = true;

            // Pause scaled time fully; use unscaled time to wait.
            Time.timeScale = 0f;
            // Keep physics step paused by leaving fixedDeltaTime as-is while timeScale = 0

            while (_remainingRealtime > 0f)
            {
                _remainingRealtime -= Time.unscaledDeltaTime;
                yield return null;
            }

            // Restore
            Time.timeScale = _prevTimeScale <= 0f ? 1f : _prevTimeScale;
            Time.fixedDeltaTime = _prevFixedDeltaTime > 0f ? _prevFixedDeltaTime : 0.02f;

            _running = false;
        }
    }
}

