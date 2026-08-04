using System;
using System.Collections.Generic;
using CardFramework.Core.Models;
using CardFramework.Core.Engines;

namespace CardFramework.Presentation.Interfaces {
    public interface ITexasHoldemView {
        event Action OnDealRequested;
        event Action OnRestartRequested;
        event Action OnFoldRequested;
        event Action OnMenuRequested;

        void ClearTable();
        void RenderRoundState(TexasHoldemEngine.RoundState roundState, List<CardData> playerHand, List<CardData> communityCards);
        void SpawnPhysicalCard(CardData card, bool isPlayer);
        void SpawnPhysicalCard(CardData card, bool isPlayer, bool isHouseHand);
        void SpawnHousePlaceholders(int count);
        void RevealHouseHand(List<CardData> houseCards);
        void UpdateWalletBalance(int balance);
        void DisplayOutcome(string message);
        void ClearOutcome();
        void SetInteractionState(bool canInteract);
        void AllowResetButton(bool allow);
        void SetRestartButtonEnabled(bool enabled);
        void ShowUi(bool show);
    }
}
