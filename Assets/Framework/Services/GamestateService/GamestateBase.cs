using System.Threading.Tasks;

namespace Framework.Services.GamestateService {
    public class GamestateBase<TContext> : IGamestate {
        [InjectService] protected Framework.Services.GamestateService.IGamestateService gamestateService;

        protected TContext context;
        
        /// <summary>
        /// Sets the context data for this gamestate
        /// </summary>
        /// <param name="context"></param>
        public void SetContext(object context) {
            this.context = (TContext)context;
        }

        /// <summary>
        /// Called BEFORE entering the gamestate
        /// </summary>
        public virtual void PreEnter() {
            Core.GetInstance().InjectServicesFor(this); 
        }

        /// <summary>
        /// Aync enter. Called WHILE entering the gamestate
        /// </summary>
        /// <returns></returns>
        public virtual async Task OnEnterAsync() {
            await Task.Yield();
        }

        /// <summary>
        /// Called AFTER the gamestate was entered, but BEFORE Tick/TickThreaded is called for the first time
        /// </summary>
        public virtual void PostEnter() {

        }

        /// <summary>
        /// Called BEFORE the gamestate is exitted
        /// </summary>
        public virtual void PreExit() {

        }

        /// <summary>
        /// Async exit. Called WHILE exitting the gamestate
        /// </summary>
        /// <returns></returns>
        public virtual async Task OnExitAsync() {
            await Task.Yield();
        }

        /// <summary>
        /// Called AFTER the gamestate is exitted and BEFORE the next gamestate is entered
        /// </summary>
        public virtual void PostExit() {

        }

		/// <summary>
		/// Tick from main thread. Once per frame.
		/// </summary>
		/// <param name="unscaledDeltaTime"></param>
		public virtual void Tick(float unscaledDeltaTime) {

        }

		/// <summary>
		/// Tick. from worker thread (NOT Unity's main thread)
		/// </summary>
		/// <param name="unscaledDeltaTime"></param>
		public virtual void TickThreaded(float unscaledDeltaTime) {

        }

        /// <summary>
        /// Called once per frame, when Unity's main thread and the workerThread rendezvouz. Use this to interchange data between the two threads.
        /// IMPORTANt: Make sure to keep this function as short/performant as possible, as both threads will wait for this function to finish! No heavy processing here! Do that in Tick() and TickThreaded()!
        /// </summary>
        public virtual void OnThreadRendezvouz() {

        }

		public virtual void OnSuspend() {
		}

		public virtual void OnResume() {
		}
	}
}