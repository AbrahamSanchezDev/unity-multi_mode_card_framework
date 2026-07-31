using UnityEngine;

namespace CardFramework.Presentation.Views {
    public class TableauColumnDropTarget : MonoBehaviour {
        [SerializeField] private int columnIndex;
        public int ColumnIndex => columnIndex;
        [SerializeField]
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
        }
    }
}
