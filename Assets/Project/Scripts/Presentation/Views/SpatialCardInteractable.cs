// File: Assets/_Project/Scripts/Presentation/Views/SpatialCardInteractable.cs
using UnityEngine;
using CardFramework.Core.Models;

namespace CardFramework.Presentation.Views {
    [RequireComponent(typeof(Collider))]
    public class SpatialCardInteractable : MonoBehaviour {
        public CardData CardData { get; private set; }
        public int SourceColumnIndex { get; private set; } = -1;
        public int CardIndexInColumn { get; private set; } = -1;
        public bool IsFromWastePile { get; private set; }

        private Collider _cardCollider;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;

        public void Initialize(CardData cardData, int sourceColumnIndex = -1, int cardIndexInColumn = -1, bool isFromWastePile = false) {
            CardData = cardData;
            SourceColumnIndex = sourceColumnIndex;
            CardIndexInColumn = cardIndexInColumn;
            IsFromWastePile = isFromWastePile;
            _cardCollider = GetComponent<Collider>();
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;
        }

        public void SetPosition(Vector3 position) {
            transform.position = position;
        }

        public void SetColliderEnabled(bool enabled) {
            if (_cardCollider != null) {
                _cardCollider.enabled = enabled;
            }
        }

        public void ResetToOriginalPosition() {
            transform.position = _originalPosition;
            transform.rotation = _originalRotation;
        }
    }
}