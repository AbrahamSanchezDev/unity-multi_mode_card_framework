using System;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using CardFramework.Core.Interfaces;

namespace CardFramework.Cloud {
    public class PlayFabCloudService : ICloudService {
        public event Action OnAuthenticationSuccess;
        public event Action<string> OnAuthenticationFailed;

        public bool IsAuthenticated { get; private set; }
        public string PlayerId { get; private set; }

        public void AuthenticateSilently() {
#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL Sandbox fallback: Browser standard tracking cookies/cached preference string
            string customId = PlayerPrefs.GetString("PlayFab_Custom_WebGL_ID", string.Empty);
            if (string.IsNullOrEmpty(customId))
            {
                customId = Guid.NewGuid().ToString();
                PlayerPrefs.SetString("PlayFab_Custom_WebGL_ID", customId);
                PlayerPrefs.Save();
            }
#else
            // Safe cross-platform hardware extraction (PC, Android, iOS, Meta Quest 3)
            string customId = SystemInfo.deviceUniqueIdentifier;
#endif

            var request = new LoginWithCustomIDRequest {
                CustomId = customId,
                CreateAccount = true
            };

            PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
        }

        private void OnLoginSuccess(LoginResult result) {
            IsAuthenticated = true;
            PlayerId = result.PlayFabId;
            Debug.Log($"[CloudService] PlayFab Silent Login Successful! PlayerID: {PlayerId}");
            OnAuthenticationSuccess?.Invoke();
        }

        private void OnLoginFailure(PlayFabError error) {
            IsAuthenticated = false;
            string errorMessage = error.GenerateErrorReport();
            Debug.LogError($"[CloudService] PlayFab Login Failed: {errorMessage}");
            OnAuthenticationFailed?.Invoke(errorMessage);
        }
    }
}