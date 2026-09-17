using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TurnObjToGrabObjInVR : MonoBehaviour {
    private XRGrabInteractable _grabInteractable;

    public bool CallRePositionOfUiOnRelease;

    protected IEnumerator Start() {
        yield return null;
#if VR
        _grabInteractable = VRUtilities.TurnObjToVRGrabable(gameObject);
        if (_grabInteractable != null && CallRePositionOfUiOnRelease) {
            _grabInteractable.lastSelectExited.AddListener(_ => TurnUiDocumentsToVrObj.RePositionUi?.Invoke());
        }
#endif
    }
}