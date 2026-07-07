using UnityEngine;
using CardFramework.Presentation.Interfaces;
using VContainer;
using CardFramework.Presentation.Views;

namespace CardFramework.Presentation.Interaction {
    public class TableInteractionManager : MonoBehaviour {
        private IInputContext _inputContext;
        private Camera _mainCamera;

        [Inject]
        public void Construct(IInputContext inputContext) {
            _inputContext = inputContext;
        }

        private void Start() {
            _mainCamera = Camera.main;

            // Listen to the multi-platform primary action (Click / VR Trigger)
            _inputContext.OnPrimarySelectPerformed += HandleWorldSelection;
        }

        private void HandleWorldSelection() {
            // Get pointer position regardless of platform (Mouse pos or screen center)
            Vector2 pointerPos = _inputContext.GetPointerPosition();

            // Cast a physics ray from the pointer position into the 3D space
            Ray ray = _mainCamera.ScreenPointToRay(pointerPos);

            if (Physics.Raycast(ray, out RaycastHit hit)) {
                // Check if we hit a 3D physical card component!
                var physicalCard = hit.collider.GetComponent<CardFaceGenerator>();
                if (physicalCard != null) {
                    // Logic to highlight, lift, or flip the card in 3D space
                    Debug.Log("Player clicked on a physical 3D card!");
                }
            }
        }

        private void OnDestroy() {
            if (_inputContext != null) {
                _inputContext.OnPrimarySelectPerformed -= HandleWorldSelection;
            }
        }
    }
}