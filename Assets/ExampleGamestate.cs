using Framework;
using Framework.Services;
using System.Threading.Tasks;

#if UNITY_2022_1_OR_NEWER
using UnityEngine;
#endif

public class ExampleGamestate : Framework.Services.GamestateService.GamestateBase<ExampleGamestate.Context> {
    [InjectService] Framework.Services.GamestateService.IGamestateService gsService;

    public class Context {
        public string name;
        public int someNumber;
    }

    public override void PreEnter() {
        base.PreEnter();

        Framework.Logger.Log("Entering example gamestate - name:" + context.name + " - someNumber:" + context.someNumber.ToString() +" ...");
    }

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

    public override void PostEnter() {
        Framework.Logger.Log("Entered gamestate " + context.name + " !");
    }

    public override void Tick(float deltaTime) {
        base.Tick(deltaTime);

        Framework.Logger.Log("Main Thread");
    }

    public override void TickThreaded(float deltaTime) {
        base.TickThreaded(deltaTime);

        Framework.Logger.Log(System.Threading.Thread.CurrentThread.Name+".Tick");
    }

    public override void OnThreadRendezvouz() {
        base.OnThreadRendezvouz();

        Framework.Logger.Log("Rendesvouz on thread " + System.Threading.Thread.CurrentThread.Name);
    }
}