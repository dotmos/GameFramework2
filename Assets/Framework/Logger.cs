#if UNITY
using UnityEngine;
#endif

namespace Framework {
    public static class Logger {
        public static void Log(string log) {
#if UNITY
            Debug.Log(log);
#endif
        }

        public static void Warning(string log) {
#if UNITY
            Debug.LogWarning(log);
#endif
        }

        public static void Error(string log) {
#if UNITY
            Debug.LogError(log);
#endif
        }
    }
}
