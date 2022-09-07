#if !DEACTIVATE_EXAMPLECORE
#if UNITY_2022_1_OR_NEWER
using UnityEngine;
#endif

using System.Collections.Generic;
using Framework;
using Framework.Services;
using System.Threading.Tasks;
using Framework.Services.GamestateService;

public class ExampleCore : Core<ExampleCore, ExampleGamestate> {
#if UNITY_2022_1_OR_NEWER
#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#else
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
#endif // UNITY_EDITOR
#endif // UNITY
    static void Startup() {
        if(System.Threading.Thread.CurrentThread.Name != "MainThread") {
            System.Threading.Thread.CurrentThread.Name = "MainThread";
        }
        CreateCore();
        
    }

    protected override List<IService> CreateServiceInstances() {
        //Create and return all services here. Do NOT set them up! That will be done at a later time. ONLY create the instances.
        List<IService> services = new List<IService>();
        return services;
    }

    protected override List<IGamestate> CreateGamestates() {
        //Create and return all gamestate instances here
        List<IGamestate> states = new List<IGamestate>();
        states.Add(new ExampleGamestate());
        return states;
    }

    protected override object InitialGamestateContext() {
        //Create the initial gamestate context here
        ExampleGamestate.Context context = new ExampleGamestate.Context();
        context.name = "Some Gamestate";
        context.someNumber = 42;

        return context;
    }

#if UNITY_2022_1_OR_NEWER
    //--------------------------------------------------------
    //Simple boot loading animation
    //--------------------------------------------------------
    GameObject loadingCube;
    protected override async Task PreBootAsync() {
        await base.PreBootAsync();

        loadingCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        loadingCube.GetComponent<Renderer>().material.color = Color.red;
        GameObject.DontDestroyOnLoad(loadingCube);
    }

    protected override void BootTick(float deltaTime) {
        base.BootTick(deltaTime);

        loadingCube.transform.Rotate(deltaTime * 90, deltaTime * 90, 0);
    }

    protected override async Task AfterBootAsync() {
        await base.AfterBootAsync();

        UnityEngine.Object.Destroy(loadingCube);
    }
    //--------------------------------------------------------
#endif
}
#endif