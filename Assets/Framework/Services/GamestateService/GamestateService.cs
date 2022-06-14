using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.Services.GamestateService {
    public class GamestateService : ServiceBase, IGamestateService {
        Dictionary<Type, IGamestate> gamestates;

        IGamestate currentGamestate;
        IGamestate nextGamestate;

        public override async Task InitializeAsync() {
            await base.InitializeAsync();

            gamestates = new Dictionary<Type, IGamestate>();
            currentGamestate = null;
            nextGamestate = null;
        }

        public void Register(IGamestate gamestate) {
            gamestates.Add(gamestate.GetType(), gamestate);
        }

        public void SwitchTo<TGamestate>(object context = null) where TGamestate : IGamestate {
            nextGamestate = gamestates[typeof(TGamestate)];
            nextGamestate.SetContext(context);
        }

        public async Task TickAsync(float deltaTime) {
            if(nextGamestate != null) {
                //Exit from current state
                if(currentGamestate != null) {
                    currentGamestate.PreExit();

                    await currentGamestate.OnExitAsync();

                    currentGamestate.PostExit();
                }
                //Switch to next gamestate
                nextGamestate.PreEnter();
                currentGamestate = nextGamestate;
                nextGamestate = null;
                await currentGamestate.OnEnterAsync();
                currentGamestate.PostEnter();
            }

            if(currentGamestate != null) {
                currentGamestate.Tick(deltaTime);
            }
        }
    }
}