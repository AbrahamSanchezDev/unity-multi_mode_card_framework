using System;
using UnityEngine;
using UnityEngine.UIElements;
using CardFramework.Core.Interfaces;
using VContainer;

namespace CardFramework.Presentation.Views {
    [RequireComponent(typeof(UIDocument))]
    public class BettingModalView : MonoBehaviour {
        public event Action<int> OnBetConfirmed;

        private VisualElement _root;
        private Label _lblCurrentBalance;
        private Label _lblCurrentBet;
        private Button _btnConfirm;

        private IEconomyService _economyService;
        private int _currentBetAmount = 10;
        private const int MinBet = 10;

        [Inject]
        public void Construct(IEconomyService economyService) {
            _economyService = economyService;
        }

        private void OnEnable() {
            SetupUiReferences();
        }
        public void SetupUiReferences() {

            if (_root != null) return;

            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null) return;
            uiDocument.enabled = true;

            // Use Unity's native inspector-linked tree structure safely
            _root = uiDocument.rootVisualElement;

            // Fetch elements via query layout markers
            _lblCurrentBalance = _root.Q<Label>("lbl-balance");
            _lblCurrentBet = _root.Q<Label>("lbl-current-bet");
            _btnConfirm = _root.Q<Button>("btn-confirm-bet");

            // Event Hooks
            _root.Q<Button>("btn-add-10").clicked += OnAdd10Clicked;
            _root.Q<Button>("btn-add-100").clicked += OnAdd100Clicked;
            _root.Q<Button>("btn-add-1000").clicked += OnAdd1kClicked;
            _root.Q<Button>("btn-clear-bet").clicked += ClearBet;
            _root.Q<Button>("btn-max-bet").clicked += SetMaxBet;
            _btnConfirm.clicked += ConfirmBet;

            // Synchronize with server balance immediately if service is already injected
            if (_economyService != null) {
                _economyService.OnBalanceUpdated += UpdateBalanceUI;
                UpdateBalanceUI(_economyService.CurrentGold);
            }

            // Start state hidden
            _root.style.display = DisplayStyle.None;
        }

        private void OnDisable() {
            RemoveUiReferences();
        }
        private void RemoveUiReferences() {

            if (_economyService != null)
                _economyService.OnBalanceUpdated -= UpdateBalanceUI;
            // Safe unhooking to prevent memory leaks or dual-subscription bugs on reload
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

            _currentBetAmount = MinBet;
            if (_root != null)
                _root.style.display = DisplayStyle.Flex;
            if (_economyService != null)
                UpdateBalanceUI(_economyService.CurrentGold);
        }

        private void OnAdd10Clicked() => AdjustBet(10);
        private void OnAdd100Clicked() => AdjustBet(100);
        private void OnAdd1kClicked() => AdjustBet(1000);

        private void AdjustBet(int amount) {
            if (_currentBetAmount + amount <= _economyService.CurrentGold) {
                _currentBetAmount += amount;
                UpdateBetUI();
            }
        }

        private void ClearBet() {
            _currentBetAmount = MinBet;
            UpdateBetUI();
        }

        private void SetMaxBet() {
            _currentBetAmount = _economyService.CurrentGold;
            if (_currentBetAmount < MinBet) _currentBetAmount = MinBet;
            UpdateBetUI();
        }

        private void UpdateBetUI() {
            if (_lblCurrentBet != null) _lblCurrentBet.text = $"{_currentBetAmount} GD";
            if (_btnConfirm != null)
                _btnConfirm.SetEnabled(_currentBetAmount >= MinBet && _currentBetAmount <= _economyService.CurrentGold);
        }

        private void UpdateBalanceUI(int freshBalance) {
            if (_lblCurrentBalance != null) _lblCurrentBalance.text = $"Available: {freshBalance} GD";
            UpdateBetUI();
        }

        private void ConfirmBet() {
            if (_currentBetAmount >= MinBet && _currentBetAmount <= _economyService.CurrentGold) {
                OnBetConfirmed?.Invoke(_currentBetAmount);
                if (_root != null)
                    _root.style.display = DisplayStyle.None;
            }
        }
    }
}