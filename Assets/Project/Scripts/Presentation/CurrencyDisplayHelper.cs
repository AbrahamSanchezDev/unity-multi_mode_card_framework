using System;
using CardFramework.Core.Interfaces;

namespace CardFramework.Presentation {
    public sealed class CurrencyDisplayHelper : IDisposable {
        private readonly IEconomyService _economyService;
        private readonly Action<int> _onBalanceChanged;
        private readonly string _currencyCode;

        public CurrencyDisplayHelper(IEconomyService economyService, Action<int> onBalanceChanged, string currencyCode = "GD") {
            _economyService = economyService;
            _onBalanceChanged = onBalanceChanged;
            _currencyCode = string.IsNullOrWhiteSpace(currencyCode) ? "GD" : currencyCode;

            if (_economyService != null) {
                _economyService.OnBalanceUpdated += HandleBalanceUpdated;
                HandleBalanceUpdated(_economyService.CurrentGold);
            }
        }

        public static string FormatBalance(int balance, string currencyCode = "GD") {
            return $"Balance: {balance} {currencyCode}";
        }

        public void Dispose() {
            if (_economyService != null) {
                _economyService.OnBalanceUpdated -= HandleBalanceUpdated;
            }
        }

        private void HandleBalanceUpdated(int balance) {
            _onBalanceChanged?.Invoke(balance);
        }
    }
}
