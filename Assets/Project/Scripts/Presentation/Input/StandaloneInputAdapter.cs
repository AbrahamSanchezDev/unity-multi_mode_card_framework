using System;
using UnityEngine;
using UnityEngine.InputSystem;
using CardFramework.Presentation.Interfaces;

namespace CardFramework.Presentation.Input {
    public class StandaloneInputAdapter : IInputContext, IDisposable {
        public event Action OnPrimarySelectPerformed;

        private readonly InputAction _clickAction;
        private readonly InputAction _positionAction;

        public StandaloneInputAdapter() {
            // Binding to standard pointer actions using the Modern Input System
            _clickAction = new InputAction("Click", binding: "<Pointer>/press");
            _positionAction = new InputAction("Position", binding: "<Pointer>/position");

            _clickAction.performed += OnClickPerformed;

            // Activate input tracking hooks
            _clickAction.Enable();
            _positionAction.Enable();
        }

        private void OnClickPerformed(InputAction.CallbackContext context) {
            OnPrimarySelectPerformed?.Invoke();
        }

        public Vector2 GetPointerPosition() {
            return _positionAction.ReadValue<Vector2>();
        }

        public bool IsSelectHeld() {
            return _clickAction.IsPressed();
        }

        public void Dispose() {
            _clickAction.performed -= OnClickPerformed;
            _clickAction.Disable();
            _positionAction.Disable();
        }
    }
}