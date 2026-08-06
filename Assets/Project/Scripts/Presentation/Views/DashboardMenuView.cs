using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using CardFramework.Cloud.Interfaces;
using VContainer;
using CardFramework.Presentation.Interfaces;

namespace CardFramework.Presentation.Views {
    [RequireComponent(typeof(UIDocument))]
    public class DashboardMenuView : MonoBehaviour {
        public event Action OnCloseRequested;
        public event Action OnLinkAccountRequested;
        public event Action OnExitApplicationRequested;
        public event Action<string> OnGameSwitchRequested;
        public event Action<CardDisplayType> OnCardDisplayTypeChangeRequested;

        private VisualElement _root;
        private Label _lblAccountStatus;

        private Button _btnOpenLinking;
        private Button _btnGeneratePin;
        private Button _btnSubmitPin;
        private Label _lblGeneratedPin;
        private TextField _txtInputPin;

        private Button _btnOpenSettings;
        private VisualElement _settingsModalOverlay;
        private Button _btnCloseSettingsModal;
        private Button _btnCardDisplayFullCard;
        private Button _btnCardDisplayEasyRead;
        private Button _btnCardDisplayImagesGirls;
        private Button _btnCardDisplayEasyReadGirls;
        private Button _btnCardDisplayEasyReadGirlsBg;
        private Button _btnCardDisplayFullBackground;
        private VisualElement _linkingModalOverlay;
        private Button _btnCloseModal;

        // Visual collection mapping game name signatures straight to their button instances
        private Dictionary<string, Button> _gameButtonsMap;

        // Track clean default base text values to safely reconstruct labels during visual update shifts
        private readonly Dictionary<string, string> _gameBaseLabels = new Dictionary<string, string> {
            { "Blackjack", "BLACKJACK" },
            { "Solitaire", "SOLITAIRE" },
            { "TexasHoldem", "TEXAS HOLD'EM" }
        };

        // Style class constants from your USS stylesheets
        private const string ActiveClassName = "game-active";
        private const string LockedClassName = "game-locked";

        private ICloudService _cloudService;
        private IGameSettingsService _gameSettingsService;
        private IAudioService _audioService;

        private void OnEnable() {
            InitUi();
        }

        public void InitUi() {
            if (_root != null) return; // Already initialized
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null) return;
            uiDocument.enabled = true;

            _root = uiDocument.rootVisualElement;
            _lblAccountStatus = _root.Q<Label>("lbl-account-status");

            _btnOpenLinking = _root.Q<Button>("btn-open-linking");
            _btnGeneratePin = _root.Q<Button>("btn-generate-pin");
            _btnSubmitPin = _root.Q<Button>("btn-submit-pin");
            _lblGeneratedPin = _root.Q<Label>("lbl-generated-pin");
            _txtInputPin = _root.Q<TextField>("txt-input-pin");
            _linkingModalOverlay = _root.Q<VisualElement>("linking-modal-overlay");
            _btnCloseModal = _root.Q<Button>("btn-close-modal");

            _btnOpenSettings = _root.Q<Button>("btn-open-settings");
            _settingsModalOverlay = _root.Q<VisualElement>("settings-modal-overlay");
            _btnCloseSettingsModal = _root.Q<Button>("btn-close-settings-modal");
            _btnCardDisplayFullCard = _root.Q<Button>("btn-carddisplay-fullcard");
            _btnCardDisplayEasyRead = _root.Q<Button>("btn-carddisplay-easyread");
            _btnCardDisplayImagesGirls = _root.Q<Button>("btn-carddisplay-imagesgirls");
            _btnCardDisplayEasyReadGirls = _root.Q<Button>("btn-carddisplay-easyreadgirls");
            _btnCardDisplayEasyReadGirlsBg = _root.Q<Button>("btn-carddisplay-easyreadgirls-bg");
            _btnCardDisplayFullBackground = _root.Q<Button>("btn-carddisplay-fullbackground");

            // Query game select buttons from visual tree
            var btnBlackjack = _root.Q<Button>("btn-game-blackjack");
            var btnSolitaire = _root.Q<Button>("btn-game-solitaire");
            var btnTexasHoldem = _root.Q<Button>("btn-game-texasholdem");

            // Initialize structural map coupling signatures to reference elements
            _gameButtonsMap = new Dictionary<string, Button>(StringComparer.OrdinalIgnoreCase) {
                { "Blackjack", btnBlackjack },
                { "Solitaire", btnSolitaire },
                { "TexasHoldem", btnTexasHoldem }
            };

            // Wire UI Toolkit Interactions straight to architecture events
            if (btnBlackjack != null) btnBlackjack.clicked += () => HandleGameSwitchClicked("Blackjack");
            if (btnSolitaire != null) btnSolitaire.clicked += () => HandleGameSwitchClicked("Solitaire");
            if (btnTexasHoldem != null) btnTexasHoldem.clicked += () => HandleGameSwitchClicked("TexasHoldem");

            if (_btnOpenLinking != null) _btnOpenLinking.clicked += HandleOpenLinkingClicked;
            if (_btnOpenSettings != null) _btnOpenSettings.clicked += HandleOpenSettingsClicked;
            if (_btnCloseModal != null) _btnCloseModal.clicked += () => OpenLinkingModal(false);
            if (_btnCloseSettingsModal != null) _btnCloseSettingsModal.clicked += () => OpenSettingsModal(false);

            if (_btnCardDisplayFullCard != null) _btnCardDisplayFullCard.clicked += () => HandleCardDisplaySelected(CardDisplayType.FullCard);
            if (_btnCardDisplayEasyRead != null) _btnCardDisplayEasyRead.clicked += () => HandleCardDisplaySelected(CardDisplayType.EasyRead);
            if (_btnCardDisplayImagesGirls != null) _btnCardDisplayImagesGirls.clicked += () => HandleCardDisplaySelected(CardDisplayType.ImagesGirls);
            if (_btnCardDisplayEasyReadGirls != null) _btnCardDisplayEasyReadGirls.clicked += () => HandleCardDisplaySelected(CardDisplayType.EasyReadGirls);
            if (_btnCardDisplayEasyReadGirlsBg != null) _btnCardDisplayEasyReadGirlsBg.clicked += () => HandleCardDisplaySelected(CardDisplayType.EasyReadGirlsAsBackground);
            if (_btnCardDisplayFullBackground != null) _btnCardDisplayFullBackground.clicked += () => HandleCardDisplaySelected(CardDisplayType.FullBackground);

            if (_btnGeneratePin != null) _btnGeneratePin.clicked += OnGeneratePinClicked;
            else Debug.LogWarning("[Sync] btn-generate-pin not found in the visual tree.");

            if (_btnSubmitPin != null) _btnSubmitPin.clicked += OnSubmitPinClicked;
            else Debug.LogWarning("[Sync] btn-submit-pin not found in the visual tree.");

            if (_txtInputPin != null) {
                _txtInputPin.RegisterValueChangedCallback(evt => OnInputPinValueChanged(evt));
            }

            _root.Q<Button>("btn-close-dash").clicked += HandleCloseDashboardClicked;
            _root.Q<Button>("btn-exit-app").clicked += HandleExitAppClicked;

            if (_gameSettingsService != null) {
                SetSelectedCardDisplayType(_gameSettingsService.CardDisplayType);
            }

            // Set default runtime visual highlighting state
            UpdateActiveGameVisuals("Blackjack");

            _root.style.display = DisplayStyle.None;
        }


        [Inject]
        public void Construct(IAudioService audioService, IGameSettingsService gameSettingsService, ICloudService cloudService) {
            _audioService = audioService;
            _gameSettingsService = gameSettingsService;
            _cloudService = cloudService;

            if (_root != null && _gameSettingsService != null) {
                SetSelectedCardDisplayType(_gameSettingsService.CardDisplayType);
            }
        }

        public void ChangeActiveGame(string gameId) {
            UpdateActiveGameVisuals(gameId);
            OnGameSwitchRequested?.Invoke(gameId);
        }

        private void HandleGameSwitchClicked(string gameId) {
            PlayButtonClickSound();
            ChangeActiveGame(gameId);
        }

        private void HandleCloseDashboardClicked() {
            PlayButtonClickSound();
            OnCloseRequested?.Invoke();
        }

        private void HandleOpenLinkingClicked() {
            PlayButtonClickSound();
            OnLinkAccountRequested?.Invoke();
        }

        private void HandleExitAppClicked() {
            PlayButtonClickSound();
            OnExitApplicationRequested?.Invoke();
        }

        private void PlayButtonClickSound() {
            _audioService?.PlayButtonClick();
        }

        /// <summary>
        /// Task-4.3.1 Carousel Polish: Iterates over the cached buttons map to apply 
        /// the active and locked USS style classes reactively.
        /// </summary>
        public void UpdateActiveGameVisuals(string activeGameKey) {
            if (_gameButtonsMap == null) return;

            foreach (var kvp in _gameButtonsMap) {
                Button targetButton = kvp.Value;
                if (targetButton == null) continue;

                bool isActive = kvp.Key.Equals(activeGameKey, StringComparison.OrdinalIgnoreCase);

                // Fetch the clean default text layout signature
                _gameBaseLabels.TryGetValue(kvp.Key, out string baseLabel);
                if (string.IsNullOrEmpty(baseLabel)) baseLabel = kvp.Key.ToUpper();

                if (isActive) {
                    targetButton.text = $"{baseLabel} (ACTIVE)";

                    // Manage style classes natively
                    if (!targetButton.ClassListContains(ActiveClassName)) {
                        targetButton.AddToClassList(ActiveClassName);
                    }
                    targetButton.RemoveFromClassList(LockedClassName);
                }
                else {
                    targetButton.text = baseLabel;

                    // Manage style classes natively
                    if (!targetButton.ClassListContains(LockedClassName)) {
                        targetButton.AddToClassList(LockedClassName);
                    }
                    targetButton.RemoveFromClassList(ActiveClassName);
                }
            }
        }

        public void ShowDashboard(string statusText) {
            if (_lblAccountStatus != null) _lblAccountStatus.text = statusText;
            if (_root != null) _root.style.display = DisplayStyle.Flex;
        }

        public void HideDashboard() {
            if (_root != null) _root.style.display = DisplayStyle.None;
        }

        public void SetSelectedCardDisplayType(CardDisplayType displayType) {
            ClearCardDisplaySelection();
            switch (displayType) {
                case CardDisplayType.FullCard:
                    _btnCardDisplayFullCard?.AddToClassList("selected");
                    break;
                case CardDisplayType.EasyRead:
                    _btnCardDisplayEasyRead?.AddToClassList("selected");
                    break;
                case CardDisplayType.ImagesGirls:
                    _btnCardDisplayImagesGirls?.AddToClassList("selected");
                    break;
                case CardDisplayType.EasyReadGirls:
                    _btnCardDisplayEasyReadGirls?.AddToClassList("selected");
                    break;
                case CardDisplayType.EasyReadGirlsAsBackground:
                    _btnCardDisplayEasyReadGirlsBg?.AddToClassList("selected");
                    break;
                case CardDisplayType.FullBackground:
                    _btnCardDisplayFullBackground?.AddToClassList("selected");
                    break;
            }
        }

        private void ClearCardDisplaySelection() {
            _btnCardDisplayFullCard?.RemoveFromClassList("selected");
            _btnCardDisplayEasyRead?.RemoveFromClassList("selected");
            _btnCardDisplayImagesGirls?.RemoveFromClassList("selected");
            _btnCardDisplayEasyReadGirls?.RemoveFromClassList("selected");
            _btnCardDisplayEasyReadGirlsBg?.RemoveFromClassList("selected");
            _btnCardDisplayFullBackground?.RemoveFromClassList("selected");
        }

        private void HandleOpenSettingsClicked() {
            PlayButtonClickSound();
            OpenSettingsModal(true);
        }

        private void OpenSettingsModal(bool open) {
            if (_settingsModalOverlay == null) return;
            _settingsModalOverlay.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void HandleCardDisplaySelected(CardDisplayType displayType) {
            PlayButtonClickSound();
            SetSelectedCardDisplayType(displayType);
            if (_gameSettingsService != null) {
                _gameSettingsService.CardDisplayType = displayType;
                _gameSettingsService.Save();
            }
            OnCardDisplayTypeChangeRequested?.Invoke(displayType);
            OpenSettingsModal(false);
        }

        #region Linking Panel Logic

        private void ToggleLinkingPanel() {
            if (_linkingModalOverlay == null) {
                Debug.LogWarning("[Sync] _linkingModalOverlay not found in the visual tree.");
                return;
            }
            bool isHidden = _linkingModalOverlay.style.display.value == DisplayStyle.None;
            _linkingModalOverlay.style.display = isHidden ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OpenLinkingModal(bool open) {
            if (_linkingModalOverlay == null) return;
            _linkingModalOverlay.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;

            if (open) {
                ResetInputVisualState();
                if (_txtInputPin != null) _txtInputPin.value = string.Empty;
            }
        }

        private async void OnGeneratePinClicked() {
            if (_lblGeneratedPin == null || _btnGeneratePin == null) return;

            _lblGeneratedPin.text = "REQ...";
            _btnGeneratePin.SetEnabled(false);

            try {
                string pin = await _cloudService.GenerateLinkingPINAsync();
                _lblGeneratedPin.text = pin;
                Debug.Log($"[Sync] Generated linking PIN: {pin}");
            }
            catch (Exception ex) {
                Debug.LogError($"[Sync] Failed to generate recovery pin: {ex.Message}");
                _lblGeneratedPin.text = "ERROR";
            }
            finally {
                _btnGeneratePin.SetEnabled(true);
            }
        }

        private async void OnSubmitPinClicked() {
            if (_txtInputPin == null || _btnSubmitPin == null || _btnOpenLinking == null) return;

            ResetInputVisualState();
            string rawPin = _txtInputPin.value?.Trim().ToUpper();

            if (string.IsNullOrEmpty(rawPin) || rawPin.Length != 6) {
                ApplyInputVisualError("INVALID LENGTH");
                Debug.LogWarning("[Sync] PIN validation aborted: string must be exactly 6 characters.");
                return;
            }

            _btnSubmitPin.SetEnabled(false);
            _btnSubmitPin.text = "VERIFYING...";

            try {
                bool success = await _cloudService.LinkAccountWithPINAsync(rawPin);
                if (success) {
                    if (_linkingModalOverlay != null) _linkingModalOverlay.style.display = DisplayStyle.None;
                    _btnOpenLinking.text = "ACCOUNT LINKED SUCCESSFULLY ✔";
                    _btnOpenLinking.SetEnabled(false);
                }
                else {
                    ApplyInputVisualError("PIN NOT FOUND");
                }
            }
            catch (Exception ex) {
                Debug.LogError($"[Sync] Link verification exception: {ex}");
                ApplyInputVisualError("NET ERROR");
            }
            finally {
                _btnSubmitPin.SetEnabled(true);
            }
        }

        private void OnInputPinValueChanged(ChangeEvent<string> evt) {
            if (evt.newValue == null) return;

            ResetInputVisualState();

            string cleanedText = Regex.Replace(evt.newValue, @"[^a-zA-Z0-9]", "");
            cleanedText = cleanedText.ToUpper();

            if (evt.newValue != cleanedText) {
                _txtInputPin.SetValueWithoutNotify(cleanedText);
            }
        }

        private void ApplyInputVisualError(string errorMessage) {
            if (_txtInputPin == null || _btnSubmitPin == null) return;

            _txtInputPin.style.borderTopColor = Color.red;
            _txtInputPin.style.borderBottomColor = Color.red;
            _txtInputPin.style.borderLeftColor = Color.red;
            _txtInputPin.style.borderRightColor = Color.red;
            _txtInputPin.style.borderTopWidth = 1.5f;
            _txtInputPin.style.borderBottomWidth = 1.5f;
            _txtInputPin.style.borderLeftWidth = 1.5f;
            _txtInputPin.style.borderRightWidth = 1.5f;

            _btnSubmitPin.text = $"✕ {errorMessage}";
            _btnSubmitPin.style.backgroundColor = new Color(0.75f, 0.22f, 0.17f);
        }

        private void ResetInputVisualState() {
            if (_txtInputPin == null || _btnSubmitPin == null) return;

            _txtInputPin.style.borderTopColor = StyleKeyword.Null;
            _txtInputPin.style.borderBottomColor = StyleKeyword.Null;
            _txtInputPin.style.borderLeftColor = StyleKeyword.Null;
            _txtInputPin.style.borderRightColor = StyleKeyword.Null;
            _txtInputPin.style.borderTopWidth = StyleKeyword.Null;
            _txtInputPin.style.borderBottomWidth = StyleKeyword.Null;
            _txtInputPin.style.borderLeftWidth = StyleKeyword.Null;
            _txtInputPin.style.borderRightWidth = StyleKeyword.Null;

            _btnSubmitPin.text = "LINK DEVICE";
            _btnSubmitPin.style.backgroundColor = StyleKeyword.Null;
        }

        #endregion
    }
}