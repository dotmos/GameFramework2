﻿using System.Threading.Tasks;

namespace Framework.Services.GamestateService {
	public interface IGamestateService : IService {

		/// <summary>
		/// !WARNING! If you are using this, you are probably doing something wrong as this goes against the idea of how gamestates work! ONLY used for test driven development!
		/// </summary>
		IGamestate Current { get; }
		/// <summary>
		/// !WARNING! If you are using this, you are probably doing something wrong as this goes against the idea of how gamestates work! ONLY used for test driven development!
		/// </summary>
		IGamestate Next { get; }

		/// <summary>
		/// Is the current gamestate started (finished the whole startup-lifecycle)
		/// </summary>
		bool IsCurrentGamestateStarted { get; }

		/// <summary> 
		///Register a new gamestate
		/// </summary> 
		void Register(IGamestate gamestate);

		/// <summary> 
		///Switch to a gamestate. This does NOT happen immediatly and might happen in the next frame due to the async nature of gamestates! If you want to setup the gamestate, pass it a context and then setup the gamestate via gamestate.OnEnterAsync/.OnPreEnter/.OnPostEnter
		/// </summary> 
		void SwitchTo<TGamestate>(object context = null) where TGamestate : IGamestate;

		Task TickAsync(float deltaTime);

		/// <summary>
		/// !WARNING! If you are using this, you are doing something wrong! ONLY used for test driven development!
		/// Wait until a new(!!) gamestate is started!
		/// If the current gamestate is still in startup this will wait for the current to startup
		/// CAUTION: this will not actively wait for you! You need to await on the returned task
		/// 
		/// This is mainly supposed to be used for automatic testing
		/// </summary>
		/// <returns></returns>
		Task<IGamestate> WaitForGamestateStarted();

		/// <summary>
		/// !WARNING! If you are using this, you are doing something wrong! ONLY used for test driven development!
		/// Wait for a specific gamestate being started! 
		/// CAUTION: this will not actively wait for you! You need to await on the returned task
		/// 
		/// This is mainly supposed to be used for automatic testing
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		Task<IGamestate> WaitForGamestateStarted<T>();
	}
}
