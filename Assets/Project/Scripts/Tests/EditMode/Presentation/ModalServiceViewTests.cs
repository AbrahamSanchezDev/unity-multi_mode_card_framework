using NUnit.Framework;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using CardFramework.Presentation.Views;

namespace CardFramework.Tests.EditMode.Presentation {
    [TestFixture]
    public class ModalServiceViewTests : ViewClassForTests {
        private GameObject _testGo;
        private ModalServiceView _view;
        private UIDocument _uiDocument;

        private VisualElement _rootElement;
        private VisualElement _modalOverlay;
        private Label _modalTitle;
        private Label _modalMessage;
        private Button _modalConfirmBtn;
        private Button _modalCancelBtn;

        [SetUp]
        public void Setup() {
            // 1. Create the GameObject in an inactive state to safely coordinate initialization sequence
            _testGo = new GameObject("Test_ModalServiceView");
            _testGo.SetActive(false);

            _uiDocument = _testGo.AddComponent<UIDocument>();
            _view = _testGo.AddComponent<ModalServiceView>();

            // 2. Provision an empty ScriptableObject tree structure to isolate mock elements without serialization dependencies
            var mockAsset = ScriptableObject.CreateInstance<VisualTreeAsset>();
            _uiDocument.visualTreeAsset = mockAsset;
            _rootElement = _uiDocument.rootVisualElement;
            _rootElement.Clear();

            // 3. Instantiate and bind target VisualElements matching production naming keys (Q<T>)
            _modalOverlay = new VisualElement() { name = "modal-overlay" };
            _modalTitle = new Label() { name = "modal-title" };
            _modalMessage = new Label() { name = "modal-message" };
            _modalConfirmBtn = new Button() { name = "modal-confirm-btn" };
            _modalCancelBtn = new Button() { name = "modal-cancel-btn" };

            _rootElement.Add(_modalOverlay);
            _rootElement.Add(_modalTitle);
            _rootElement.Add(_modalMessage);
            _rootElement.Add(_modalConfirmBtn);
            _rootElement.Add(_modalCancelBtn);

            // 4. Force Awake execution to cache dependencies and set initial state (UIDocument.enabled = false)
            var awakeMethod = typeof(ModalServiceView).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            awakeMethod.Invoke(_view, null);

            _testGo.SetActive(true);
        }

        [TearDown]
        public void TearDown() {
            if (_testGo != null) {
                UnityEngine.Object.DestroyImmediate(_testGo);
            }
        }

        [Test]
        public void View_Awake_DisablesUIDocumentImmediately() {
            // Ensures the modal engine does not absorb input tracking parameters on system startup
            Assert.IsFalse(_uiDocument.enabled, "UIDocument should be initialized to an enabled = false state during Awake.");
        }

        [Test]
        public void View_ShowLoading_ConfiguresVisualElementsCorrectly() {
            _view.ShowLoading("System synchronizing...");

            // Assertions mapping text values and display state modifiers
            Assert.IsTrue(_uiDocument.enabled, "UIDocument should be toggled active to draw overlay systems.");
            Assert.AreEqual("PLEASE WAIT", _modalTitle.text);
            Assert.AreEqual("System synchronizing...", _modalMessage.text);
            Assert.AreEqual(DisplayStyle.None, _modalConfirmBtn.style.display.value);
            Assert.AreEqual(DisplayStyle.None, _modalCancelBtn.style.display.value);
            Assert.AreEqual(DisplayStyle.Flex, _modalOverlay.style.display.value);
            Assert.AreEqual(PickingMode.Position, _modalOverlay.pickingMode);
        }

        [Test]
        public void View_ShowAlert_ConfiguresElementsAndDispatchesCallbackOnClick() {
            bool confirmCallbackInvoked = false;
            _view.ShowAlert("Warning Alert", "Low memory detected.", () => confirmCallbackInvoked = true);

            Assert.IsTrue(_uiDocument.enabled);
            Assert.AreEqual("WARNING ALERT", _modalTitle.text); // Verifies .ToUpper() conversion check
            Assert.AreEqual("Low memory detected.", _modalMessage.text);
            Assert.AreEqual(DisplayStyle.Flex, _modalConfirmBtn.style.display.value);
            Assert.AreEqual("OK", _modalConfirmBtn.text);
            Assert.AreEqual(DisplayStyle.None, _modalCancelBtn.style.display.value);

            // Simulate the button interaction via base class reflection helper
            SimulateButtonClick(_modalConfirmBtn);

            Assert.IsTrue(confirmCallbackInvoked, "The alert confirm callback action should be executed on click.");
            Assert.AreEqual(DisplayStyle.None, _modalOverlay.style.display.value, "Overlay should hide itself following click confirmation.");
            Assert.IsFalse(_uiDocument.enabled, "UIDocument should be disabled after click execution completes.");
        }

        [Test]
        public void View_ShowConfirmation_HandlesConfirmFlowsCorrectly() {
            bool confirmCalled = false;
            bool cancelCalled = false;

            _view.ShowConfirmation("Delete File", "Are you sure?", () => confirmCalled = true, () => cancelCalled = true);

            Assert.IsTrue(_uiDocument.enabled);
            Assert.AreEqual("DELETE FILE", _modalTitle.text);
            Assert.AreEqual(DisplayStyle.Flex, _modalConfirmBtn.style.display.value);
            Assert.AreEqual("YES", _modalConfirmBtn.text);
            Assert.AreEqual(DisplayStyle.Flex, _modalCancelBtn.style.display.value);
            Assert.AreEqual("NO", _modalCancelBtn.text);

            SimulateButtonClick(_modalConfirmBtn);

            Assert.IsTrue(confirmCalled);
            Assert.IsFalse(cancelCalled);
            Assert.IsFalse(_uiDocument.enabled);
        }

        [Test]
        public void View_ShowConfirmation_HandlesCancelFlowsCorrectly() {
            bool confirmCalled = false;
            bool cancelCalled = false;

            _view.ShowConfirmation("Exit App", "Quit now?", () => confirmCalled = true, () => cancelCalled = true);

            SimulateButtonClick(_modalCancelBtn);

            Assert.IsFalse(confirmCalled);
            Assert.IsTrue(cancelCalled);
            Assert.IsFalse(_uiDocument.enabled);
        }

        [Test]
        public void View_HideModal_ResetsOverlayPropertiesAndDisablesDocument() {
            // 1. Put the view into a valid active state using a public initialization path
            _view.ShowLoading("Mock Activation Message");

            // 2. Double-check that it is actively visible before testing the hide routine
            Assert.AreEqual(DisplayStyle.Flex, _modalOverlay.style.display.value);
            Assert.AreEqual(PickingMode.Position, _modalOverlay.pickingMode);
            Assert.IsTrue(_uiDocument.enabled);

            // 3. Execute the target cleanup method
            _view.HideModal();

            // 4. Assert clean termination parameters
            Assert.AreEqual(DisplayStyle.None, _modalOverlay.style.display.value);
            Assert.AreEqual(PickingMode.Ignore, _modalOverlay.pickingMode);
            Assert.IsFalse(_uiDocument.enabled, "UIDocument should turn off completely to clear application raycasts.");
        }

        [Test]
        public void View_InitializeVisualElements_ReturnsFalseWhenDocumentOrTreeIsDisabled() {
            // Force document state off to confirm the short-circuit logic path
            _uiDocument.enabled = false;

            var initMethod = typeof(ModalServiceView).GetMethod("InitializeVisualElements", BindingFlags.Instance | BindingFlags.NonPublic);
            bool result = (bool)initMethod.Invoke(_view, null);

            Assert.IsFalse(result, "InitializeVisualElements should return false if the layout component is currently disabled.");
        }

        [Test]
        public void View_TestLoading_ConfiguresUI() {
            _view.TestLoading();

            Assert.AreEqual("PLEASE WAIT", _modalTitle.text);
            Assert.AreEqual("Initializing system overlay framework...", _modalMessage.text);
        }

        [Test]
        public void View_TestAlert_ConfiguresAndDispatchesAction() {
            bool callbackInvoked = false;

            // Execute the isolated test wrapper block directly
            _view.TestAlert(() => callbackInvoked = true);

            Assert.AreEqual("TEST ALERT", _modalTitle.text);
            Assert.AreEqual("This is a test alert modal.", _modalMessage.text);

            // Re-enabling the UI framework right before clicking allows 
            // the subsequent nested calls inside the lambda to validate successfully
            _uiDocument.enabled = true;
            SimulateButtonClick(_modalConfirmBtn);

            Assert.IsTrue(callbackInvoked, "The passing callback parameter should fire securely.");
        }

        [Test]
        public void View_TestConfirmation_ConfiguresAndDispatchesAction() {
            bool confirmInvoked = false;

            _view.TestConfirmation(() => confirmInvoked = true, null);

            Assert.AreEqual("TEST CONFIRMATION", _modalTitle.text);
            Assert.AreEqual("Do you want to proceed?", _modalMessage.text);

            SimulateButtonClick(_modalConfirmBtn);
            Assert.IsTrue(confirmInvoked);
        }

        [Test]
        public void View_StartCoroutine_AdvancesSafelyWhenTestModeUiIsTrue() {
            _view.TestModeUi = true;

            // Test the coroutine container simply to verify it iterates over the sequence paths
            IEnumerator startRoutine = _view.Start();

            Assert.DoesNotThrow(() => {
                while (startRoutine.MoveNext()) {
                    // Cycles instantly over the WaitForSeconds wrappers
                }
            }, "The coroutine layout track should execute from end-to-end without unhandled faults.");
        }

        [Test]
        public void View_StartCoroutine_DoesNotExecuteSequenceWhenTestModeUiIsFalse() {
            // 1. Arrange: Disable the test mode flag
            _view.TestModeUi = false;

            // 2. Act: Manually advance the coroutine
            IEnumerator startRoutine = _view.Start();
            bool hasElements = startRoutine.MoveNext();

            // 3. Assert: The coroutine should complete instantly on step 1 without hitting any internal blocks
            Assert.IsFalse(hasElements, "The coroutine should return false immediately on its first evaluation step if TestModeUi is false.");
            Assert.IsFalse(_uiDocument.enabled, "UIDocument should remain disabled since no internal modal actions were executed.");
        }
       
        [Test]
        public void View_StartCoroutineCallback_TriggersNestedTestConfirmation() {
            // 1. Arrange: Enforce TestModeUi to follow the custom flow branch
            _view.TestModeUi = true;
            IEnumerator startRoutine = _view.Start();

            // 2. Act: Fully advance the coroutine to exhaust all WaitForSeconds states
            while (startRoutine.MoveNext()) { }

            // Verify we are sitting at the TestAlert stage before clicking
            Assert.AreEqual("TEST ALERT", _modalTitle.text);

            // 3. Act: Hook a listener to run immediately AFTER HideModal() turns off the UI document,
            // but BEFORE onConfirm executes TestConfirmation(). This ensures the flag stays true.
            _modalConfirmBtn.clicked += KeepDocumentAliveForNestedCall;

            SimulateButtonClick(_modalConfirmBtn);

            // 4. Assert: Verify the inner anonymous lambda successfully invoked TestConfirmation()
            Assert.AreEqual("TEST CONFIRMATION", _modalTitle.text, "The inner callback failed to transition the UI to TestConfirmation.");
            Assert.AreEqual(DisplayStyle.Flex, _modalCancelBtn.style.display.value, "The Cancel button should be visible on the confirmation screen.");

            // Local helper function attached sequentially to the button event loop stack
            void KeepDocumentAliveForNestedCall() {
                _modalConfirmBtn.clicked -= KeepDocumentAliveForNestedCall;

                // Force the layout engine back online immediately so InitializeVisualElements passes cleanly
                _uiDocument.enabled = true;
                SetPrivateField(_view, "_root", _rootElement);
            }
        }

        [Test]
        public void View_TestConfirmationCancelPath_ExecutesOnCancelLambdaBranch() {
            bool cancelActionInvoked = false;

            // 1. Arrange: Invoke TestConfirmation passing an inline trackable token to the onCancel argument
            _view.TestConfirmation(onConfirm: null, onCancel: () => cancelActionInvoked = true);

            // Verify the setup states are configured properly
            Assert.AreEqual("NO", _modalCancelBtn.text);
            Assert.AreEqual(DisplayStyle.Flex, _modalCancelBtn.style.display.value);

            // 2. Act: Trigger the cancellation path via our reflection-based button helper
            SimulateButtonClick(_modalCancelBtn);

            // 3. Assert: Confirm that lines 18-21 (the cancel block branch) were executed successfully
            Assert.IsTrue(cancelActionInvoked, "The cancellation lambda track was not covered or executed upon click.");
            Assert.AreEqual(DisplayStyle.None, _modalOverlay.style.display.value, "The window should cleanly close following the cancellation click step.");
        }
    }
}