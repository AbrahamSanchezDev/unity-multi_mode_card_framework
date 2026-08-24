using System;
using CardFramework.Presentation.Views;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VRCardController : MonoBehaviour {
    public Action actionOnCardSelected;
    public Action actionOnCardDeselected;

    public Func<float> getHighersOffset;


    // Min-Y enforcement handled by VRGrabYEnforcer to avoid per-card LateUpdate overhead.

    private XRGrabInteractable _grabInteractable;
    private OffsetGrabTransformer _offsetGrabTransformer;

    [SerializeField] private float _yOffset = 0.05f;


    private void Awake() {
        VRUtilities.TurnObjToVRGrabableLimitY(gameObject);
        _grabInteractable = GetComponent<XRGrabInteractable>();
        _offsetGrabTransformer = GetComponent<OffsetGrabTransformer>();
        if (_grabInteractable) {
            _grabInteractable.firstSelectEntered.AddListener(_ => OnSelected());
            _grabInteractable.lastSelectExited.AddListener(_ => OnDeSelected());
            _grabInteractable.hoverEntered.AddListener(OnGrab);
        }
        else {
            Debug.LogWarning($"No XRGrabInteractable found on {gameObject.name}. VR interactions may not work as expected.");
        }
    }

    private void OnGrab(HoverEnterEventArgs args) {
        if (getHighersOffset != null) {
            _yOffset = getHighersOffset.Invoke() - transform.position.y;
        }
        if (_offsetGrabTransformer) {
            _offsetGrabTransformer.YOffset = _yOffset;
        }
        else {
            Debug.LogWarning($"No OffsetGrabTransformer found on {gameObject.name}. Y offset may not be applied correctly.");
        }
    }

    public void OnSelected() {
        actionOnCardSelected?.Invoke();
    }
    private void OnDeSelected() {
        actionOnCardDeselected?.Invoke();
    }
}