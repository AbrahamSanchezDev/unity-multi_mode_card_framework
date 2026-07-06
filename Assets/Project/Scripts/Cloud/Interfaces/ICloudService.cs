using System;

namespace CardFramework.Core.Interfaces {
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
    }
}