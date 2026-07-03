using System;
using CardFramework.Core.Models;

namespace CardFramework.Presentation.Interfaces
{
    /// <summary>
    /// Contract defining the abstraction of the Blackjack table visual representation.
    /// </summary>
    public interface IBlackjackView
    {
        // UI Events forwarded to the controller
        event Action OnHitRequested;
        event Action OnStandRequested;
        event Action OnRestartRequested;

        // UI Update Methods
        void UpdatePlayerScore(int score);
        void UpdateDealerScore(int score);
        void DisplayWinner(string winnerName);
        void ClearTable();
        void SetInteractionState(bool canInteract);

        // Adds a physical 3D card instance onto the matching actor's layout zone
        void SpawnPhysicalCard(CardData card, bool isPlayer);
    }
}