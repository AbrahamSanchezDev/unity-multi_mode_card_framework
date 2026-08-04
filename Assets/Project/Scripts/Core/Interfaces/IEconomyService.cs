using System;

namespace CardFramework.Core.Interfaces {
    public interface IEconomyService {
        // Triggers whenever the server updates the current gold balance
        event Action<int> OnBalanceUpdated;

        // Triggers if an economy operation fails (e.g., insufficient funds)
        event Action<string> OnEconomyError;

        // Cached local representation of the player's server gold
        int CurrentGold { get; }

        // Fetches the latest balance and recharge metadata from the cloud
        void RefreshBalance();

        // Server-authoritative credit transaction (e.g., winning a hand)
        void CreditGold(int amount);

        // Server-authoritative debit transaction (e.g., placing a bet)
        void DebitGold(int amount);
    }
}