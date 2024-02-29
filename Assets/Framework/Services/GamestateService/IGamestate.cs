using System.Threading.Tasks;

namespace Framework.Services.GamestateService {
    public interface IGamestate {
        /// <summary>
        /// Called before entering the state
        /// </summary>
        void PreEnter();
        /// <summary>
        /// Async enter
        /// </summary>
        /// <returns></returns>
        Task OnEnterAsync();
        /// <summary>
        /// Called after entering the state
        /// </summary>
        void PostEnter();

		/// <summary>
		/// Ticked from main thread
		/// </summary>
		/// <param name="unscaledDeltaTime"></param>
		void Tick(float unscaledDeltaTime);
		/// <summary>
		/// Ticked from worker thread
		/// </summary>
		/// <param name="unscaledDeltaTime"></param>
		void TickThreaded(float unscaledDeltaTime);

        /// <summary>
        /// Called when the main- and workerthread rendezvouz. This happens once each frame
        /// </summary>
        void OnThreadRendezvouz();

        /// <summary>
        /// Called before exitting the state
        /// </summary>
        void PreExit();
        /// <summary>
        /// Async exit
        /// </summary>
        /// <returns></returns>
        Task OnExitAsync();
        /// <summary>
        /// Called after exitting the state
        /// </summary>
        void PostExit();
		/// <summary>
		/// Current gamestate got suspended due to another gamestate got pushed ontop
		/// </summary>
		Task OnSuspend();
		/// <summary>
		/// Current gamestate is resumed after gamestate above got popped
		/// </summary>
		Task OnResume();
        /// <summary>
        /// Sets the context of the state
        /// </summary>
        /// <param name="context"></param>
        void SetContext(object context);
    }
}