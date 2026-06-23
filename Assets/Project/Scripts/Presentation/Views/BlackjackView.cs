using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardFramework.Presentation.Interfaces;

namespace CardFramework.Presentation.Views
{
    /// <summary>
    /// MonoBehavior UI implementation mapping Unity Canvas components to the IBlackjackView architectural boundary.
    /// </summary>
    public class BlackjackView : MonoBehaviour, IBlackjackView
    {
        [Header("Buttons Configuration")]
        [SerializeField] private Button hitButton;
        [SerializeField] private Button standButton;
        [SerializeField] private Button restartButton;

        [Header("Text Labels Configuration")]
        [SerializeField] private TextMeshProUGUI playerScoreText;
        [SerializeField] private TextMeshProUGUI dealerScoreText;
        [SerializeField] private TextMeshProUGUI outcomeMessageText;

        // Implementation of the architectural view contract events
        public event Action OnHitRequested;
        public event Action OnStandRequested;
        public event Action OnRestartRequested;

        private void Awake()
        {
            // Sanity check to ensure UI components are properly assigned in the inspector
            ValidateInspectorBindings();

            // Forward Unity UI button click events directly into the architecture loop
            hitButton.onClick.AddListener(() => OnHitRequested?.Invoke());
            standButton.onClick.AddListener(() => OnStandRequested?.Invoke());
            restartButton.onClick.AddListener(() => OnRestartRequested?.Invoke());
        }

        private void OnDestroy()
        {
            // Clean up listeners to prevent memory fragmentation on scene teardown
            hitButton.onClick.RemoveAllListeners();
            standButton.onClick.RemoveAllListeners();
            restartButton.onClick.RemoveAllListeners();
        }

        public void UpdatePlayerScore(int score)
        {
            playerScoreText.text = $"Player: {score}";
        }

        public void UpdateDealerScore(int score)
        {
            dealerScoreText.text = $"Dealer: {score}";
        }

        public void DisplayWinner(string winnerName)
        {
            outcomeMessageText.gameObject.SetActive(true);
            outcomeMessageText.text = winnerName;
        }

        public void ClearTable()
        {
            outcomeMessageText.text = string.Empty;
            outcomeMessageText.gameObject.SetActive(false);
            
            // Note: Card asset instantiation cleanup will be handled inside TASK-3.4
        }

        public void SetInteractionState(bool canInteract)
        {
            hitButton.interactable = canInteract;
            standButton.interactable = canInteract;
        }

        private void ValidateInspectorBindings()
        {
            if (hitButton == null || standButton == null || restartButton == null ||
                playerScoreText == null || dealerScoreText == null || outcomeMessageText == null)
            {
                Debug.LogError($"[{name}]: Missing UI references in the Unity Inspector. Please assign all fields.");
            }
        }
    }
}