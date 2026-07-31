using System;
using UnityEngine;
using UnityEngine.UIElements;
using CardFramework.Core.Interfaces;
using VContainer;
using CardFramework.Presentation.Controllers;

namespace CardFramework.Presentation.Views {
    [RequireComponent(typeof(UIDocument))]
    public class BettingModalView : MonoBehaviour {
        public event Action<int> OnBetConfirmed;

        private VisualElement _root;
        private Label _lblCurrentBalance;
        private Label _lblCurrentBet;
        private Button _btnConfirm;

        private IEconomyService _economyService;
        private NavigationController _navigationController;
        
        private int _currentBetAmount = 10;
        private int _activeMinBet = 10;
        private int _activeMaxBet = int.MaxValue;

        private const int DefaultMinBet = 10;

        [Inject]
        public void Construct(IEconomyService economyService, NavigationController navigationController) {
            _economyService = economyService;
            _navigationController = navigationController;
        }

        private void OnEnable() {
            SetupUiReferences();
            if (_navigationController != null)
                _navigationController.OnSwitchGameRequested += HandleGameSwitch;
        }

        public void SetupUiReferences() {
            if (_root != null) return;

            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null) return;
            uiDocument.enabled = true;

            _root = uiDocument.rootVisualElement;
            if (_root == null) return;

            _lblCurrentBalance = _root.Q<Label>("lbl-balance");
            _lblCurrentBet = _root.Q<Label>("lbl-current-bet");
            _btnConfirm = _root.Q<Button>("btn-confirm-bet");

            _root.Q<Button>("btn-add-10").clicked += OnAdd10Clicked;
            _root.Q<Button>("btn-add-100").clicked += OnAdd100Clicked;
            _root.Q<Button>("btn-add-1000").clicked += OnAdd1kClicked;
            _root.Q<Button>("btn-clear-bet").clicked += ClearBet;
            _root.Q<Button>("btn-max-bet").clicked += SetMaxBet;
            _btnConfirm.clicked += ConfirmBet;

            if (_economyService != null) {
                _economyService.OnBalanceUpdated += UpdateBalanceUI;
                UpdateBalanceUI(_economyService.CurrentGold);
            }

            HideModal();
        }

        private void OnDisable() {
            RemoveUiReferences();
            if (_navigationController != null)
                _navigationController.OnSwitchGameRequested -= HandleGameSwitch;
        }

        private void RemoveUiReferences() {
            if (_economyService != null)
                _economyService.OnBalanceUpdated -= UpdateBalanceUI;

            if (_root != null) {
                var btn10 = _root.Q<Button>("btn-add-10"); if (btn10 != null) btn10.clicked -= OnAdd10Clicked;
                var btn100 = _root.Q<Button>("btn-add-100"); if (btn100 != null) btn100.clicked -= OnAdd100Clicked;
                var btn1k = _root.Q<Button>("btn-add-1000"); if (btn1k != null) btn1k.clicked -= OnAdd1kClicked;
                var btnClear = _root.Q<Button>("btn-clear-bet"); if (btnClear != null) btnClear.clicked -= ClearBet;
                var btnMax = _root.Q<Button>("btn-max-bet"); if (btnMax != null) btnMax.clicked -= SetMaxBet;
                if (_btnConfirm != null) _btnConfirm.clicked -= ConfirmBet;
            }
            _root = null;
        }

        [ContextMenu("Show Betting Modal")]
        public void ShowModal() {
            ShowModalWithCap(DefaultMinBet, int.MaxValue);
        }

        /// <summary>
        /// Displays betting modal configured with custom game limits (e.g. 0 to 50 GD for Solitaire).
        /// </summary>
        public void ShowModalWithCap(int minBet, int maxBet) {
            SetupUiReferences();

            _activeMinBet = Mathf.Max(0, minBet);
            _activeMaxBet = maxBet;
            _currentBetAmount = _activeMinBet;

            if (_root != null)
                _root.style.display = DisplayStyle.Flex;

            if (_economyService != null)
                UpdateBalanceUI(_economyService.CurrentGold);
        }

        private void OnAdd10Clicked() => AdjustBet(10);
        private void OnAdd100Clicked() => AdjustBet(100);
        private void OnAdd1kClicked() => AdjustBet(1000);

        private void AdjustBet(int amount) {
            int targetBet = _currentBetAmount + amount;
            int maxAllowed = Mathf.Min(_activeMaxBet, _economyService != null ? _economyService.CurrentGold : targetBet);

            if (targetBet <= maxAllowed) {
                _currentBetAmount = targetBet;
                UpdateBetUI();
            }
        }

        private void ClearBet() {
            _currentBetAmount = _activeMinBet;
            UpdateBetUI();
        }

        private void SetMaxBet() {
            int availableGold = _economyService != null ? _economyService.CurrentGold : 0;
            _currentBetAmount = Mathf.Min(_activeMaxBet, availableGold);
            if (_currentBetAmount < _activeMinBet) _currentBetAmount = _activeMinBet;
            UpdateBetUI();
        }

        private void UpdateBetUI() {
            if (_lblCurrentBet != null) _lblCurrentBet.text = $"{_currentBetAmount} GD";
            
            int maxAllowed = Mathf.Min(_activeMaxBet, _economyService != null ? _economyService.CurrentGold : _currentBetAmount);
            if (_btnConfirm != null)
                _btnConfirm.SetEnabled(_currentBetAmount >= _activeMinBet && _currentBetAmount <= maxAllowed);
        }

        private void UpdateBalanceUI(int freshBalance) {
            if (_lblCurrentBalance != null) _lblCurrentBalance.text = $"Available: {freshBalance} GD";
            UpdateBetUI();
        }

        private void ConfirmBet() {
            int maxAllowed = Mathf.Min(_activeMaxBet, _economyService != null ? _economyService.CurrentGold : _currentBetAmount);
            if (_currentBetAmount >= _activeMinBet && _currentBetAmount <= maxAllowed) {
                OnBetConfirmed?.Invoke(_currentBetAmount);
                HideModal();
            }
        }

        private void HideModal() {
            if (_root != null)
                _root.style.display = DisplayStyle.None;
        }

        private void HandleGameSwitch(string targetGameKey) {
            HideModal();
        }
    }
}