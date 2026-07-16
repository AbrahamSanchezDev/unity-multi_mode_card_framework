using System;
using System.Threading.Tasks;

namespace CardFramework.Cloud.Interfaces {
    public interface ICloudService {
        // Triggers when the silent authentication loop completes successfully
        event Action OnAuthenticationSuccess;

        // Triggers if the backend cloud connectivity fails
        event Action<string> OnAuthenticationFailed;

        // True if the user currently holds a valid cloud session token
        bool IsAuthenticated { get; }

        // Unique cloud identity token assigned to this profile session
        string PlayerId { get; }

        // Initializes the silent login sequence using hardware uniqueness markers
        void AuthenticateSilently();

        /// <summary>
        /// Generates a unique 6-character linking PIN from the PlayFab server.
        /// </summary>
        Task<string> GenerateLinkingPINAsync();

        /// <summary>
        /// Validates a 6-character PIN provided by the user to link the current device to an existing profile.
        /// </summary>
        Task<bool> LinkAccountWithPINAsync(string pinCode);
    }
}