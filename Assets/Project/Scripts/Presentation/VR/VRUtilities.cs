using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public static class VRUtilities {
    public static void TurnObjToVRReady(GameObject obj) {
        var interactor = obj.GetComponent<XRSimpleInteractable>();
        // Add XRSimpleInteractable if not already present
        if (interactor == null) {
            interactor = obj.AddComponent<XRSimpleInteractable>();
        }
        var poker = obj.GetComponent<XRPokeFilter>();
        if (poker == null) {
            poker = obj.AddComponent<XRPokeFilter>();
        }
        // poker.pokeInteractable = interactor;
        // var col = obj.GetComponent<Collider>();
        // if (col == null) {
        //     // poker.pokeCollider = col;
        // }

    }

    public static XRGrabInteractable TurnObjToVRGrabable(GameObject obj) {

        var rb = obj.GetComponent<Rigidbody>();
        if (rb == null) {
            rb = obj.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;

        var grabableObj = obj.GetComponent<XRGrabInteractable>();
        // Add XRSimpleInteractable if not already present
        if (grabableObj == null) {
            grabableObj = obj.AddComponent<XRGrabInteractable>();
        }
        grabableObj.trackRotation = false;
        grabableObj.throwOnDetach = false;

        var grabTransform = obj.GetComponent<OffsetGrabTransformer>();
        if (grabTransform == null) {
            grabTransform = obj.AddComponent<OffsetGrabTransformer>();
        }
        // grabTransform.permittedDisplacementAxes = XRGeneralGrabTransformer.ManipulationAxes.X | XRGeneralGrabTransformer.ManipulationAxes.Z;
        grabTransform.allowOneHandedScaling = false;
        grabTransform.allowTwoHandedScaling = false;
        grabTransform.clampScaling = false;
        return grabableObj;
    }

    public static XRGrabInteractable TurnObjToVRGrabableLimitY(GameObject obj) {

        var rb = obj.GetComponent<Rigidbody>();
        if (rb == null) {
            rb = obj.AddComponent<Rigidbody>();
        }
        // rb.constraints = RigidbodyConstraints.FreezePositionY |
        //             RigidbodyConstraints.FreezeRotationX |
        //             RigidbodyConstraints.FreezeRotationY |
        //             RigidbodyConstraints.FreezeRotationZ;

        rb.isKinematic = true;

        var grabableObj = obj.GetComponent<XRGrabInteractable>();
        // Add XRSimpleInteractable if not already present
        if (grabableObj == null) {
            grabableObj = obj.AddComponent<XRGrabInteractable>();
        }
        grabableObj.trackRotation = false;
        grabableObj.throwOnDetach = false;

        var grabTransform = obj.GetComponent<OffsetGrabTransformer>();
        if (grabTransform == null) {
            grabTransform = obj.AddComponent<OffsetGrabTransformer>();
        }
        grabTransform.permittedDisplacementAxes = XRGeneralGrabTransformer.ManipulationAxes.X | XRGeneralGrabTransformer.ManipulationAxes.Z;
        grabTransform.allowOneHandedScaling = false;
        grabTransform.allowTwoHandedScaling = false;
        grabTransform.clampScaling = false;

        return grabableObj;
    }

}