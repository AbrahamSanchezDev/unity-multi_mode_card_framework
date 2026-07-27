using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using CardFramework.Core.Interfaces;

namespace CardFramework.Core.Managers {
    public class CloudMailboxManager {
        private readonly ITimeService _timeService;
        private readonly IEconomyService _economyService;

        private const string PlayFabCooldownKey = "LastDailyClaimTimeUtc";
        private const int CooldownHoursRequired = 24;

        public CloudMailboxManager(ITimeService timeService, IEconomyService economyService) {
            _timeService = timeService;
            _economyService = economyService;
        }

        /// <summary>
        /// TASK-4.4 Anti-Cheat: Checks PlayFab User Read-Only Data cloud keys to see if the cooldown is active.
        /// </summary>
        public async Task<bool> TryClaimDailyRewardAsync(int rewardAmount) {
            try {
                // 1. Fetch the absolute true time from the server pipeline
                DateTime serverTime = await _timeService.GetServerTimeUtcAsync();

                // 2. Fetch the true historical claim timestamp directly from the player's cloud record
                DateTime? lastClaimTime = await GetLastClaimTimeFromCloudAsync();

                if (lastClaimTime.HasValue) {
                    TimeSpan elapsed = serverTime - lastClaimTime.Value;

                    if (elapsed.TotalHours < CooldownHoursRequired) {
                        double hoursLeft = CooldownHoursRequired - elapsed.TotalHours;
                        Debug.LogWarning($"[Anti-Cheat] Claim rejected! Cloud verified check failed. Cooldown remaining: {hoursLeft:F2} hours.");
                        return false;
                    }
                }

                // 3. Process the server-verified reward payload injection
                Debug.Log($"[Mailbox] Cloud time verification successful. Crediting {rewardAmount} GD to account.");
                _economyService.CreditGold(rewardAmount);

                // 4. Save the verified server timestamp directly up to PlayFab cloud storage
                await SaveClaimTimeToCloudAsync(serverTime);

                return true;
            }
            catch (Exception ex) {
                Debug.LogError($"[Mailbox] Anti-cheat verification flow aborted: {ex.Message}");
                return false;
            }
        }

        private Task<DateTime?> GetLastClaimTimeFromCloudAsync() {
            var tcs = new TaskCompletionSource<DateTime?>();

            PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
                result => {
                    if (result.Data != null && result.Data.TryGetValue(PlayFabCooldownKey, out var record)) {
                        if (DateTime.TryParse(record.Value, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime parsedTime)) {
                            tcs.SetResult(parsedTime.ToUniversalTime());
                            return;
                        }
                    }
                    tcs.SetResult(null);
                },
                error => {
                    Debug.LogError($"[Mailbox] Error fetching cloud user metadata: {error.GenerateErrorReport()}");
                    tcs.SetException(new Exception("Cloud read failure."));
                }
            );

            return tcs.Task;
        }

        /// <summary>
        /// TASK-4.4: Checks the cloud database to determine if the player has already claimed 
        /// their reward within the strict 24-hour cooldown window.
        /// </summary>
        public async Task<bool> IsDailyRewardClaimedAsync() {
            try {
                // 1. Fetch authoritative true time from the cloud network pipeline
                DateTime serverTime = await _timeService.GetServerTimeUtcAsync();

                // 2. Query historical timestamp records from the user profile database
                DateTime? lastClaimTime = await GetLastClaimTimeFromCloudAsync();

                if (lastClaimTime.HasValue) {
                    TimeSpan elapsed = serverTime - lastClaimTime.Value;

                    // Returns true if the elapsed time is less than the required cooldown limit
                    return elapsed.TotalHours < CooldownHoursRequired;
                }

                return false;
            }
            catch (Exception ex) {
                // Fallback safely to prevent layout exploits if network timeouts happen
                Debug.LogError($"[Mailbox] Cooldown pre-check check aborted: {ex.Message}");
                return false;
            }
        }

        private Task<bool> SaveClaimTimeToCloudAsync(DateTime timestamp) {
            var tcs = new TaskCompletionSource<bool>();

            var request = new UpdateUserDataRequest {
                Data = new Dictionary<string, string>
                {
                    { PlayFabCooldownKey, timestamp.ToString("o") } // Save using standard ISO 8601 Round-trip string format
                },
                Permission = UserDataPermission.Public // public or private, but securely decoupled from unsafe user editing
            };

            PlayFabClientAPI.UpdateUserData(request,
                result => {
                    Debug.Log("[Mailbox] Authorized timestamp successfully logged to PlayFab User Profile Cloud Data.");
                    tcs.SetResult(true);
                },
                error => {
                    Debug.LogError($"[Mailbox] Failed writing timestamp up to PlayFab cloud storage: {error.GenerateErrorReport()}");
                    tcs.SetResult(false);
                }
            );

            return tcs.Task;
        }
    
        public async Task<TimeSpan> GetRemainingCooldownAsync() {
            // Pull authoritative server time from ITimeService
            DateTime serverTime = await _timeService.GetServerTimeUtcAsync();
            
            // Assuming daily reset occurs at 00:00 UTC (Midnight)
            DateTime nextResetUtc = serverTime.Date.AddDays(1);
            
            TimeSpan remaining = nextResetUtc - serverTime;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    
    }
}