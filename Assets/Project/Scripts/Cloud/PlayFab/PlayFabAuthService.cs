using System;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using CardFramework.Cloud.Interfaces;
using UnityEngine;

namespace CardFramework.Cloud.PlayFab
{
    /// <summary>
    /// Concrete implementation of authentication services powered by Microsoft PlayFab.
    /// </summary>
    public class PlayFabAuthService : IAuthenticationService
    {
        public bool IsLoggedIn => PlayFabClientAPI.IsClientLoggedIn();
        public string PlayerId { get; private set; }

        public Task<bool> LoginAnonymousAsync()
        {
            var tcs = new TaskCompletionSource<bool>();

            // Generate a unique identifier based on the user's running platform hardware
            string uniqueId = SystemInfo.deviceUniqueIdentifier;

            var request = new LoginWithCustomIDRequest
            {
                CustomId = uniqueId,
                CreateAccount = true
            };

            PlayFabClientAPI.LoginWithCustomID(request, 
                result => 
                {
                    PlayerId = result.PlayFabId;
                    tcs.SetResult(true);
                }, 
                error => 
                {
                    // Fail gracefully and log the cloud error
                    UnityEngine.Debug.LogError($"[PlayFab Auth Error]: {error.ErrorMessage}");
                    tcs.SetResult(false);
                }
            );

            return tcs.Task;
        }

        public Task<bool> LoginWithDeviceAsync()
        {
            // Falling back to CustomID utilizing hardware signature as standard 
            // cross-platform robust solution in PlayFab.
            return LoginAnonymousAsync();
        }

        public Task LogoutAsync()
        {
            PlayFabClientAPI.ForgetAllCredentials();
            PlayerId = null;
            return Task.CompletedTask;
        }
    }
}