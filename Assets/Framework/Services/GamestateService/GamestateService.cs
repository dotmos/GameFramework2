using System.Threading.Tasks;

namespace Framework.Services.GamestateService {
    public class GamestateService : ServiceBase, IGamestateService {
        public override async Task InitializeAsync() {
            await base.InitializeAsync();
        }

        public void Tick(float deltaTime) {
        }
    }
}