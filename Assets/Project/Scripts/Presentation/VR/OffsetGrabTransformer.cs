using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class OffsetGrabTransformer : XRGeneralGrabTransformer {
    [SerializeField] private float yOffset = 0.00120005f;
    
    public float YOffset {
        get => yOffset;
        set => yOffset = value;
    }

    private Vector3 offsetToApply = Vector3.zero;

    public override void OnGrab(XRGrabInteractable grabInteractable) {
        base.OnGrab(grabInteractable);
        // Set the offset once when grabbed
        offsetToApply = Vector3.up * yOffset;
    }

    public override void OnUnlink(XRGrabInteractable grabInteractable) {
        base.OnUnlink(grabInteractable);
        // Reset when released
        offsetToApply = Vector3.zero;
    }

    public override void Process(
        XRGrabInteractable grabInteractable,
        XRInteractionUpdateOrder.UpdatePhase updatePhase,
        ref Pose targetPose,
        ref Vector3 localScale) {
        // Call base to do all normal grab transformer calculations
        base.Process(grabInteractable, updatePhase, ref targetPose, ref localScale);

        // Then apply our fixed Y offset on top
        targetPose.position += offsetToApply;
    }
}