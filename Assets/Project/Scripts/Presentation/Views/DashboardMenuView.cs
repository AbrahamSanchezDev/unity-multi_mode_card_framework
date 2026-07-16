using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using CardFramework.Cloud.Interfaces;
using VContainer;

namespace CardFramework.Presentation.Views {
    [RequireComponent(typeof(UIDocument))]
    public class DashboardMenuView : MonoBehaviour {
        public event Action OnCloseRequested;
        public event Action OnLinkAccountRequested;
        public event Action OnExitApplicationRequested;
        public event Action<string> OnGameSwitchRequested;

        private VisualElement _root;
        private Label _lblAccountStatus;

        private Button _btnOpenLinking;
        private Button _btnGeneratePin;
        private Button _btnSubmitPin;
        private Label _lblGeneratedPin;
        private TextField _txtInputPin;

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
            if (btnBlackjack != null) btnBlackjack.clicked += () => OnGameSwitchRequested?.Invoke("Blackjack");
            if (btnSolitaire != null) btnSolitaire.clicked += () => OnGameSwitchRequested?.Invoke("Solitaire");
            if (btnTexasHoldem != null) btnTexasHoldem.clicked += () => OnGameSwitchRequested?.Invoke("TexasHoldem");

            _root.Q<Button>("btn-close-dash").clicked += () => OnCloseRequested?.Invoke();
            _root.Q<Button>("btn-open-linking").clicked += () => OnLinkAccountRequested?.Invoke();
            _root.Q<Button>("btn-exit-app").clicked += () => OnExitApplicationRequested?.Invoke();

            // Set default runtime visual highlighting state
            UpdateActiveGameVisuals("Blackjack");

            _root.style.display = DisplayStyle.None;
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

        #region Linking Panel Logic

        [Inject]
        public void InitializePresenter(ICloudService cloudService) {
            _cloudService = cloudService;

            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null || uiDocument.rootVisualElement == null) {
                Debug.LogWarning("[Sync] UIDocument or rootVisualElement not found.");
                return;
            }
            uiDocument.enabled = true;

            var root = uiDocument.rootVisualElement;

            // Query elements matching your UXML identifiers
            _btnOpenLinking = root.Q<Button>("btn-open-linking");
            _btnGeneratePin = root.Q<Button>("btn-generate-pin");
            _btnSubmitPin = root.Q<Button>("btn-submit-pin");
            _lblGeneratedPin = root.Q<Label>("lbl-generated-pin");
            _txtInputPin = root.Q<TextField>("txt-input-pin");

            _linkingModalOverlay = root.Q<VisualElement>("linking-modal-overlay");
            _btnCloseModal = root.Q<Button>("btn-close-modal");

            // Modal Interaction Events
            if (_btnOpenLinking != null) _btnOpenLinking.clicked += () => OpenLinkingModal(true);
            else Debug.LogWarning("[Sync] _btnOpenLinking not found in the visual tree.");

            if (_btnCloseModal != null) _btnCloseModal.clicked += () => OpenLinkingModal(false);
            else Debug.LogWarning("[Sync] _btnCloseModal not found in the visual tree.");

            // Wire Events
            if (_btnGeneratePin != null) _btnGeneratePin.clicked += OnGeneratePinClicked;
            else Debug.LogWarning("[Sync] btn-generate-pin not found in the visual tree.");

            if (_btnSubmitPin != null) _btnSubmitPin.clicked += OnSubmitPinClicked;
            else Debug.LogWarning("[Sync] btn-submit-pin not found in the visual tree.");

            if (_txtInputPin != null) {
                _txtInputPin.RegisterValueChangedCallback(evt => OnInputPinValueChanged(evt));
            }
        }

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