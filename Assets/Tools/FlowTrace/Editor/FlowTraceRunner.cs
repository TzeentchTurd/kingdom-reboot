using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Provides Tools/FlowTrace/Run Linear Progression Smoke
// Opens Main.unity, enters Play, runs level 0 battle, logs result to persistentDataPath/battle_trace.log
// Always writes a line containing BATTLE_END; on timeout also writes timeout_30s
namespace Tools.FlowTrace
{
    [InitializeOnLoad]
    public static class FlowTraceBootstrap
    {
        static FlowTraceBootstrap()
        {
            if (SessionState.GetBool(FlowTraceRunner.SessionActiveKey, false))
            {
                FlowTraceRunner.SubscribeUpdate();
            }
        }
    }

    public static class FlowTraceRunner
    {
        internal const string SessionActiveKey = "FlowTrace_SmokeActive";
        private const string SessionStepKey = "FlowTrace_Step";
        private const string SessionEndTimeKey = "FlowTrace_EndTime";
        private const string SessionLogPathKey = "FlowTrace_LogPath";
        private const string SessionScenePathKey = "FlowTrace_ScenePath";

        private enum Step
        {
            Idle = 0,
            OpenScene = 1,
            RequestPlayMode = 2,
            WaitPlayMode = 3,
            WaitFlow = 4,
            InitLevel = 5,
            StartBattle = 6,
            Monitor = 7,
            Finish = 8,
            StopPlay = 9,
            Done = 10
        }

        [MenuItem("Tools/FlowTrace/Run Linear Progression Smoke")]
        public static void RunSmoke()
        {
            if (SessionState.GetBool(SessionActiveKey, false))
            {
                Debug.LogWarning("FlowTrace smoke already running.");
                return;
            }

            string scenePath = "Assets/Scenes/Main.unity";
            string logPath = Path.Combine(Application.persistentDataPath, "battle_trace.log");

            try
            {
                EditorSceneManager.OpenScene(scenePath);
            }
            catch (Exception e)
            {
                WriteLogSafe(logPath, $"BATTLE_END(error_open_scene): {e.Message}");
                Debug.LogError($"FlowTrace: Failed to open scene {scenePath}: {e}");
                return;
            }

            SessionState.SetBool(SessionActiveKey, true);
            SessionState.SetInt(SessionStepKey, (int)Step.RequestPlayMode);
            SessionState.SetString(SessionLogPathKey, logPath);
            SessionState.SetString(SessionScenePathKey, scenePath);

            SubscribeUpdate();
        }

        public static void SubscribeUpdate()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            if (!SessionState.GetBool(SessionActiveKey, false))
            {
                EditorApplication.update -= Update;
                return;
            }

            var step = (Step)SessionState.GetInt(SessionStepKey, (int)Step.Idle);
            string logPath = SessionState.GetString(SessionLogPathKey, Path.Combine(Application.persistentDataPath, "battle_trace.log"));

            try
            {
                switch (step)
                {
                    case Step.RequestPlayMode:
                        EditorApplication.isPlaying = true;
                        SessionState.SetInt(SessionStepKey, (int)Step.WaitPlayMode);
                        break;

                    case Step.WaitPlayMode:
                        if (EditorApplication.isPlaying && !EditorApplication.isPaused)
                        {
                            SessionState.SetInt(SessionStepKey, (int)Step.WaitFlow);
                        }
                        break;

                    case Step.WaitFlow:
                        if (GameFlowManager.Instance != null)
                        {
                            SessionState.SetInt(SessionStepKey, (int)Step.InitLevel);
                        }
                        break;

                    case Step.InitLevel:
                        {
                            var flow = GameFlowManager.Instance;
                            if (flow == null) { SessionState.SetInt(SessionStepKey, (int)Step.WaitFlow); break; }
                            flow.TransitionToLevel(0);
                            // Give one frame for build
                            SessionState.SetInt(SessionStepKey, (int)Step.StartBattle);
                        }
                        break;

                    case Step.StartBattle:
                        {
                            var flow = GameFlowManager.Instance;
                            if (flow == null) { SessionState.SetInt(SessionStepKey, (int)Step.WaitFlow); break; }
                            flow.ChangeState(GameState.Battle);
                            double endTime = EditorApplication.timeSinceStartup + 30.0;
                            SessionState.SetString(SessionEndTimeKey, endTime.ToString("R"));
                            SessionState.SetInt(SessionStepKey, (int)Step.Monitor);
                        }
                        break;

                    case Step.Monitor:
                        {
                            var flow = GameFlowManager.Instance;
                            double endTime = ParseDouble(SessionState.GetString(SessionEndTimeKey, "0"));
                            bool timedOut = EditorApplication.timeSinceStartup >= endTime;

                            if (flow != null && flow.CurrentState == GameState.Victory)
                            {
                                WriteLogSafe(logPath, "BATTLE_END(ok)");
                                SessionState.SetInt(SessionStepKey, (int)Step.Finish);
                            }
                            else if (timedOut)
                            {
                                // Write both timeout marker and BATTLE_END to satisfy acceptance
                                WriteLogSafe(logPath, "timeout_30s\nBATTLE_END(timeout_30s)");
                                SessionState.SetInt(SessionStepKey, (int)Step.Finish);
                            }
                        }
                        break;

                    case Step.Finish:
                        SessionState.SetInt(SessionStepKey, (int)Step.StopPlay);
                        break;

                    case Step.StopPlay:
                        if (EditorApplication.isPlaying)
                        {
                            EditorApplication.isPlaying = false;
                        }
                        SessionState.SetInt(SessionStepKey, (int)Step.Done);
                        break;

                    case Step.Done:
                        CleanupSession();
                        break;
                }
            }
            catch (Exception e)
            {
                WriteLogSafe(logPath, $"BATTLE_END(error_exception): {e.Message}");
                CleanupSession();
                if (EditorApplication.isPlaying)
                    EditorApplication.isPlaying = false;
                Debug.LogError($"FlowTrace exception: {e}");
            }
        }

        private static void CleanupSession()
        {
            SessionState.EraseBool(SessionActiveKey);
            SessionState.EraseInt(SessionStepKey);
            SessionState.EraseString(SessionEndTimeKey);
            SessionState.EraseString(SessionLogPathKey);
            SessionState.EraseString(SessionScenePathKey);
            EditorApplication.update -= Update;
        }

        private static double ParseDouble(string s)
        {
            if (double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var d))
                return d;
            return 0;
        }

        private static void WriteLogSafe(string path, string content)
        {
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(path, content);
                Debug.Log($"FlowTrace wrote log: {path}\n{content}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"FlowTrace: Failed to write log {path}: {ex}");
            }
        }
    }
}

