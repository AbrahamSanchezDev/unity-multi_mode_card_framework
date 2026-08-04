using UnityEngine;

namespace CardFramework.Presentation.Views {
    [RequireComponent(typeof(Collider))]
    public class TableauDropTarget : MonoBehaviour {
        [SerializeField] private int columnIndex;
        [SerializeField] private GameObject[] visuals;

        public int ColumnIndex => columnIndex;

        private Collider _collider;

        public void SetColumnIndex(int index) {
            columnIndex = index;
        }

        public void SetEnabled(bool enabled) {
            if (_collider == null) {
                _collider = GetComponent<Collider>();
            }
            if (_collider != null) {
                _collider.enabled = enabled;
            }

            if (visuals != null) {
                foreach (var visual in visuals) {
                    if (visual != null) {
                        visual.SetActive(enabled);
                    }
                }
            }
        }
    }
}
