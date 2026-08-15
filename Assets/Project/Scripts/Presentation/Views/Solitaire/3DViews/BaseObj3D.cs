using UnityEngine;
using TMPro;

namespace CardFramework.Presentation.Views {
    public class BaseObj3D : MonoBehaviour {
        [SerializeField] protected TMP_Text displayText;

        public virtual void SetDisplayText(string text) {
            if (displayText != null) {
                displayText.text = text;
            }
        }

        public virtual void DoSetup() {
            // Base setup logic for 3D objects
        }
    }
}
