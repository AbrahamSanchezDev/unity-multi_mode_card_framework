using UnityEngine;

namespace CardFramework.Presentation.Views {
    [RequireComponent(typeof(Collider))]
    public class FoundationDropTarget : MonoBehaviour {
        [SerializeField] private int foundationIndex;

        public int FoundationIndex => foundationIndex;
    }
}
