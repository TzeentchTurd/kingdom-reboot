using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FlowTrace;
using UnityEngine;

namespace Game.Dialogue
{
    public struct BarkContext
    {
        public string unit;
        public float dmg;
        public int essence;
    }

    public class BarkPlayer : MonoBehaviour
    {
        [SerializeField] private BarkBank bank;
        [SerializeField] private float cooldownSeconds = 2f;
        [SerializeField] private int maxConcurrent = 2;

        private readonly Dictionary<string, float> _lastPlay = new();
        private readonly List<GameObject> _active = new();
        private Transform _uiRoot;
        private bool _tmpAvailable;

        private static BarkPlayer _instance;
        public static BarkPlayer Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("BarkCanvas");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<BarkPlayer>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            EnsureUIRoot();
        }

        private void EnsureUIRoot()
        {
            if (_uiRoot != null) return;

            var canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            var scalerType = FindType("UnityEngine.UI.CanvasScaler");
            if (scalerType != null)
            {
                var scaler = gameObject.GetComponent(scalerType) ?? gameObject.AddComponent(scalerType);
                // ReferenceResolution / ScaleWithScreenSize via reflection (optional)
                TrySetProperty(scaler, "uiScaleMode", EnumValue(scalerType, "ScaleMode", "ScaleWithScreenSize"));
                TrySetProperty(scaler, "referenceResolution", new Vector2(1920, 1080));
            }

            _tmpAvailable = FindType("TMPro.TextMeshProUGUI") != null;

            // Root container
            var root = new GameObject("BarkRoot");
            root.transform.SetParent(transform, false);
            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -20);
            _uiRoot = root.transform;
        }

        public static void Play(string category, BarkContext ctx)
        {
            Instance.InternalPlay(category, ctx);
        }

        private void InternalPlay(string category, BarkContext ctx)
        {
            if (string.IsNullOrEmpty(category)) return;

            var now = Time.unscaledTime;
            if (_lastPlay.TryGetValue(category, out var last) && now - last < Mathf.Max(0.1f, cooldownSeconds))
            {
                return; // throttled
            }

            EnsureUIRoot();
            string text = SelectLine(category);
            if (string.IsNullOrEmpty(text)) return;

            text = ApplyPlaceholders(text, ctx);

            // Log trace
            BattleFlowTracer.Trace("Bark", text);

            // Enforce max concurrent
            while (_active.Count >= Mathf.Max(1, maxConcurrent))
            {
                var go = _active[0];
                _active.RemoveAt(0);
                if (go != null) Destroy(go);
            }

            var lineGo = CreateLineGO(text, _active.Count);
            _active.Add(lineGo);
            StartCoroutine(CoAnimateLine(lineGo));
            _lastPlay[category] = now;
        }

        private string SelectLine(string category)
        {
            if (bank == null)
            {
                // Try to load from asset path or resources
                bank = LoadBankAsset();
            }
            var list = category switch
            {
                "onUnitDowned" => bank?.onUnitDowned,
                "onExtractionComplete" => bank?.onExtractionComplete,
                "onBattleEnd" => bank?.onBattleEnd,
                _ => null
            };
            if (list == null || list.Count == 0) return null;
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        private static BarkBank LoadBankAsset()
        {
#if UNITY_EDITOR
            var bank = UnityEditor.AssetDatabase.LoadAssetAtPath<BarkBank>("Assets/Game/Dialogue/BarkBank.asset");
            if (bank != null) return bank;
#endif
            return Resources.Load<BarkBank>("BarkBank");
        }

        private string ApplyPlaceholders(string s, BarkContext ctx)
        {
            if (string.IsNullOrEmpty(s)) return s;
            try
            {
                s = s.Replace("{unit}", ctx.unit ?? "");
                s = s.Replace("{dmg}", ctx.dmg.ToString("0"));
                s = s.Replace("{essence}", ctx.essence.ToString());
            }
            catch { }
            return s;
        }

        private GameObject CreateLineGO(string text, int index)
        {
            var go = new GameObject($"BarkLine_{index}");
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(_uiRoot, false);
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -index * 28f);

            if (_tmpAvailable)
            {
                var tmpType = FindType("TMPro.TextMeshProUGUI");
                var comp = go.AddComponent(tmpType);
                TrySetProperty(comp, "text", text);
                TrySetProperty(comp, "fontSize", 24f);
                TrySetProperty(comp, "color", new Color(0.95f, 0.95f, 0.95f, 1f));
                return go;
            }

            // Fallback: worldspace TextMesh attached to camera top
            var cam = Camera.main;
            if (cam != null)
            {
                var world = new GameObject($"Bark3D_{index}");
                var tm = world.AddComponent<TextMesh>();
                tm.text = text;
                tm.characterSize = 0.15f;
                tm.color = new Color(0.95f, 0.95f, 0.95f, 1f);
                world.transform.SetParent(cam.transform, false);
                world.transform.localPosition = new Vector3(0, 1.8f - index * 0.2f, 2.5f);
                // Replace GO with world object for animation
                Destroy(go);
                return world;
            }
            return go;
        }

        private IEnumerator CoAnimateLine(GameObject go)
        {
            float t = 0f; float dur = 2.0f;
            var tmpType = FindType("TMPro.TextMeshProUGUI");
            bool hasTMP = tmpType != null && go.GetComponent(tmpType) != null;
            var tm = go.GetComponent<TextMesh>();
            var startPos = go.transform is RectTransform rt ? rt.anchoredPosition : Vector2.zero;

            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float a = 1f - Mathf.Clamp01(t / dur);
                if (hasTMP)
                {
                    var comp = go.GetComponent(tmpType);
                    TrySetProperty(comp, "color", new Color(0.95f, 0.95f, 0.95f, a));
                    var rtt = go.transform as RectTransform;
                    if (rtt != null) rtt.anchoredPosition = startPos + new Vector2(0, -10f * (t / dur));
                }
                else if (tm != null)
                {
                    tm.color = new Color(0.95f, 0.95f, 0.95f, a);
                    go.transform.localPosition += Vector3.up * 0.01f;
                }
                yield return null;
            }
            _active.Remove(go);
            Destroy(go);
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

        private static void TrySetProperty(object obj, string name, object value)
        {
            if (obj == null) return;
            try
            {
                var p = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null && p.CanWrite) p.SetValue(obj, value);
            }
            catch { }
        }

        private static object EnumValue(Type owner, string enumName, string valueName)
        {
            try
            {
                var nested = owner.GetNestedType(enumName);
                if (nested == null || !nested.IsEnum) return null;
                return Enum.Parse(nested, valueName);
            }
            catch { return null; }
        }
    }
}
