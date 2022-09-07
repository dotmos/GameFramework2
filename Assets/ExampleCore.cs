#if ENABLE_FRAMEWORK_EXAMPLE
#if UNITY_2022_1_OR_NEWER
using UnityEngine;
#endif

using System.Collections.Generic;
using Framework;
using Framework.Services;
using System.Threading.Tasks;
using Framework.Services.GamestateService;

/// <summary>
/// Example core. Don't forget to supply the initial gamestate when deriving from Core. In this case "ExampleGamestate" is the initial gamestate
/// </summary>
public class ExampleCore : Core<ExampleCore, ExampleGamestate> {

#if UNITY_2022_1_OR_NEWER
#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#else
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
#endif // UNITY_EDITOR
#endif // UNITY_2022_1_OR_NEWER
    static void Startup() {
        if(System.Threading.Thread.CurrentThread.Name != "MainThread") {
            System.Threading.Thread.CurrentThread.Name = "MainThread";
        }
        CreateCore();
        
    }

    protected override List<IService> CreateServiceInstances() {
        //Create and return all services here. Do NOT set them up! That will be done at a later time. ONLY create the instances.
        List<IService> services = new List<IService>();
        
        // ...
        // Add all your services to "services"
        // i.e. services.Add(new Services.MyService());
        // ...
    
        return services;
    }

    protected override List<IGamestate> CreateGamestates() {
        //Create and return all gamestate instances here
        List<IGamestate> states = new List<IGamestate>();
        states.Add(new ExampleGamestate());
        // ...
        // add more gamestates here
        // i.e. states.Add(new MyGamestate());
        // ...

        return states;
    }

    protected override object InitialGamestateContext() {
        //Create the initial gamestate context here
        // In this example, ExampleGamestate is the initial gamestate so we create and return a context for ExampleGamestate
        // Gamestate contexts are used to supply data to gamestates and to interchange data from one gamestate to another
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

        //Create a visual cube that should be displayed during the Core boot process
        loadingCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        loadingCube.GetComponent<Renderer>().material.color = Color.red;
        GameObject.DontDestroyOnLoad(loadingCube);
    }

    protected override void BootTick(float deltaTime) {
        base.BootTick(deltaTime);

        //Rotate the cube
        loadingCube.transform.Rotate(deltaTime * 90, deltaTime * 90, 0);
    }

    protected override async Task AfterBootAsync() {
        await base.AfterBootAsync();

        //Destroy the cube
        UnityEngine.Object.Destroy(loadingCube);
    }
    //--------------------------------------------------------
#endif // UNITY_2022_1_OR_NEWER
}
#endif