using System.Threading.Tasks;

namespace Framework.Services.GamestateService {
    public class GamestateBase<TContext> : IGamestate {
        protected TContext context;
        
        /// <summary>
        /// Sets the context data for this gamestate
        /// </summary>
        /// <param name="context"></param>
        public void SetContext(object context) {
            this.context = (TContext)context;
        }

        /// <summary>
        /// Called before entering the gamestate
        /// </summary>
        public virtual void PreEnter() {
            Core.GetInstance().InjectServicesFor(this);
        }

        /// <summary>
        /// Asny enter
        /// </summary>
        /// <returns></returns>
        public virtual async Task OnEnterAsync() {
            await Task.Yield();
        }

        /// <summary>
        /// Called after the gamestate was entered
        /// </summary>
        public virtual void PostEnter() {

        }

        /// <summary>
        /// Called before the gamestate is exitted
        /// </summary>
        public virtual void PreExit() {

        }

        /// <summary>
        /// Async exit
        /// </summary>
        /// <returns></returns>
        public virtual async Task OnExitAsync() {
            await Task.Yield();
        }

        /// <summary>
        /// Called after the gamestate is exitted
        /// </summary>
        public virtual void PostExit() {

        }

        public virtual void Tick(float deltaTime) {

        }
    }
}