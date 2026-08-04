using System;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using CardFramework.Core.Interfaces;

namespace CardFramework.Cloud {
    public class PlayFabEconomyService : IEconomyService {
        public event Action<int> OnBalanceUpdated;
        public event Action<string> OnEconomyError;

        public int CurrentGold { get; private set; }

        private const string CurrencyCodeGold = "GD"; // Our global Gold Code assigned in PlayFab
        public bool BypassAuthCheckForTesting { get; set; } = false;
        
        public void RefreshBalance() {
            // Check the runtime testing bypass OR the native SDK state
            bool isLoggedIn = BypassAuthCheckForTesting || PlayFabClientAPI.IsClientLoggedIn();

            if (!isLoggedIn) {
                OnEconomyError?.Invoke("Cannot fetch balance: Client is not authenticated via PlayFab.");
                return;
            }

            var request = new GetUserInventoryRequest();
            PlayFabClientAPI.GetUserInventory(request, OnFetchInventorySuccess, OnPlayFabError);
        }

        public void CreditGold(int amount) {
            if (amount <= 0) return;

            var request = new AddUserVirtualCurrencyRequest {
                VirtualCurrency = CurrencyCodeGold,
                Amount = amount
            };

            PlayFabClientAPI.AddUserVirtualCurrency(request, OnModifyCurrencySuccess, OnPlayFabError);
        }

        public void DebitGold(int amount) {
            if (amount <= 0) return;

            // Optional: local client validation before making the network request
            if (CurrentGold < amount) {
                OnEconomyError?.Invoke("Insufficient gold balance for this transaction.");
                return;
            }

            var request = new SubtractUserVirtualCurrencyRequest {
                VirtualCurrency = CurrencyCodeGold,
                Amount = amount
            };

            PlayFabClientAPI.SubtractUserVirtualCurrency(request, OnModifyCurrencySuccess, OnPlayFabError);
        }

        private void OnFetchInventorySuccess(GetUserInventoryResult result) {
            // Extract our specific currency balance from the account's virtual currency dictionary
            if (result.VirtualCurrency.TryGetValue(CurrencyCodeGold, out int goldBalance)) {
                CurrentGold = goldBalance;
                Debug.Log($"[Economy] Balance successfully loaded from Cloud: {CurrentGold} GD");
                OnBalanceUpdated?.Invoke(CurrentGold);
            }
        }

        private void OnModifyCurrencySuccess(ModifyUserVirtualCurrencyResult result) {
            CurrentGold = result.Balance;
            OnBalanceUpdated?.Invoke(CurrentGold);
        }

        private void OnPlayFabError(PlayFabError error) {
            string errorMessage = error.GenerateErrorReport();
            Debug.LogError($"[Economy Master Error] Cloud Transaction Fault: {errorMessage}");
            OnEconomyError?.Invoke(errorMessage);
        }
    }
}