using NUnit.Framework;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using CardFramework.Presentation.Views;

namespace CardFramework.Tests.EditMode.Presentation {
    [TestFixture]
    public class DashboardMenuViewTests : ViewClassForTests {
        private GameObject _testGo;
        private DashboardMenuView _view;
        private UIDocument _uiDocument;

        private VisualElement _rootElement;
        private Label _lblAccountStatus;
        private Button _btnCloseDash;
        private Button _btnOpenLinking;
        private Button _btnExitApp;
        private Button _btnGameBlackjack;
        private Button _btnGameSolitaire;
        private Button _btnGameTexasHoldem;

        [SetUp]
        public void Setup() {
            // 1. Create GameObject in an inactive state to coordinate initialization sequence cleanly
            _testGo = new GameObject("Test_DashboardMenuView");
            _testGo.SetActive(false);

            _uiDocument = _testGo.AddComponent<UIDocument>();
            _view = _testGo.AddComponent<DashboardMenuView>();

            // 2. Provision an empty ScriptableObject tree structure to isolate mockup elements
            var mockAsset = ScriptableObject.CreateInstance<VisualTreeAsset>();
            _uiDocument.visualTreeAsset = mockAsset;
            _rootElement = _uiDocument.rootVisualElement;
            _rootElement.Clear();

            // 3. Instantiate UI Toolkit nodes mapping target query design markers (Q<T>)
            _lblAccountStatus = new Label() { name = "lbl-account-status" };
            _btnCloseDash = new Button() { name = "btn-close-dash" };
            _btnOpenLinking = new Button() { name = "btn-open-linking" };
            _btnExitApp = new Button() { name = "btn-exit-app" };
            _btnGameBlackjack = new Button() { name = "btn-game-blackjack" };
            _btnGameSolitaire = new Button() { name = "btn-game-solitaire" };
            _btnGameTexasHoldem = new Button() { name = "btn-game-texasholdem" };

            _rootElement.Add(_lblAccountStatus);
            _rootElement.Add(_btnCloseDash);
            _rootElement.Add(_btnOpenLinking);
            _rootElement.Add(_btnExitApp);
            _rootElement.Add(_btnGameBlackjack);
            _rootElement.Add(_btnGameSolitaire);
            _rootElement.Add(_btnGameTexasHoldem);

            // 4. Force awake initialization to execute InitUi completely while _root is still null.
            // This guarantees every button binds its lambda event listeners properly!
            var enableMethod = typeof(DashboardMenuView).GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic);
            enableMethod.Invoke(_view, null);

            // 5. Override cached private fields AFTER initialization to map our active test references cleanly
            SetPrivateField(_view, "_root", _rootElement);
            SetPrivateField(_view, "_lblAccountStatus", _lblAccountStatus);

            _testGo.SetActive(true);
        }

        [TearDown]
        public void TearDown() {
            if (_testGo != null) {
                UnityEngine.Object.DestroyImmediate(_testGo);
            }
        }

        [Test]
        public void View_InitUi_BindsElementsAndSetsDisplayToNone() {
            // Force reset and call layout sequence directly
            SetPrivateField(_view, "_root", null);
            _view.InitUi();
            
            Assert.AreEqual(DisplayStyle.None, _rootElement.style.display.value, "The dashboard should default its style visibility layout to DisplayStyle.None on initialization.");
        }

        [Test]
        public void View_InitUi_ShortCircuitsWhenAlreadyInitialized() {
            _rootElement.style.display = DisplayStyle.Flex;
            
            // Should short circuit immediately since _root is already cached by Setup logic
            _view.InitUi();

            Assert.AreEqual(DisplayStyle.Flex, _rootElement.style.display.value);
        }

        [Test]
        public void View_ShowDashboard_UpdatesStatusTextAndSetsDisplayFlex() {
            _view.ShowDashboard("Connected: User123");

            Assert.AreEqual("Connected: User123", _lblAccountStatus.text);
            Assert.AreEqual(DisplayStyle.Flex, _rootElement.style.display.value);
        }

        [Test]
        public void View_HideDashboard_SetsDisplayNone() {
            _rootElement.style.display = DisplayStyle.Flex;

            _view.HideDashboard();

            Assert.AreEqual(DisplayStyle.None, _rootElement.style.display.value);
        }

        [Test]
        public void View_InterfaceInteractions_DispatchArchitecturalEvents() {
            bool closeCalled = false;
            bool linkCalled = false;
            bool exitCalled = false;

            _view.OnCloseRequested += () => closeCalled = true;
            _view.OnLinkAccountRequested += () => linkCalled = true;
            _view.OnExitApplicationRequested += () => exitCalled = true;

            SimulateButtonClick(_btnCloseDash);
            SimulateButtonClick(_btnOpenLinking);
            SimulateButtonClick(_btnExitApp);

            Assert.IsTrue(closeCalled, "OnCloseRequested event failed to dispatch.");
            Assert.IsTrue(linkCalled, "OnLinkAccountRequested event failed to dispatch.");
            Assert.IsTrue(exitCalled, "OnExitApplicationRequested event failed to dispatch.");
        }

        [Test]
        public void View_GameSwitchInteractions_DispatchCorrectContextParameters() {
            string selectedGame = string.Empty;
            _view.OnGameSwitchRequested += (gameName) => selectedGame = gameName;

            SimulateButtonClick(_btnGameBlackjack);
            Assert.AreEqual("Blackjack", selectedGame);

            SimulateButtonClick(_btnGameSolitaire);
            Assert.AreEqual("Solitaire", selectedGame);

            SimulateButtonClick(_btnGameTexasHoldem);
            Assert.AreEqual("TexasHoldem", selectedGame);
        }

        [Test]
        public void View_NullFields_HandleOperationsGracefully() {
            SetPrivateField(_view, "_lblAccountStatus", null);
            SetPrivateField(_view, "_root", null);

            Assert.DoesNotThrow(() => {
                _view.ShowDashboard("Status Test");
                _view.HideDashboard();
            }, "Dashboard operations should protect themselves from missing node hierarchy elements gracefully.");
        }
    }
}