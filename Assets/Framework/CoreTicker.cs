using System;
using System.Diagnostics;
using System.Threading;

#if UNITY_2022_1_OR_NEWER
using UnityEngine;
#endif

namespace Framework {
    public interface ICoreTicker : IDisposable {
        void SetTickAction(Action<float> a);
    }

#if UNITY_2022_1_OR_NEWER
    /// <summary>
    /// Helper class for ticking the Core
    /// </summary>
    public class CoreTicker : MonoBehaviour, ICoreTicker {
        Action<float> tickAction;

        public static ICoreTicker Create() {
            GameObject go = new GameObject("CoreTicker");
            DontDestroyOnLoad(go);
            return go.AddComponent<CoreTicker>();
        }

        public void SetTickAction(Action<float> a) {
            tickAction = a;
        }

        void Update() {
            tickAction?.Invoke(Time.deltaTime);
        }

        public void Dispose() {
            tickAction = null;
            Destroy(this.gameObject);
        }
    }
#else
    /// <summary>
    /// Helper class for ticking the Core
    /// </summary>
    public class CoreTicker : ICoreTicker {
        Thread tickerThread;
        Action<float> tickAction;
        bool keepTicking;

        public static ICoreTicker Create() {
            return new CoreTicker();
        }

        public CoreTicker() {
            keepTicking = true;
            tickerThread = new Thread(Tick);
            tickerThread.Start();
        }

        void Tick() {
            Stopwatch watch = Stopwatch.StartNew();
            while (keepTicking) {
                tickAction?.Invoke(watch.ElapsedMilliseconds/1000.0f);
                watch.Restart();
            }
            
            tickAction = null;
            watch.Stop();
            watch = null;
        }

        public void SetTickAction(Action<float> a) {
            tickAction = a;
        }

        public void Dispose() {
            keepTicking = false;
        }
    }
#endif
}