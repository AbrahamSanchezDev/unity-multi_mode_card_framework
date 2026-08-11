using UnityEngine;
using UnityEngine.InputSystem;

public class RepositionUiObjsOnSettingsPressed : MonoBehaviour {
    [Header("Input References")]
    [SerializeField] private InputActionReference repositionUiAction;

    private InputAction _action;

    protected void OnEnable() {
        if (repositionUiAction == null || repositionUiAction.action == null) return;

        // Store active reference locally to safely unsubscribe later
        _action = repositionUiAction.action;
        _action.performed += HandleRepositionUi;

        // Enabling via reference is okay if managed globally, 
        // but ensuring enabled state here prevents silent input drops
        if (!_action.enabled) {
            _action.Enable();
        }
    }

    protected void OnDisable() {
        if (_action != null) {
            _action.performed -= HandleRepositionUi;
            // Avoid calling _action.Disable() here if this Action is shared 
            // across other components (e.g., player movement, global menus)
        }
    }

    private void HandleRepositionUi(InputAction.CallbackContext context) {
        TurnUiDocumentsToVrObj.RePositionUi?.Invoke();
        Debug.Log("[RepositionUiObjsOnSettingsPressed] Reposition UI event invoked.");
    }
}