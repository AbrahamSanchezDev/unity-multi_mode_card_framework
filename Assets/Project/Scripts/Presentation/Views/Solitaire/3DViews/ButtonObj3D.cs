using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using DG.Tweening;



namespace CardFramework.Presentation.Views {
    public class ButtonObj3D : BaseObj3D {
        private System.Action onClickAction;

        private XRSimpleInteractable interactable;

        private Transform _buttonVisualTransform;

        private void Awake() {
            // Ensure the button is set up when the object is initialized
            DoSetup();
            DoBasicSetup();
        }

        private void DoBasicSetup() {

#if VR
            // Basic setup logic for the button, if any
            interactable = GetComponent<XRSimpleInteractable>();
            if (interactable == null) {
                interactable = gameObject.AddComponent<XRSimpleInteractable>();
            }
            interactable.activated.AddListener(_ => OnSelected());
            interactable.selectEntered.AddListener(_ => OnSelected());
            interactable.hoverEntered.AddListener(_ => OnHoverEntered());
            interactable.hoverExited.AddListener(_ => OnHoverExited());

            _buttonVisualTransform = transform.GetChild(0); // Assuming the first child is the visual representation of the button
#endif
        }

        private void OnHoverEntered() {
            AnimateScale(1.1f, 0.2f); // Scale up slightly
        }
        private void OnHoverExited() {
            AnimateScale(1.0f, 0.2f); // Scale back to original
        }

        private void AnimateScale(float targetScale, float duration) {
            _buttonVisualTransform.DOKill();
            transform.DOScale(targetScale, duration)
                             .SetEase(Ease.InOutSine);
        }

        public void PlayPulseImpact(float punchStrength = 0.2f, float duration = 0.25f) {
            // Prevent stacking fast clicks
            transform.DOKill();
            transform.localScale = Vector3.one;

            // Punches scale by punchStrength vector and vibrates back to original scale
            transform.DOPunchScale(Vector3.one * punchStrength, duration, vibrato: 1, elasticity: 0.5f);
        }

        public void SetupButton(string text, System.Action onClickAction) {
            DoSetup();
            SetDisplayText(text);
            this.onClickAction = onClickAction;
        }

        public void SetupButton(System.Action onClickAction) {
            DoSetup();
            this.onClickAction = onClickAction;
        }

        private void OnSelected() {
            onClickAction?.Invoke();
            PlayPulseImpact();
        }

        override public void DoSetup() {
            base.DoSetup();
            // Additional setup logic for ButtonObj3D

        }
    }
}
