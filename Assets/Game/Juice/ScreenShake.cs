using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Game.Juice
{
    public class ScreenShake : MonoBehaviour
    {
        private static ScreenShake _instance;
        private static Transform _camTf;
        private Vector3 _baseLocalPos;
        private Coroutine _co;

        private static ScreenShake Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("__ScreenShake");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<ScreenShake>();
                }
                return _instance;
            }
        }

        public static void Shake(float amplitude, float duration)
        {
            amplitude = Mathf.Max(0f, amplitude);
            duration = Mathf.Max(0f, duration);
            if (duration <= 0f || amplitude <= 0f) return;

            // Try Cinemachine impulse path first
            if (TryCinemachineImpulse(amplitude)) return;

            // Fallback: simple shake on main camera transform
            _camTf = Camera.main != null ? Camera.main.transform : _camTf;
            if (_camTf == null) return;
            Instance._baseLocalPos = _camTf.localPosition;
            if (Instance._co != null) Instance.StopCoroutine(Instance._co);
            Instance._co = Instance.StartCoroutine(Instance.CoShake(amplitude, duration));
        }

        private IEnumerator CoShake(float amplitude, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float damper = 1f - Mathf.Clamp01(t / duration);
                Vector2 offs = UnityEngine.Random.insideUnitCircle * amplitude * damper;
                if (_camTf != null)
                    _camTf.localPosition = _baseLocalPos + new Vector3(offs.x, offs.y, 0f);
                yield return null;
            }
            if (_camTf != null)
                _camTf.localPosition = _baseLocalPos;
            _co = null;
        }

        private static bool TryCinemachineImpulse(float amplitude)
        {
            try
            {
                // Find CinemachineImpulseSource type from loaded assemblies
                var cmsType = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .Select(a => {
                        try { return a.GetType("Cinemachine.CinemachineImpulseSource", false); }
                        catch { return null; }
                    })
                    .FirstOrDefault(t => t != null);

                if (cmsType == null) return false;

                // Find an instance in scene
                var objs = FindObjectsOfType(cmsType) as UnityEngine.Object[];
                if (objs == null || objs.Length == 0) return false;

                var src = objs[0];
                var m = cmsType.GetMethod("GenerateImpulse", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(float) }, null)
                        ?? cmsType.GetMethod("GenerateImpulse", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);

                if (m != null)
                {
                    if (m.GetParameters().Length == 1)
                        m.Invoke(src, new object[] { amplitude });
                    else
                        m.Invoke(src, null);
                    return true;
                }
            }
            catch
            {
                // ignore
            }
            return false;
        }
    }

    
}
