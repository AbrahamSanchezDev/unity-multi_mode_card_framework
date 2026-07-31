using System;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer.Unity;
using CardFramework.Presentation.Views;

namespace CardFramework.Presentation.Controllers {
    public class CardGamesRoomIntroController : IStartable, IDisposable {
        private readonly GameRoomIntroView _introView;
        private readonly GameTableManager _tableManager;
        private readonly BlackjackView _blackjackView;
        private readonly SolitaireView _solitaireView;
        private readonly DashboardMenuView _dashboardView;

        public CardGamesRoomIntroController(GameRoomIntroView introView, GameTableManager tableManager, BlackjackView blackjackView = null, SolitaireView solitaireView = null, DashboardMenuView dashboardView = null) {
            _introView = introView;
            _tableManager = tableManager;
            _blackjackView = blackjackView;
            _solitaireView = solitaireView;
            _dashboardView = dashboardView;
        }

        public void Start() {
            if (_introView != null) {
                _introView.OnOptionSelected += HandleOptionSelected;
                HideGameViews();
                _introView.Show();
            }
        }

        public void Dispose() {
            if (_introView != null) {
                _introView.OnOptionSelected -= HandleOptionSelected;
            }
        }

        private void HandleOptionSelected(string optionId) {
            if (_introView != null) {
                _introView.Hide();
            }

            switch (optionId) {
                case "Blackjack":
                    ShowGameView(_blackjackView);
                    ChangeGameView("Blackjack");
                    break;
                case "Solitaire":
                    ShowGameView(_solitaireView);
                    ChangeGameView("Solitaire");
                    break;
                case "TexasHoldem":
                    ShowGameView(_blackjackView);
                    ChangeGameView("TexasHoldem");
                    break;
                default:
                    Debug.LogWarning($"[CardGamesRoomIntroController] No handler configured for option '{optionId}'.");
                    break;
            }
        }
        private void ChangeGameView(string gameId) {
            _dashboardView?.ChangeActiveGame(gameId);
            _tableManager?.SwitchTable(gameId);
        }

        private void HideGameViews() {
            _solitaireView?.ShowUi(false);
            _blackjackView?.ShowUi(false);
        }

        private void ShowGameView(MonoBehaviour view) {
            HideGameViews();
            SetViewVisible(view, true);
        }

        private void SetViewVisible(MonoBehaviour view, bool visible) {
            Debug.Log($"[CardGamesRoomIntroController] Set view '{view.name}' visibility to {visible}.");
            if (view == null) return;

            var uiDocument = view.GetComponent<UIDocument>();
            if (uiDocument != null) {
                uiDocument.enabled = visible;
                if (uiDocument.rootVisualElement != null) {
                    uiDocument.rootVisualElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }
        }
    }
}
