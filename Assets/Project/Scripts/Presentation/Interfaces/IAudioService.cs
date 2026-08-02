using System;
using UnityEngine;
using CardFramework.Presentation.Views;

namespace CardFramework.Presentation.Interfaces {
    /// <summary>
    /// Defines the contract for a centralized audio service that can play short card-game sound effects.
    /// </summary>
    public interface IAudioService {
        /// <summary>
        /// Plays the configured sound used when a card is lifted or grabbed.
        /// </summary>
        void PlayCardGrab();

        /// <summary>
        /// Plays the configured sound used when a card is released or placed.
        /// </summary>
        void PlayCardDrop();

        /// <summary>
        /// Plays the configured sound used when a deck is shuffled or a deal starts.
        /// </summary>
        void PlayShuffle();

        /// <summary>
        /// Plays the configured sound used when a move is rejected or a drag snaps back.
        /// </summary>
        void PlayInvalidMove();

        /// <summary>
        /// Plays the configured sound used for UI button interactions.
        /// </summary>
        void PlayButtonClick();

        /// <summary>
        /// Sets the overall playback volume for the service.
        /// </summary>
        /// <param name="volume">The master volume value in the range <c>[0, 1]</c>.</param>
        void SetMasterVolume(float volume);

        /// <summary>
        /// Attaches the service to a spatial card so it can listen for drag-related events.
        /// </summary>
        /// <param name="interactable">The card interactable to observe.</param>
        void AttachToSpatialCard(SpatialCardInteractable interactable);

        /// <summary>
        /// Detaches the service from a spatial card so it stops listening for drag-related events.
        /// </summary>
        /// <param name="interactable">The card interactable to stop observing.</param>
        void DetachFromSpatialCard(SpatialCardInteractable interactable);
    }
}
