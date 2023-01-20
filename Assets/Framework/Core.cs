using Framework.Services;
using Framework.Services.GamestateService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

#if UNITY_2022_1_OR_NEWER
using UnityEngine;
#endif

namespace Framework {
    public interface ICore {
        /// <summary>
        /// Inject services for the given object instance
        /// </summary>
        /// <param name="instance"></param>
        void InjectServicesFor(object instance);
    }

    /// <summary>
    /// The framework's core. Everything starts here. Derive from this class and create your own core.
    /// Then call CreateCore() in a static function of your derived class to launch the core.
    /// </summary>
    public abstract class Core : ICore, IDisposable {
        /// <summary>
        /// The current instance of the core
        /// </summary>
        protected static Core coreInstance;

        public static Core GetInstance() {
            return coreInstance;
        }

        /// <summary>
        /// Whether or not the core is currently running or booting
        /// </summary>
        public static bool IsRunning { get; protected set; }

        public abstract void InjectServicesFor(object instance);

        public abstract void Dispose();

		/// <summary>
		/// Returns all services
		/// </summary>
		/// <returns></returns>
		public abstract List<IService> GetServices();
    }

    /// <summary>
    /// The framework's core. Everything starts here. Derive from this class and create your own core.
    /// Then call CreateCore() in a static function of your derived class to launch the core.
    /// </summary>
    public abstract class Core<TCoreImplementation, TInitialGamestate> : Core where TCoreImplementation:Core,new() where TInitialGamestate : Services.GamestateService.IGamestate {

        /// <summary>
        /// The ticker used to tick/update the core
        /// </summary>
        ICoreTicker coreTicker;

        /// <summary>
        /// The boot progress. Range 0-1. Use this to display a loading bar
        /// </summary>
        protected float BootProgress { get; private set; }

        /// <summary>
        /// The service injector that is used to inject services using the [InjectService] attribute
        /// </summary>
        public Framework.Services.ServiceInjector serviceInjector;

        /// <summary>
        /// Holds all services that the core can use
        /// </summary>
        List<IService> services;

        /// <summary>
        /// The gamestate service. Takes over control once the core is set-up.
        /// </summary>
        Services.GamestateService.IGamestateService gamestateService;

        /// <summary>
        /// Creates the core. This should be the very first thing to do after launching the application
        /// </summary>
        protected static void CreateCore() {
#if UNITY_EDITOR
			// Stop tasks that are still running in the background when stopping the app in the unity editor by triggering a domain reload. Not the most elegant way, but it does the job and just kills everything.
			UnityEditor.EditorApplication.playModeStateChanged += state => {
				if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode) {
					UnityEngine.Debug.Log("[AppInit] Reloading scripts to prevent dangling Tasks from continuing executing in edit mode");
					UnityEditor.EditorUtility.RequestScriptReload();
					// Not sure if needed
					//UnityEditor.EditorApplication.isPaused = paused;
				}
			};
#endif

			coreInstance = new TCoreImplementation();
        }

        public Core() {
            //async initialize. TODO: Create "real" async Task if NOT using Unity?
            InitializeAsync();
        }

        async Task InitializeAsync() {
            Logger.Log("Creating Core ...");

			Application.wantsToQuit += OnQuit;

			BootProgress = 0;
            IsRunning = true;

            //Trigger custom pre-boot logic
            await PreBootAsync();

            //Create core ticker
            coreTicker = CoreTicker.Create();

            //Start ticking Core.BootTick()
            coreTicker.SetTickAction((t) => BootTick(t));

            //Create service injector
            serviceInjector = new Services.ServiceInjector();

            //Create Services
            Logger.Log("Creating services ...");
            services = new List<IService>();
            //Create gamestate service
            gamestateService = new Services.GamestateService.GamestateService();
            services.Add(gamestateService);
            //Create additional services
            List<IService> _additionalServices = CreateServiceInstances();
            if (_additionalServices != null && _additionalServices.Count > 0) {
                services.AddRange(_additionalServices);
            }

            //Register all services with serviceInjector
            for (int i = 0, iEnd = services.Count; i < iEnd; ++i) {
                serviceInjector.Register(services[i]);
            }

            //Cross-Inject all services
            Logger.Log("Cross-Injecting services ...");
            for (int i = 0, iEnd = services.Count; i < iEnd; ++i) {
                serviceInjector.InjectServicesFor(services[i]);
            }

            //async service setup
            Logger.Log("Initializing Services ...");
            await InitializeServicesAsync();

            //Register all gamestates
            List<IGamestate> gamestates = CreateGamestates();
            for(int i=0,iEnd = gamestates.Count; i<iEnd; ++i) {
                gamestateService.Register(gamestates[i]);
            }

			//Dispose coreTicker
			coreTicker.Dispose();

			//Trigger custom after-boot logic
			await AfterBootAsync();

            Logger.Log("###### Core loaded! ######");

            //Set initial gamestate
            gamestateService.SwitchTo<TInitialGamestate>(InitialGamestateContext());

            // Update coreTicker to tick Core.Tick() now
            //coreTicker.SetTickAction((t) => Tick(t));

            //Start Core.TickAsync()
            StartTickAsync();
        }

        async Task InitializeServicesAsync() {
            //Initialize all services
            for (int i = 0; i < services.Count; ++i) {
                //Initialize service
                await services[i].InitializeAsync();
                //Update boot progress
                BootProgress = (i + 1) / (float)services.Count;
            }
            BootProgress = 1.0f;
        }

        /// <summary>
        /// Inject services for the given object instance
        /// </summary>
        /// <param name="instance"></param>
        public override void InjectServicesFor(object instance) {
            serviceInjector.InjectServicesFor(instance);
        }

        /// <summary>
        /// Create and return service instances for the core to use. Do NOT setup the services! Only create their instances. You MUST downcast to the service's interface before adding them to the output list!
        /// </summary>
        /// <returns></returns>
        protected abstract List<IService> CreateServiceInstances();

        /// <summary>
        /// Create an return all gamestates that are used in the game
        /// </summary>
        protected abstract List<IGamestate> CreateGamestates();

        /// <summary>
        /// Create and return the initial gamestate context
        /// </summary>
        /// <returns></returns>
        protected abstract object InitialGamestateContext();

        /// <summary>
        /// Called before any core logic is being created. Can be used to setup a boot animation.
        /// </summary>
        protected virtual async Task PreBootAsync() { await Task.Yield(); }

        /// <summary>
        /// Ticked during core boot. Can be used for updating a boot animation.
        /// </summary>
        /// <param name="deltaTime"></param>
        protected virtual void BootTick(float deltaTime) { }

        /// <summary>
        /// Called after core logic is created but BEFORE first gamestate is being started. Can be used for boot animation cleanup.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task AfterBootAsync() { await Task.Yield(); }

        async void StartTickAsync() {
            Stopwatch watch = Stopwatch.StartNew();
#if UNITY_2022_1_OR_NEWER
            while (IsRunning) {
                IsRunning = Application.isPlaying;

                //Create delta time
                float deltaTime = watch.ElapsedMilliseconds / 1000.0f;// Time.deltaTime;// Time.time - lastTick;
                watch.Restart();

                //Tick gamestate service
                await gamestateService.TickAsync(deltaTime);

                await Task.Yield();
            }
#else
            NOT IMPLEMENTED
#endif



            await Task.Yield();
        }

		bool OnQuit() {
			for (int i = 0, count = services.Count; i < count; ++i) {
				services[i].OnQuit();
			}

			return true;
		}

		/// <summary>
		/// Return all registered services
		/// </summary>
		/// <returns></returns>
		public override List<IService> GetServices() {
			return services;
		}

        public override void Dispose() {
            coreTicker.Dispose();
            IsRunning = false;
        }
    }
}