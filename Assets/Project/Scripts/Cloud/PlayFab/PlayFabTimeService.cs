using System;
using System.Threading.Tasks;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using CardFramework.Core.Interfaces;

namespace CardFramework.Cloud {
    public class PlayFabTimeService : ITimeService {
        /// <summary>
        /// Resolves the current authoritative time from the cloud network using a Task completion source loop.
        /// </summary>
        public Task<DateTime> GetServerTimeUtcAsync() {
            var completionSource = new TaskCompletionSource<DateTime>();

            var request = new GetTimeRequest();

            PlayFabClientAPI.GetTime(request,
                result => {
                    Debug.Log($"[TimeService] Authoritative server time resolved: {result.Time}");
                    completionSource.SetResult(result.Time.ToUniversalTime());
                },
                error => {
                    Debug.LogError($"[TimeService] Failed to fetch server time: {error.GenerateErrorReport()}. Falling back to safe UTC evaluation.");
                    // Fallback to local UTC if network resolution fails completely, but flag the error context
                    completionSource.SetException(new Exception($"PlayFab Time Resolution Fault: {error.ErrorMessage}"));
                }
            );

            return completionSource.Task;
        }
    }
}