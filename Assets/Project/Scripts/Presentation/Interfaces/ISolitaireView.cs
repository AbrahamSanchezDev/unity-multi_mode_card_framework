using System;
using System.Collections.Generic;
using CardFramework.Core.Models;

namespace CardFramework.Presentation.Interfaces {
    public interface ISolitaireView {
        event Action OnStockTapped;
        event Action OnRestartRequested;
        event Action OnMenuRequested;
        event Action<List<CardData>, int, int, int> OnTableauDropRequested;
        event Action<List<CardData>, int> OnFoundationDropRequested;

        void AnimateStockDraw(CardData card, int destinationStackCount, Action onComplete);
        void ClearTable();
        void RenderLayout(List<CardData>[] tableau, List<CardData>[] foundation, List<CardData> stock, List<CardData> waste, List<(int ColumnIndex, int CardIndex)> newlyRevealedCards = null);
        void UpdateWalletBalance(int balance);
        void UpdateFoundationScore(int foundationCount, int totalCards);
        void DisplayOutcome(string message);
        void ClearOutcome();
        void SetInteractionState(bool canInteract);

        void ShowUi(bool show);
    }
}