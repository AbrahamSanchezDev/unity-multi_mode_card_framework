using System;
using CardFramework.Presentation.Views;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VRCardController : MonoBehaviour {

    private CardFaceGenerator cardFaceGenerator;

    public Action actionOnCardSelected;
    public Action actionOnCardDeselected;


    private void Awake() {
        cardFaceGenerator = GetComponent<CardFaceGenerator>();
        VRUtilities.TurnObjToVRGrabableLimitY(gameObject);
        var interactor = GetComponent<XRGrabInteractable>();
        if (interactor) {
            interactor.firstSelectEntered.AddListener(_ => OnSelected());
            interactor.lastSelectExited.AddListener(_ => OnDeSelected());
        }
        else {
            Debug.LogWarning($"No XRGrabInteractable found on {gameObject.name}. VR interactions may not work as expected.");
        }

    }

    public void OnSelected() {
        Debug.Log($"Card {gameObject.name} selected in VR.");
        actionOnCardSelected?.Invoke();

    }
    private void OnDeSelected() {
        Debug.Log($"Card {gameObject.name} deselected in VR.");
        actionOnCardDeselected?.Invoke();
    }

}