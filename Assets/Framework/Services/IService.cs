﻿using System.Threading.Tasks;

namespace Framework.Services {
	/// <summary>
	/// Interface for Framework services
	/// </summary>
	public interface IService {
		/// <summary>
		/// Initialize the service
		/// </summary>
		Task InitializeAsync();

		/// <summary>
		/// Callback on new session
		/// </summary>
		void OnNewSession();

		/// <summary>
		/// Callback on session exit
		/// </summary>
		void OnExitSession();

		/// <summary>
		/// Callback after SaveGameData was loaded
		/// </summary>
		Task OnSaveGameLoaded();

		/// <summary>
		/// Called when the application wants to quit.
		/// </summary>
		void OnQuit();
	}
}