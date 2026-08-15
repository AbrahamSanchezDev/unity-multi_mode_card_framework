namespace CardFramework.Presentation.Views {
    public class ButtonObj3D : BaseObj3D {
        private System.Action onClickAction;

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
        }

        override public void DoSetup() {
            base.DoSetup();
            // Additional setup logic for ButtonObj3D
        }
    }
}
