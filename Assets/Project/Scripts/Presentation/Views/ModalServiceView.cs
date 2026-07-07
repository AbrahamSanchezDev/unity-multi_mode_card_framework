using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using CardFramework.Presentation.Interfaces;
using UnityEngine.Events;

namespace CardFramework.Presentation.Views {
    [RequireComponent(typeof(UIDocument))]
    public class ModalServiceView : MonoBehaviour, IModalService {
        private UIDocument _uiDocument;
        private VisualElement _root;
        private VisualElement _modalOverlay;
        private Label _modalTitle;
        private Label _modalMessage;
        private Button _modalConfirmBtn;
        private Button _modalCancelBtn;

        public bool TestModeUi;

        private void Awake() {
            // Cache the native UIDocument component reference immediately
            _uiDocument = GetComponent<UIDocument>();

            // Ensure the panel starts completely disabled so it releases input focus on startup
            _uiDocument.enabled = false;
        }

        public IEnumerator Start() {
            if (TestModeUi) {

                // Optional: Demonstrate the modal service functionality after a brief delay
                yield return new WaitForSeconds(1f);
                TestLoading();
                yield return new WaitForSeconds(2f);
                TestAlert(() => {
                    TestConfirmation();
                });
            }
        }
        
        public void TestLoading() {
            ShowLoading("Initializing system overlay framework...");
        }

        public void TestAlert(UnityAction onConfirm = null) {
            ShowAlert("Test Alert", "This is a test alert modal.", () => {
                Debug.Log("Alert confirmed by user.");
                onConfirm?.Invoke();
            });
        }

        public void TestConfirmation(UnityAction onConfirm = null, UnityAction onCancel = null) {
            ShowConfirmation("Test Confirmation", "Do you want to proceed?",
                () => {
                    Debug.Log("User confirmed action.");
                    onConfirm?.Invoke();
                },
                () => {
                    Debug.Log("User canceled action.");
                    onCancel?.Invoke();
                });
        }

        /// <summary>
        /// Safe initialization to query elements only after the UIDocument updates its tree.
        /// </summary>
        private bool InitializeVisualElements() {
            if (_uiDocument == null || !_uiDocument.enabled) return false;

            _root = _uiDocument.rootVisualElement;
            if (_root == null) return false;

            // Query fresh elements within the newly generated visual tree structure
            _modalOverlay = _root.Q<VisualElement>("modal-overlay");
            _modalTitle = _root.Q<Label>("modal-title");
            _modalMessage = _root.Q<Label>("modal-message");
            _modalConfirmBtn = _root.Q<Button>("modal-confirm-btn");
            _modalCancelBtn = _root.Q<Button>("modal-cancel-btn");

            return _modalOverlay != null;
        }

        public void ShowLoading(string message) {
            // Enable the document to force the creation of the root visual tree
            _uiDocument.enabled = true;

            // Query the active nodes safely
            if (!InitializeVisualElements()) return;

            // Apply configurations to the current alive elements
            _modalTitle.text = "PLEASE WAIT";
            _modalMessage.text = message;
            _modalConfirmBtn.style.display = DisplayStyle.None;
            _modalCancelBtn.style.display = DisplayStyle.None;

            _modalOverlay.style.display = DisplayStyle.Flex;
            _modalOverlay.pickingMode = PickingMode.Position;
        }

        public void ShowAlert(string title, string message, Action onConfirm = null) {
            _uiDocument.enabled = true;
            if (!InitializeVisualElements()) return;

            _modalTitle.text = title.ToUpper();
            _modalMessage.text = message;

            _modalConfirmBtn.style.display = DisplayStyle.Flex;
            _modalConfirmBtn.text = "OK";
            _modalCancelBtn.style.display = DisplayStyle.None;

            _modalConfirmBtn.clicked += SystemAction;

            _modalOverlay.style.display = DisplayStyle.Flex;
            _modalOverlay.pickingMode = PickingMode.Position;

            void SystemAction() {
                _modalConfirmBtn.clicked -= SystemAction;
                InvokeCallback(onConfirm);
                HideModal();
            }
        }

        public void ShowConfirmation(string title, string message, Action onConfirm, Action onCancel) {
            _uiDocument.enabled = true;
            if (!InitializeVisualElements()) return;

            _modalTitle.text = title.ToUpper();
            _modalMessage.text = message;

            _modalConfirmBtn.style.display = DisplayStyle.Flex;
            _modalConfirmBtn.text = "YES";
            _modalCancelBtn.style.display = DisplayStyle.Flex;
            _modalCancelBtn.text = "NO";

            _modalConfirmBtn.clicked += ConfirmAction;
            _modalCancelBtn.clicked += CancelAction;

            _modalOverlay.style.display = DisplayStyle.Flex;
            _modalOverlay.pickingMode = PickingMode.Position;

            void ConfirmAction() { InvokeCallback(onConfirm); Unbind(); }
            void CancelAction() { InvokeCallback(onCancel); Unbind(); }

            void Unbind() {
                _modalConfirmBtn.clicked -= ConfirmAction;
                _modalCancelBtn.clicked -= CancelAction;
                HideModal();
            }
        }

        private void InvokeCallback(Action callback) {
            callback?.Invoke();
        }

        public void HideModal() {
            if (_modalOverlay != null) {
                _modalOverlay.style.display = DisplayStyle.None;
                _modalOverlay.pickingMode = PickingMode.Ignore;
            }

            // Safely turn off the UIDocument component to unblock input raycasts completely
            if (_uiDocument != null) {
                _uiDocument.enabled = false;
            }
        }
    }
}