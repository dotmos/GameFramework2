using System.Threading.Tasks;

namespace Framework.Services {
    /// <summary>
    /// Interface for Framework services
    /// </summary>
    public interface IService {
        /// <summary>
        /// Initialize the service
        /// </summary>
        Task InitializeAsync();
    }
}