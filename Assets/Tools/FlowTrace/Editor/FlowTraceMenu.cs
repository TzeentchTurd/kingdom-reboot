using System.IO;
using FlowTrace;
using Game.Battle;
using Game.Flow;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FlowTraceEditor
{
    public static class FlowTraceMenu
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";
        private const string BoardsFolder = "Assets/Boards";

        [MenuItem("Tools/FlowTrace/Run Battle Smoke")]
        public static void RunBattleSmoke()
        {
            // Clear previous trace log before starting
            BattleFlowTracer.ClearLogFile();
            EnsureBoardsFolder();
            var battlePrefab = EnsureBattleBoardPrefab();
            var eventPrefab = EnsureEventBoardPrefab();
            var barkPrefab = EnsureBarkCanvasPrefab();

            EnsureScene(battlePrefab, eventPrefab, barkPrefab);

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // Enter play mode
            EditorApplication.isPlaying = true;

            // Optionally nudge the battle load when play starts
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeChanged;
                // Try to trigger an early load (state machine will also load during BattleBoardState)
                var seq = UnityEngine.Object.FindFirstObjectByType<StageSequencer>();
                seq?.LoadBattleBoard();
            }
        }

        private static void EnsureBoardsFolder()
        {
            if (!AssetDatabase.IsValidFolder(BoardsFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Boards");
            }
        }

        private static GameObject EnsureBattleBoardPrefab()
        {
            var path = Path.Combine(BoardsFolder, "BattleBoard.prefab");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) return prefab;

            var go = new GameObject("BattleBoard");
            go.AddComponent<BattleBoard>();
            var text = go.AddComponent<TextMesh>();
            text.text = "Battle Board";
            text.anchor = TextAnchor.MiddleCenter;
            text.characterSize = 0.2f;
            go.transform.position = Vector3.zero;

            var created = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            AssetDatabase.SaveAssets();
            return created;
        }

        private static GameObject EnsureEventBoardPrefab()
        {
            var path = Path.Combine(BoardsFolder, "EventBoard.prefab");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) return prefab;

            var go = new GameObject("EventBoard");
            go.AddComponent<EventBoard>();
            var text = go.AddComponent<TextMesh>();
            text.text = "事件棋盘";
            text.anchor = TextAnchor.MiddleCenter;
            text.characterSize = 0.2f;
            go.transform.position = Vector3.zero;

            var created = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            AssetDatabase.SaveAssets();
            return created;
        }

        private static void EnsureScene(GameObject battlePrefab, GameObject eventPrefab, GameObject barkPrefab)
        {
            // If scene exists, ensure it has required objects and references
            if (File.Exists(ScenePath))
            {
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                EnsureSceneObjects(scene, battlePrefab, eventPrefab, barkPrefab);
                EditorSceneManager.SaveScene(scene);
                return;
            }

            // Create new minimal scene
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EnsureSceneObjects(newScene, battlePrefab, eventPrefab, barkPrefab);
            EditorSceneManager.SaveScene(newScene, ScenePath);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureSceneObjects(Scene scene, GameObject battlePrefab, GameObject eventPrefab, GameObject barkPrefab)
        {
            var root = FindOrCreate("GameRoot");
            var stageRoot = FindOrCreate("StageRoot");

            if (stageRoot.transform.parent != root.transform)
                stageRoot.transform.SetParent(root.transform);

            var sequencer = root.GetComponent<StageSequencer>();
            if (sequencer == null) sequencer = root.AddComponent<StageSequencer>();
            sequencer.StageRoot = stageRoot.transform;
            sequencer.BattleBoardPrefab = battlePrefab;
            sequencer.EventBoardPrefab = eventPrefab;

            var sm = root.GetComponent<BattleStateMachine>();
            if (sm == null) sm = root.AddComponent<BattleStateMachine>();

            // Ensure watchdog exists and enabled
            var watchdog = root.GetComponent<BattleSmokeWatchdog>();
            if (watchdog == null) watchdog = root.AddComponent<BattleSmokeWatchdog>();
            watchdog.enabled = true;

            // Ensure BarkCanvas exists in scene
            var bark = GameObject.Find("BarkCanvas");
            if (bark == null && barkPrefab != null)
            {
                bark = (GameObject)PrefabUtility.InstantiatePrefab(barkPrefab);
            }
            if (bark == null)
            {
                bark = new GameObject("BarkCanvas");
            }
            if (bark.GetComponent<Game.Dialogue.BarkPlayer>() == null)
            {
                bark.AddComponent<Game.Dialogue.BarkPlayer>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static GameObject EnsureBarkCanvasPrefab()
        {
            var path = Path.Combine("Assets/UI", "BarkCanvas.prefab");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                // Ensure it has BarkPlayer; if not, patch it
                if (prefab.GetComponent<Game.Dialogue.BarkPlayer>() == null)
                {
                    var temp = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    if (temp.GetComponent<Game.Dialogue.BarkPlayer>() == null)
                        temp.AddComponent<Game.Dialogue.BarkPlayer>();
                    PrefabUtility.SaveAsPrefabAsset(temp, path);
                    Object.DestroyImmediate(temp);
                    AssetDatabase.SaveAssets();
                    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                }
                return prefab;
            }

            var go = new GameObject("BarkCanvas");
            go.AddComponent<Canvas>();
            go.AddComponent<Game.Dialogue.BarkPlayer>();

            var created = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            AssetDatabase.SaveAssets();
            return created;
        }

        private static GameObject FindOrCreate(string name)
        {
            var go = GameObject.Find(name);
            if (go == null)
            {
                go = new GameObject(name);
            }
            return go;
        }
    }
}
