using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using CardFramework.Cloud.Interfaces;

namespace CardFramework.Cloud.PlayFab {
    // Interface Wrapper for PlayFab Data Operations to allow perfect isolation in testing
    public interface IPlayFabDataWrapper {
        void UpdateUserData(UpdateUserDataRequest request, Action<UpdateUserDataResult> resultCallback, Action<PlayFabError> errorCallback);
        void GetUserData(GetUserDataRequest request, Action<GetUserDataResult> resultCallback, Action<PlayFabError> errorCallback);
    }
    // Default production implementation wrapping PlayFab's static architecture
    public class DefaultPlayFabDataWrapper : IPlayFabDataWrapper {
        public void UpdateUserData(UpdateUserDataRequest request, Action<UpdateUserDataResult> resultCallback, Action<PlayFabError> errorCallback)
            => PlayFabClientAPI.UpdateUserData(request, resultCallback, errorCallback);

        public void GetUserData(GetUserDataRequest request, Action<GetUserDataResult> resultCallback, Action<PlayFabError> errorCallback)
            => PlayFabClientAPI.GetUserData(request, resultCallback, errorCallback);
    }
    
    /// <summary>
    /// Concrete implementation of key-value persistence storage using PlayFab Player Data.
    /// </summary>
    public class PlayFabDataService : ICloudSaveService {
        private readonly IPlayFabDataWrapper _wrapper;
        // Production constructor (Uses default static SDK handles seamlessly)
        public PlayFabDataService() : this(new DefaultPlayFabDataWrapper()) { }

        // Test injection constructor (Allows perfect deterministic mocking)
        public PlayFabDataService(IPlayFabDataWrapper wrapper) {
            _wrapper = wrapper;
        }

        public Task<bool> SaveDataAsync<T>(string key, T data) {
            var tcs = new TaskCompletionSource<bool>();
            string jsonRaw = UnityEngine.JsonUtility.ToJson(data);

            var request = new UpdateUserDataRequest {
                Data = new Dictionary<string, string> { { key, jsonRaw } },
                Permission = UserDataPermission.Private
            };

            _wrapper.UpdateUserData(request,
                result => tcs.SetResult(true),
                error => {
                    UnityEngine.Debug.LogError($"[PlayFab Save Data Error]: {error.ErrorMessage}");
                    tcs.SetResult(false);
                }
            );

            return tcs.Task;
        }

        public Task<T> LoadDataAsync<T>(string key) {
            var tcs = new TaskCompletionSource<T>();
            var request = new GetUserDataRequest { Keys = new List<string> { key } };

            _wrapper.GetUserData(request,
                result => {
                    if (result.Data != null && result.Data.ContainsKey(key)) {
                        string jsonRaw = result.Data[key].Value;
                        T deserializedData = UnityEngine.JsonUtility.FromJson<T>(jsonRaw);
                        tcs.SetResult(deserializedData);
                    }
                    else {
                        tcs.SetResult(default);
                    }
                },
                error => {
                    UnityEngine.Debug.LogError($"[PlayFab Load Data Error]: {error.ErrorMessage}");
                    tcs.SetResult(default);
                }
            );

            return tcs.Task;
        }
    }
}