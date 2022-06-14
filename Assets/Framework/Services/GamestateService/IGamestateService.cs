using System.Threading.Tasks;

namespace Framework.Services.GamestateService {
    public interface IGamestateService : IService {
        /// <summary> 
		///Register a new gamestate
		/// </summary> 
		void Register(IGamestate gamestate);

        /// <summary> 
        ///Switch to a gamestate
        /// </summary> 
        void SwitchTo<TGamestate>(object context = null) where TGamestate : IGamestate;

        Task TickAsync(float deltaTime);
    }
}