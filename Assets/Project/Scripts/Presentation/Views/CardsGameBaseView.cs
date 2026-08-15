// File: Assets/_Project/Scripts/Presentation/Views/SolitaireView.cs
using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using CardFramework.Presentation.Interfaces;
using CardFramework.Core.Models;
using VContainer;

namespace CardFramework.Presentation.Views {
    [RequireComponent(typeof(UIDocument))]
    public class CardsGameBaseView : MonoBehaviour {
        protected VisualElement _root;

        [SerializeField] protected ViewObj3D view3D;
        protected IAudioService _audioService;

        protected BoxCollider _boxCollider;

        protected INotificationsView _notificationsView;
        public event Action OnMenuRequested;

        protected void Setup3DView() {

            if (view3D) {
                // view3D.ShowGiveUpButton(false);
                view3D.ShowFinalResultDisplay(false);
                view3D.SetFinalResultText(string.Empty);

                view3D.SetupView(
                    onMainAction: HandleMainButtonClicked,
                    onGiveUp: HandleGiveUpClicked,
                    onStartNewGame: HandleNewGameClicked,
                    onMenu: HandleMenuClicked,
                    onEmail: HandleEmailClicked
                );
            }
        }

        protected virtual void HandleMainButtonClicked() {
            Debug.Log("[CardsGameBaseView] HandleMainButtonClicked called.");
        }

        protected virtual void HandleGiveUpClicked() {
            Debug.Log("[CardsGameBaseView] HandleGiveUpClicked called.");
        }

        protected virtual void HandleNewGameClicked() {
            Debug.Log("[CardsGameBaseView] HandleNewGameClicked called.");
        }

        protected virtual void HandleMenuClicked() {
            PlayButtonClickSound();
            OnMenuRequested?.Invoke();
        }

        protected virtual void HandleEmailClicked() {
            _notificationsView?.ToggleNotificationDisplay();
        }

        protected void Show3DView(bool show) {
            if (view3D) {
                view3D.gameObject.SetActive(show);
            }
        }

        protected void ShowGiveUpButton(bool show) {
            if (view3D) {
                view3D.ShowGiveUpButton(show);
            }
        }

        protected void ShowNewGameButton(bool show) {
            if (view3D) {
                view3D.ShowNewGameButton(show);
            }
        }

        protected void ShowFinalResult(string resultText) {
            if (view3D) {
                view3D.SetFinalResultText(resultText);
                view3D.ShowFinalResultDisplay(true);
            }
        }

        protected void UpdateBalanceDisplay(string newBalance) {
            if (view3D) {
                view3D.UpdateCurrencyDisplay(newBalance);
            }
        }

        protected void UpdateScoreDisplay(string newScore) {
            if (view3D) {
                view3D.UpdateScoreDisplay(newScore);
            }
        }

        protected void UpdateFinalResultText(string resultText) {
            if (view3D) {
                view3D.SetFinalResultText(resultText);
            }
        }

        protected void ShowFinalResultDisplay(bool show) {
            if (view3D) {
                view3D.ShowFinalResultDisplay(show);
            }
        }


        protected void PlayButtonClickSound() {
            _audioService?.PlayButtonClick();
        }


        public void ShowUi(bool show) {
            if (_root != null) {
                _root.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            }
            if (_boxCollider)
                _boxCollider.enabled = show;

            if (view3D) {
                view3D.gameObject.SetActive(show);
            }
        }
    }
}