using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Game.Juice
{
    public class DamageNumber : MonoBehaviour
    {
        [SerializeField] private float lifetime = 1.0f;
        [SerializeField] private float floatSpeed = 1.0f;
        [SerializeField] private Color color = new Color(1f, 0.3f, 0.3f, 1f);

        private object _tmpComp;   // TextMeshPro component, if used
        private TextMesh _tmComp;  // Legacy TextMesh fallback
        private Renderer _renderer;

        public static void Spawn(Vector3 worldPos, float amount)
        {
            var cfg = JuiceConfig.Instance;
            float lt = cfg != null ? Mathf.Max(0.1f, cfg.dmgNumberLifetime) : 1.0f;

            // Try to load prefab from Resources if present
            GameObject prefab = Resources.Load<GameObject>("DamageNumber");
            GameObject go = prefab != null ? Instantiate(prefab) : new GameObject("DamageNumber");

            var dn = go.GetComponent<DamageNumber>();
            if (dn == null) dn = go.AddComponent<DamageNumber>();
            dn.lifetime = lt;
            dn.color = new Color(1f, 0.3f, 0.3f, 1f);
            dn.floatSpeed = 1.0f;

            go.transform.position = worldPos + Vector3.up * 1.5f;
            dn.SetupText(Mathf.RoundToInt(amount).ToString());
            dn.Begin();
        }

        private void SetupText(string text)
        {
            // Prefer TextMeshPro (world space)
            var tmpType = FindType("TMPro.TextMeshPro");
            if (tmpType != null)
            {
                var comp = GetComponent(tmpType) ?? gameObject.AddComponent(tmpType);
                _tmpComp = comp;
                SetTextTMP(_tmpComp, text);
                SetColorTMP(_tmpComp, color);
                _renderer = GetComponent<Renderer>();
                return;
            }

            // Fallback to built-in TextMesh
            _tmComp = GetComponent<TextMesh>();
            if (_tmComp == null) _tmComp = gameObject.AddComponent<TextMesh>();
            _tmComp.text = text;
            _tmComp.color = color;
            _tmComp.characterSize = 0.2f;
            _tmComp.alignment = TextAlignment.Center;
            _tmComp.anchor = TextAnchor.MiddleCenter;
            _renderer = _tmComp.GetComponent<Renderer>();
        }

        private void Begin()
        {
            // Always face camera if exists
            var cam = Camera.main;
            if (cam != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
            }
            StartCoroutine(CoFloatAndFade());
        }

        private IEnumerator CoFloatAndFade()
        {
            float t = 0f;
            Color start = color;
            while (t < lifetime)
            {
                float dt = Time.unscaledDeltaTime; // animate during hitstop
                t += dt;
                transform.position += Vector3.up * (floatSpeed * dt);

                float a = Mathf.Lerp(1f, 0f, t / lifetime);
                var c = new Color(start.r, start.g, start.b, a);
                if (_tmpComp != null) SetColorTMP(_tmpComp, c);
                if (_tmComp != null)
                {
                    _tmComp.color = c;
                }
                yield return null;
            }
            Destroy(gameObject);
        }

        private static void SetTextTMP(object tmp, string text)
        {
            try
            {
                var prop = tmp.GetType().GetProperty("text", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                prop?.SetValue(tmp, text);
            }
            catch { }
        }

        private static void SetColorTMP(object tmp, Color c)
        {
            try
            {
                var prop = tmp.GetType().GetProperty("color", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                prop?.SetValue(tmp, c);
            }
            catch { }
        }

        private static Type FindType(string fullName)
        {
            try
            {
                var t = Type.GetType(fullName, false);
                if (t != null) return t;
                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (a.IsDynamic) continue;
                    try
                    {
                        t = a.GetType(fullName, false);
                        if (t != null) return t;
                    }
                    catch { }
                }
            }
            catch { }
            return null;
        }
    }
}

