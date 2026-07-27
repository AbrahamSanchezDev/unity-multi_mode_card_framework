using System;
using System.Collections.Generic;
using CardFramework.Core.Models;

namespace CardFramework.Presentation.Interfaces {
    public interface ISolitaireView {
        event Action OnStockTapped;
        event Action OnRestartRequested;
        event Action OnMenuRequested;
        event Action<CardData, int> OnTableauDropRequested;
        event Action<CardData, int> OnFoundationDropRequested;

        void ClearTable();
        void RenderLayout(List<CardData>[] tableau, List<CardData>[] foundation, List<CardData> stock, List<CardData> waste);
        void UpdateWalletBalance(int balance);
        void SetInteractionState(bool canInteract);
    }
}