using UnityEngine;

namespace CardFramework.Presentation.Views {

    public class ViewObj3D : MonoBehaviour {
        [SerializeField] private ButtonObj3D gameMainActionButton;
        [SerializeField] private ButtonObj3D gameGiveUpButton;
        [SerializeField] private ButtonObj3D gameStartNewGameButton;
        [SerializeField] private ButtonObj3D gameMenuButton;
        [SerializeField] private ButtonObj3D EmailButton;

        [SerializeField] private DisplayObj3D gameScoreDisplay;
        [SerializeField] private DisplayObj3D gameCurrencyDisplay;
        [SerializeField] private DisplayObj3D gameFinalResultDisplay;

        public void SetupView(System.Action onMainAction, System.Action onGiveUp, System.Action onStartNewGame, System.Action onMenu, System.Action onEmail) {
            gameMainActionButton.SetupButton(onMainAction);
            gameGiveUpButton.SetupButton(onGiveUp);
            gameStartNewGameButton.SetupButton(onStartNewGame);
            gameMenuButton.SetupButton(onMenu);
            EmailButton.SetupButton(onEmail);
        }

        public void ShowGiveUpButton(bool show) {
            gameGiveUpButton.gameObject.SetActive(show);
        }

        public void ShowNewGameButton(bool show) {
            gameStartNewGameButton.gameObject.SetActive(show);
        }

        public void ShowFinalResultDisplay(bool show) {
            gameFinalResultDisplay?.gameObject.SetActive(show);
        }
        public void SetFinalResultText(string text) {
            gameFinalResultDisplay?.SetDisplayText(text);
        }

        public void UpdateScoreDisplay(string newScore) {
            gameScoreDisplay?.SetDisplayText(newScore);
        }

        public void UpdateCurrencyDisplay(string newCurrency) {
            gameCurrencyDisplay?.SetDisplayText(newCurrency);
        }
    }
}