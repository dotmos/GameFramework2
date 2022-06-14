namespace Framework.Services.GamestateService {
    public interface IGamestateService : IService {
        void Tick(float deltaTime);
    }
}