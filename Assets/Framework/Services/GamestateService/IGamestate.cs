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
        /// <param name="deltaTime"></param>
        void Tick(float deltaTime);
        /// <summary>
        /// Ticked from worker thread
        /// </summary>
        /// <param name="deltaTime"></param>
        void TickThreaded(float deltaTime);

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
        /// Sets the context of the state
        /// </summary>
        /// <param name="context"></param>
        void SetContext(object context);
    }
}