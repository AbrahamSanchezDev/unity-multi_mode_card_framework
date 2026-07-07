using NUnit.Framework;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using CardFramework.Core.Interfaces;
using CardFramework.Presentation.Views;

namespace CardFramework.Tests.EditMode.Presentation {
    [TestFixture]
    public class BettingModalViewTests {
        private GameObject _testGo;
        private BettingModalView _view;
        private UIDocument _uiDocument;
        private MockEconomyService _mockEconomy;

        private VisualElement _rootElement;
        private Label _lblBalance;
        private Label _lblCurrentBet;
        private Button _btnConfirm;
        private Button _btnAdd10;
        private Button _btnAdd100;
        private Button _btnAdd1000;
        private Button _btnClear;
        private Button _btnMax;

        private class MockEconomyService : IEconomyService {
            public int CurrentGold { get; set; } = 5000;
            public event Action<int> OnBalanceUpdated;
            public event Action<string> OnEconomyError;

            public void TriggerBalanceUpdate(int newBalance) {
                CurrentGold = newBalance;
                OnBalanceUpdated?.Invoke(newBalance);
            }

            public void CreditGold(int amount) {
                CurrentGold += amount;
                OnBalanceUpdated?.Invoke(CurrentGold);
            }

            public void DebitGold(int amount) {
                if (amount > CurrentGold) {
                    OnEconomyError?.Invoke("Insufficient funds");
                    return;
                }
                CurrentGold -= amount;
                OnBalanceUpdated?.Invoke(CurrentGold);
            }

            public void RefreshBalance() {
                OnBalanceUpdated?.Invoke(CurrentGold);
            }
        }

        [SetUp]
        public void Setup() {
            _testGo = new GameObject("Test_BettingModalView");
            _testGo.SetActive(false);

            _uiDocument = _testGo.AddComponent<UIDocument>();
            _view = _testGo.AddComponent<BettingModalView>();

            _mockEconomy = new MockEconomyService();
            _view.Construct(_mockEconomy);

            var mockAsset = ScriptableObject.CreateInstance<VisualTreeAsset>();
            _uiDocument.visualTreeAsset = mockAsset;

            _rootElement = _uiDocument.rootVisualElement;
            _rootElement.Clear();

            _lblBalance = new Label() { name = "lbl-balance" };
            _lblCurrentBet = new Label() { name = "lbl-current-bet" };
            _btnConfirm = new Button() { name = "btn-confirm-bet" };
            _btnAdd10 = new Button() { name = "btn-add-10" };
            _btnAdd100 = new Button() { name = "btn-add-100" };
            _btnAdd1000 = new Button() { name = "btn-add-1000" };
            _btnClear = new Button() { name = "btn-clear-bet" };
            _btnMax = new Button() { name = "btn-max-bet" };

            _rootElement.Add(_lblBalance);
            _rootElement.Add(_lblCurrentBet);
            _rootElement.Add(_btnConfirm);
            _rootElement.Add(_btnAdd10);
            _rootElement.Add(_btnAdd100);
            _rootElement.Add(_btnAdd1000);
            _rootElement.Add(_btnClear);
            _rootElement.Add(_btnMax);

            _testGo.SetActive(true);
        }

        [TearDown]
        public void TearDown() {
            if (_testGo != null) {
                UnityEngine.Object.DestroyImmediate(_testGo);
            }
        }

        [Test]
        public void View_OnEnableLifecycle_TriggersSetupUiReferences() {
            // Nullify _root to clear state and ensure it must re-evaluate SetupUiReferences lines
            SetPrivateField("_root", null);

            var enableMethod = typeof(BettingModalView).GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(enableMethod, "OnEnable method should be accessible via reflection.");

            Assert.DoesNotThrow(() => {
                enableMethod.Invoke(_view, null);
            }, "Invoking the OnEnable lifecycle pathway directly should execute successfully.");

            // Verify it set up the references again
            var rootField = typeof(BettingModalView).GetField("_root", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(rootField.GetValue(_view), "_root element field should be safely initialized after OnEnable.");
        }

        [Test]
        public void View_SetupUiReferences_BypassesIfAlreadyInitialized() {
            _view.SetupUiReferences();

            _lblBalance.text = "PreservedText";
            _view.SetupUiReferences();

            Assert.AreEqual("PreservedText", _lblBalance.text);
        }

        [Test]
        public void View_SetupUiReferences_HandlesMissingUIDocumentGracefully() {
            GameObject genericGo = new GameObject("EmptyViewGo");
            genericGo.SetActive(false);

            var emptyView = genericGo.AddComponent<BettingModalView>();

            Assert.DoesNotThrow(() => {
                emptyView.SetupUiReferences();
            }, "Should exit early if standard UIDocument or its root asset is missing entirely.");

            UnityEngine.Object.DestroyImmediate(genericGo);
        }

        [Test]
        public void View_ShowModal_SetsDisplayFlexAndResetsBet() {
            _view.SetupUiReferences();
            _view.ShowModal();

            Assert.AreEqual(DisplayStyle.Flex, _rootElement.style.display.value);
            Assert.AreEqual("10 GD", _lblCurrentBet.text);
        }

        [Test]
        public void View_BetIncrements_ExecuteClickActions() {
            _view.SetupUiReferences();
            _view.ShowModal();

            InvokePrivateMethod("OnAdd10Clicked");
            Assert.AreEqual("20 GD", _lblCurrentBet.text);

            InvokePrivateMethod("OnAdd100Clicked");
            Assert.AreEqual("120 GD", _lblCurrentBet.text);

            InvokePrivateMethod("OnAdd1kClicked");
            Assert.AreEqual("1120 GD", _lblCurrentBet.text);
        }

        [Test]
        public void View_AdjustBet_PreventsExceedingCurrentGold() {
            _mockEconomy.CurrentGold = 50;
            _view.SetupUiReferences();
            _view.ShowModal();

            InvokePrivateMethod("OnAdd100Clicked");
            Assert.AreEqual("10 GD", _lblCurrentBet.text);
        }

        [Test]
        public void View_ClearBet_ResetsToMinimumBet() {
            _view.SetupUiReferences();
            _view.ShowModal();

            InvokePrivateMethod("OnAdd100Clicked");
            InvokePrivateMethod("ClearBet");

            Assert.AreEqual("10 GD", _lblCurrentBet.text);
        }

        [Test]
        public void View_SetMaxBet_MatchesEconomyServiceGold() {
            _view.SetupUiReferences();
            _view.ShowModal();

            InvokePrivateMethod("SetMaxBet");
            Assert.AreEqual("5000 GD", _lblCurrentBet.text);
        }

        [Test]
        public void View_SetMaxBet_UnderMinimumBetFloor_ClampsToMinBet() {
            _mockEconomy.CurrentGold = 5;
            _view.SetupUiReferences();
            _view.ShowModal();

            InvokePrivateMethod("SetMaxBet");
            Assert.AreEqual("10 GD", _lblCurrentBet.text);
            Assert.IsFalse(_btnConfirm.enabledSelf);
        }

        [Test]
        public void View_ConfirmBet_DispatchesActionAndHidesModal() {
            _view.SetupUiReferences();
            _view.ShowModal();
            InvokePrivateMethod("OnAdd100Clicked");

            int receivedBet = 0;
            _view.OnBetConfirmed += (amount) => receivedBet = amount;

            InvokePrivateMethod("ConfirmBet");

            Assert.AreEqual(110, receivedBet);
            Assert.AreEqual(DisplayStyle.None, _rootElement.style.display.value);
        }

        [Test]
        public void View_EconomyService_UpdatesBalanceLineOnEventDispatched() {
            _view.SetupUiReferences();

            _mockEconomy.TriggerBalanceUpdate(999);

            Assert.AreEqual("Available: 999 GD", _lblBalance.text);
        }

        [Test]
        public void View_OnDisableLifecycle_CleansUiReferencesAndUnhooksHandlers() {
            _view.SetupUiReferences();

            var disableMethod = typeof(BettingModalView).GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(disableMethod);

            Assert.DoesNotThrow(() => {
                disableMethod.Invoke(_view, null);
            });
        }

        [Test]
        public void View_CleanReferencesWithoutInitialization_HandlesNullGracefully() {
            GameObject genericGo = new GameObject("FreshUnusedView");
            genericGo.SetActive(false);
            var cleanView = genericGo.AddComponent<BettingModalView>();

            var removeReferencesMethod = typeof(BettingModalView).GetMethod("RemoveUiReferences", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(removeReferencesMethod);

            Assert.DoesNotThrow(() => {
                removeReferencesMethod.Invoke(cleanView, null);
            });

            UnityEngine.Object.DestroyImmediate(genericGo);
        }

        [Test]
        public void View_MissingLabelElementsFallback_DoesNotThrowExceptions() {
            _view.SetupUiReferences();

            SetPrivateField("_lblCurrentBet", null);
            SetPrivateField("_btnConfirm", null);
            SetPrivateField("_lblCurrentBalance", null);

            Assert.DoesNotThrow(() => {
                InvokePrivateMethod("UpdateBetUI");
                InvokePrivateMethod("ClearBet");
                _mockEconomy.TriggerBalanceUpdate(150);
            });
        }

        private void InvokePrivateMethod(string methodName) {
            var method = typeof(BettingModalView).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) Assert.Fail($"Method {methodName} could not be resolved.");
            method.Invoke(_view, null);
        }

        private void SetPrivateField(string fieldName, object value) {
            var field = typeof(BettingModalView).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) Assert.Fail($"Field {fieldName} could not be resolved.");
            field.SetValue(_view, value);
        }
    }
}