using System.IO;
using UnityEngine;

namespace FlowTrace
{
    public static class BattleFlowTracer
    {
        public const string CategoryState = "State";
        public const string BattleEndMarker = "BATTLE_END";
        private static bool _reachedResolution;
        public static bool ReachedResolution => _reachedResolution;

        private static string LogPath => Path.Combine(Application.persistentDataPath, "battle_trace.log");
        private static object _fileLock = new object();
        private static bool _pathLogged;

        public static void Trace(string category, string message)
        {
            var line = $"[Flow][{category}] {message}";
            Debug.Log(line);
            AppendLine(line);
        }

        public static void TraceStateEnter(string stateName)
        {
            Trace(CategoryState, $"Enter {stateName}");
            if (stateName == nameof(Game.Battle.ResolutionState))
            {
                _reachedResolution = true;
            }
        }

        public static void TraceStateExit(string stateName)
        {
            Trace(CategoryState, $"Exit {stateName}");
        }

        public static void LogBattleEndOk() => LogBattleEnd("ok");

        public static void LogBattleEnd(string reason)
        {
            var line = $"{BattleEndMarker}({reason})";
            Debug.Log(line);
            AppendLine(line);
        }

        public static void ClearLogFile()
        {
            try
            {
                if (File.Exists(LogPath)) File.Delete(LogPath);
                // Also note the path for convenience once per session
                if (!_pathLogged)
                {
                    Debug.Log($"[Flow][Smoke] log path: {LogPath}");
                    _pathLogged = true;
                }
            }
            catch { }
        }

        private static void AppendLine(string line)
        {
            try
            {
                lock (_fileLock)
                {
                    File.AppendAllText(LogPath, line + "\n");
                }
            }
            catch { }
        }
    }
}
