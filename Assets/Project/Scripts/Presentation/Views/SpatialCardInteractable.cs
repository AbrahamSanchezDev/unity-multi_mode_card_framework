// File: Assets/_Project/Scripts/Presentation/Views/SpatialCardInteractable.cs
using System;
using UnityEngine;
using CardFramework.Core.Models;

namespace CardFramework.Presentation.Views {
    [RequireComponent(typeof(Collider))]
    public class SpatialCardInteractable : MonoBehaviour {
        /// <summary>
        /// Raised when this spatial card begins a drag interaction.
        /// </summary>
        public event Action<SpatialCardInteractable> OnCardGrabbed;

        /// <summary>
        /// Raised when this spatial card is released after a drag or a valid drop completes.
        /// </summary>
        public event Action<SpatialCardInteractable> OnCardDropped;

        public CardData CardData { get; private set; }
        public int SourceColumnIndex { get; private set; } = -1;
        public int CardIndexInColumn { get; private set; } = -1;
        public bool IsFromWastePile { get; private set; }

        private Collider _cardCollider;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private CardData _lastRenderedCardData;
        private bool _hasRenderedState;
        private bool _lastRenderedFaceUp;
        private bool _pendingRevealAnimation;

        public void Initialize(CardData cardData, int sourceColumnIndex = -1, int cardIndexInColumn = -1, bool isFromWastePile = false) {
            CardData = cardData;
            SourceColumnIndex = sourceColumnIndex;
            CardIndexInColumn = cardIndexInColumn;
            IsFromWastePile = isFromWastePile;
            _cardCollider = GetComponent<Collider>();
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;
            SyncRenderedState(cardData, cardData.IsFaceUp);
        }

        public void SetPosition(Vector3 position) {
            transform.position = position;
        }

        /// <summary>
        /// Invokes the card-grab event so any interested audio or view service can react to the interaction.
        /// </summary>
        public void NotifyGrabbed() {
            OnCardGrabbed?.Invoke(this);
        }

        /// <summary>
        /// Invokes the card-drop event so any interested audio or view service can react to a completed release.
        /// </summary>
        public void NotifyDropped() {
            OnCardDropped?.Invoke(this);
        }

        public void SetColliderEnabled(bool enabled) {
            if (_cardCollider != null) {
                _cardCollider.enabled = enabled;
            }
        }

        public bool ShouldAnimateReveal(CardData cardData, bool isFaceUp, bool requestedReveal) {
            if (!requestedReveal || !isFaceUp) {
                return false;
            }

            if (!_hasRenderedState) {
                return false;
            }

            bool sameCard = _lastRenderedCardData.HasSameIdentity(cardData);
            bool shouldReveal = sameCard && !_lastRenderedFaceUp;
            _pendingRevealAnimation = shouldReveal;
            return shouldReveal;
        }

        public void SyncRenderedState(CardData cardData, bool isFaceUp) {
            _lastRenderedCardData = cardData;
            _lastRenderedFaceUp = isFaceUp;
            _hasRenderedState = true;
            if (!isFaceUp) {
                _pendingRevealAnimation = false;
            }
        }

        public void ResetToOriginalPosition() {
            transform.position = _originalPosition;
            transform.rotation = _originalRotation;
        }
    }
}