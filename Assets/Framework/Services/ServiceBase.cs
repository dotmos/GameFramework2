using System.Threading.Tasks;

namespace Framework.Services {
	/// <summary>
	/// Base class for creating framework services
	/// </summary>
	public class ServiceBase : IService {
		public ServiceBase() {
			Logger.Log("- Created " + this.GetType());
		}

		public virtual async Task InitializeAsync() {
			Logger.Log("- Initializing " + this.GetType());
			Core.GetInstance().InjectServicesFor(this);

			await Task.Yield();
		}

		public virtual void OnNewSession() {
		}

		public virtual void OnExitSession() {
		}

		public virtual async Task OnSaveGameLoaded() {
			await Task.Yield();
		}


		public virtual void OnQuit() {}
	}
}