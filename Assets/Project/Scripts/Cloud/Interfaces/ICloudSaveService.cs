using System.Threading.Tasks;

namespace CardFramework.Cloud.Interfaces
{
    /// <summary>
    /// Contract for persistent player data storage, balancing, and cloud save states.
    /// </summary>
    public interface ICloudSaveService
    {
        Task<bool> SaveDataAsync<T>(string key, T data);
        Task<T> LoadDataAsync<T>(string key);
    }
}