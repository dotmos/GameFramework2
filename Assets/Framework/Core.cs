using Framework.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        Framework.Services.ServiceInjector serviceInjector;

        /// <summary>
        /// Holds all services that the core can use
        /// </summary>
        List<IService> services;

        /// <summary>
        /// The gamestate service. Takes over control once the core is set-up.
        /// </summary>
        Services.GamestateService.IGamestateService gamestateService;

        public Core() {
            //async initialize. TODO: Create "real" async Task if NOT using Unity?
            InitializeAsync();
        }

        async void InitializeAsync() {
            Logger.Log("Creating Core ...");

            BootProgress = 0;

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
            if(_additionalServices != null && _additionalServices.Count > 0) {
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
            await InitializeServicesAsync();

            //Trigger custom after-boot logic
            await AfterBootAsync();

            //Set initial gamestate

            //Start ticking Core.Tick()
            coreTicker.SetTickAction((t) => Tick(t));
        }

        async Task InitializeServicesAsync() {
            Logger.Log("Initializing services ...");
            //Initialize all services
            for (int i = 0; i < services.Count; ++i) {
                //Initialize service
                await services[i].InitializeAsync();
                //Update boot progress
                BootProgress = (i+1) / (float)services.Count;
            }
            BootProgress = 1.0f;
        }

        /// <summary>
        /// Inject services for the given object instance
        /// </summary>
        /// <param name="instance"></param>
        public void InjectServicesFor(object instance) {
            serviceInjector.InjectServicesFor(instance);
        }

        /// <summary>
        /// Create and return service instances for the core to use. Do NOT setup the services! Only create their instances. You MUST downcast to the service's interface before adding them to the output list!
        /// </summary>
        /// <returns></returns>
        protected abstract List<IService> CreateServiceInstances();

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

        /// <summary>
        /// Ticked regularly once the Core has finished booting
        /// </summary>
        /// <param name="deltaTime"></param>
        protected virtual void Tick(float deltaTime) {
            //Tick gamestate service
            gamestateService.Tick(deltaTime);
        }

        public void Dispose() {
            coreTicker.Dispose();
        }
    }

    /// <summary>
    /// The framework's core. Everything starts here. Derive from this class and create your own core.
    /// Then call CreateCore() in a static function of your derived class to launch the core.
    /// </summary>
    public abstract class Core<T> : Core where T:Core,new() {

        /// <summary>
        /// Creates the core. This should be the very first thing to do after launching the application
        /// </summary>
        protected static void CreateCore() {
            coreInstance = new T();
        }
    }
}