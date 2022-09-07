#if ENABLE_FRAMEWORK_EXAMPLE
using Framework;
using Framework.Services;
using System.Threading.Tasks;

#if UNITY_2022_1_OR_NEWER
using UnityEngine;
#endif

public class ExampleGamestate : Framework.Services.GamestateService.GamestateBase<ExampleGamestate.Context> {
    [InjectService] Framework.Services.GamestateService.IGamestateService gsService;

    /// <summary>
    /// Context of ExampleGamestate.
    /// This is used to supply data to ExampleGamestate if the Core or another gamestate switches to ExampleGamestate
    /// </summary>
    public class Context {
        public string name;
        public int someNumber;
    }

    /// <summary>
    /// This is called BEFORE entering the gamestate
    /// </summary>
    public override void PreEnter() {
        base.PreEnter();

        Framework.Logger.Log("Entering example gamestate - name:" + context.name + " - someNumber:" + context.someNumber.ToString() +" ...");
    }

    /// <summary>
    /// This is called WHILE entering the gamestate
    /// </summary>
    /// <returns></returns>
    public override async Task OnEnterAsync() {
        await base.OnEnterAsync();

#if UNITY_2022_1_OR_NEWER
        //Wait some artificial time
        //Display a spinning cube while waiting
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
        for(int i=0; i<500; ++i) {
            go.transform.Rotate(0, 90*(watch.ElapsedMilliseconds/1000.0f), 0);
            watch.Restart();
            await Task.Delay(1);
        }
        GameObject.Destroy(go);
#else
        //Wait some artificial time
        await Task.Delay(1000);
#endif
    }

    /// <summary>
    /// This is called AFTER the gamestate was entered, but BEFORE ticking it for the first time
    /// </summary>
    public override void PostEnter() {
        Framework.Logger.Log("Entered gamestate " + context.name + " !");
    }

    /// <summary>
    /// Tick. Once per frame
    /// </summary>
    /// <param name="deltaTime"></param>
    public override void Tick(float deltaTime) {
        base.Tick(deltaTime);

        Framework.Logger.Log("Main Thread");
    }

    /// <summary>
    /// Tick from workerThread (NOT Unity's main thread). Once per frame.
    /// </summary>
    /// <param name="deltaTime"></param>
    public override void TickThreaded(float deltaTime) {
        base.TickThreaded(deltaTime);

        Framework.Logger.Log(System.Threading.Thread.CurrentThread.Name+".Tick");
    }

    /// <summary>
    /// Called once per frame, when Unity's main thread and the workerThread rendezvouz. Use this to interchange data between the two threads.
    /// IMPORTANt: Make sure to keep this function as short/performant as possible, as both threads will wait for this function to end! No heavy processing here! Do that in Tick and TickThreaded!
    /// </summary>
    public override void OnThreadRendezvouz() {
        base.OnThreadRendezvouz();

        Framework.Logger.Log("Rendesvouz on thread " + System.Threading.Thread.CurrentThread.Name);
    }
}
#endif