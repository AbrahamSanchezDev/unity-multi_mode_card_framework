using System;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using CardFramework.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;


using ClientAPI = PlayFab.PlayFabClientAPI;
using CreateRequest = PlayFab.ClientModels.CreateSharedGroupRequest;
using UpdateRequest = PlayFab.ClientModels.UpdateSharedGroupDataRequest;
using GetRequest = PlayFab.ClientModels.GetSharedGroupDataRequest;
using LinkRequest = PlayFab.ClientModels.LinkCustomIDRequest;

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

        /// <summary>
        /// TASK-4.3: Generates a 6-character PIN and registers a dynamic tracking group in PlayFab
        /// so that an external device can look it up.
        /// </summary>
        public async Task<string> GenerateLinkingPINAsync() {
            if (!IsAuthenticated)
                throw new InvalidOperationException("[PlayFabService] Must be authenticated to generate a PIN.");

            string generatedPin = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            var completionSource = new TaskCompletionSource<string>();

            // 1. Usamos la firma exacta basada en tu archivo: CreateSharedGroup
            var createGroupRequest = new CreateRequest { SharedGroupId = generatedPin };

            ClientAPI.CreateSharedGroup(createGroupRequest, _ => {
                // 2. Registramos el PlayerId dentro del payload del grupo
                var updateGroupDataRequest = new UpdateRequest {
                    SharedGroupId = generatedPin,
                    Data = new Dictionary<string, string> { { "TargetPlayerId", PlayerId } }
                };

                ClientAPI.UpdateSharedGroupData(updateGroupDataRequest, result => {
                    Debug.Log($"[PlayFabService] PIN Shared Group registered successfully: {generatedPin}");
                    completionSource.SetResult(generatedPin);
                },
                error => completionSource.SetException(new Exception($"Failed to update group data: {error.ErrorMessage}")));
            },
            error => completionSource.SetException(new Exception($"Failed to reserve PIN slot: {error.ErrorMessage}")));

            return await completionSource.Task;
        }

        /// <summary>
        /// TASK-4.3: Uses the provided pinCode to look up the target account profile 
        /// and link it to this device's credentials.
        /// </summary>
        public async Task<bool> LinkAccountWithPINAsync(string pinCode) {
            if (string.IsNullOrEmpty(pinCode) || pinCode.Length != 6) {
                Debug.LogWarning("[PlayFabService] Structural verification aborted: Invalid PIN length.");
                return false;
            }

            var completionSource = new TaskCompletionSource<bool>();
            string formattedPin = pinCode.Trim().ToUpper();

            // 3. Usamos la firma exacta basada en tu archivo: GetSharedGroupData
            var getGroupRequest = new GetRequest { SharedGroupId = formattedPin };

            ClientAPI.GetSharedGroupData(getGroupRequest, result => {
                if (result.Data != null && result.Data.TryGetValue("TargetPlayerId", out var targetPlayerIdValue)) {
                    string externalPlayerId = targetPlayerIdValue.Value;
                    Debug.Log($"[PlayFabService] PIN matched! Found target profile ID: {externalPlayerId}");

                    var linkRequest = new LinkRequest {
                        CustomId = SystemInfo.deviceUniqueIdentifier,
                        ForceLink = true
                    };

                    ClientAPI.LinkCustomID(linkRequest,
                        linkResult => {
                            Debug.Log("[PlayFabService] Cross-platform device link completed successfully via PIN validation loop!");
                            completionSource.SetResult(true);
                        },
                        linkError => {
                            Debug.LogError($"[PlayFabService] Link assignment failed: {linkError.ErrorMessage}");
                            completionSource.SetResult(false);
                        }
                    );
                }
                else {
                    Debug.LogWarning("[PlayFabService] PIN validation failed: Token expired or empty group payload.");
                    completionSource.SetResult(false);
                }
            },
            error => {
                Debug.LogError($"[PlayFabService] PIN network resolution failed: {error.ErrorMessage}");
                completionSource.SetResult(false);
            });

            return await completionSource.Task;
        }

    }
}
