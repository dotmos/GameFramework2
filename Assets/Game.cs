#if UNITY
using UnityEngine;
#endif

using System.Collections.Generic;
using Framework;
using Framework.Services;

public class Game : Core<Game> {
#if UNITY
#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#else
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
#endif // UNITY_EDITOR
#endif // UNITY
    static void Startup() {
        CreateCore();
        
    }

    protected override List<IService> CreateServiceInstances() {
        return null;
    }
}