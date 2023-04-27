using System.Threading.Tasks;

namespace Framework.Services.GamestateService {
    public interface IGamestateService : IService {

		IGamestate Current { get; }

        /// <summary> 
		///Register a new gamestate
		/// </summary> 
		void Register(IGamestate gamestate);

		/// <summary> 
		///Switch to a gamestate. This does NOT happen immediatly and might happen in the next frame due to the async nature of gamestates! If you want to setup the gamestate, pass it a context and then setup the gamestate via gamestate.OnEnterAsync/.OnPreEnter/.OnPostEnter
		/// </summary> 
		void SwitchTo<TGamestate>(object context = null) where TGamestate : IGamestate;

        Task TickAsync(float deltaTime);
    }
}