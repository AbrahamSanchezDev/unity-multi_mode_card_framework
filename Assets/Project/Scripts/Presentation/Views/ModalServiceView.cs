using System;
using UnityEngine;
using UnityEngine.UIElements;
using CardFramework.Presentation.Interfaces;

namespace CardFramework.Presentation.Views {
    [RequireComponent(typeof(UIDocument))]
    public class ModalServiceView : MonoBehaviour, IModalService {
        private VisualElement _root;
        private VisualElement _modalOverlay;
        private Label _modalTitle;
        private Label _modalMessage;
        private Button _modalConfirmBtn;
        private Button _modalCancelBtn;

        private void OnEnable() {
            var uiDocument = GetComponent<UIDocument>();
            _root = uiDocument.rootVisualElement;

            // Query modal elements
            _modalOverlay = _root.Q<VisualElement>("modal-overlay");
            _modalTitle = _root.Q<Label>("modal-title");
            _modalMessage = _root.Q<Label>("modal-message");
            _modalConfirmBtn = _root.Q<Button>("modal-confirm-btn");
            _modalCancelBtn = _root.Q<Button>("modal-cancel-btn");

            // Make sure the modal is hidden initially
            HideModal();
        }

        public void ShowLoading(string message) {
            _modalTitle.text = "PLEASE WAIT";
            _modalMessage.text = message;
            _modalConfirmBtn.style.display = DisplayStyle.None;
            _modalCancelBtn.style.display = DisplayStyle.None;
            _modalOverlay.style.display = DisplayStyle.Flex;
        }

        public void ShowAlert(string title, string message, Action onConfirm = null) {
            _modalTitle.text = title.ToUpper();
            _modalMessage.text = message;

            _modalConfirmBtn.style.display = DisplayStyle.Flex;
            _modalConfirmBtn.text = "OK";
            _modalCancelBtn.style.display = DisplayStyle.None;

            _modalConfirmBtn.clicked += SystemAction;
            _modalOverlay.style.display = DisplayStyle.Flex;

            void SystemAction() {
                _modalConfirmBtn.clicked -= SystemAction;
                HideModal();
                onConfirm?.Invoke();
            }
        }

        public void ShowConfirmation(string title, string message, Action onConfirm, Action onCancel) {
            _modalTitle.text = title.ToUpper();
            _modalMessage.text = message;

            _modalConfirmBtn.style.display = DisplayStyle.Flex;
            _modalConfirmBtn.text = "YES";
            _modalCancelBtn.style.display = DisplayStyle.Flex;
            _modalCancelBtn.text = "NO";

            _modalConfirmBtn.clicked += ConfirmAction;
            _modalCancelBtn.clicked += CancelAction;
            _modalOverlay.style.display = DisplayStyle.Flex;

            void ConfirmAction() { Unbind(); onConfirm?.Invoke(); }
            void CancelAction() { Unbind(); onCancel?.Invoke(); }

            void Unbind() {
                _modalConfirmBtn.clicked -= ConfirmAction;
                _modalCancelBtn.clicked -= CancelAction;
                HideModal();
            }
        }

        public void HideModal() {
            if (_modalOverlay != null) _modalOverlay.style.display = DisplayStyle.None;
        }
    }
}