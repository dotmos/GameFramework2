#if UNITY_2022_1_OR_NEWER
using UnityEngine;
#endif

namespace Framework {
    public static class Logger {
        public static void Log(string log) {
#if UNITY_2022_1_OR_NEWER
            Debug.Log(log);
#else
            NOT IMPLEMENTED
#endif
        }

        public static void Warning(string log) {
#if UNITY_2022_1_OR_NEWER
            Debug.LogWarning(log);
#else
            NOT IMPLEMENTED
#endif
        }

        public static void Error(string log) {
#if UNITY_2022_1_OR_NEWER
            Debug.LogError(log);
#else
            NOT IMPLEMENTED
#endif
        }
    }
}
