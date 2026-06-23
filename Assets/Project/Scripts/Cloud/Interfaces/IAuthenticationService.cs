using System.Threading.Tasks;

namespace CardFramework.Cloud.Interfaces
{
    /// <summary>
    /// Contract for identity management and cloud authentication.
    /// </summary>
    public interface IAuthenticationService
    {
        bool IsLoggedIn { get; }
        string PlayerId { get; }

        Task<bool> LoginAnonymousAsync();
        Task<bool> LoginWithDeviceAsync();
        Task LogoutAsync();
    }
}