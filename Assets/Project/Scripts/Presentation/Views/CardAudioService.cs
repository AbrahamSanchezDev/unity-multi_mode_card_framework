using System.Collections.Generic;
using UnityEngine;
using CardFramework.Presentation.Interfaces;

namespace CardFramework.Presentation.Views {
    /// <summary>
    /// Centralized card-game audio playback service that routes short sound effects through a single <see cref="AudioSource"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public class CardAudioService : MonoBehaviour, IAudioService {
        [Header("Audio Source")]
        [SerializeField]
        [Tooltip("AudioSource used to fire short one-shot sounds.")]
        private AudioSource audioSource;

        [SerializeField]
        [Tooltip("Optional AudioSource for ambient background music.")]
        private AudioSource ambientAudioSource;

        [Header("Master Mix")]
        [SerializeField]
        [Tooltip("Overall volume multiplier applied to every sound effect.")]
        [Range(0f, 1f)]
        private float masterVolume = 1f;

        [SerializeField]
        [Tooltip("Allows a slight pitch variation per playback to avoid repetitive fatigue.")]
        [Range(0f, 0.2f)]
        private float pitchVariance = 0.05f;

        [Header("Card Audio Clips")]
        [SerializeField]
        [Tooltip("Played when a card is lifted or grabbed.")]
        private AudioClip cardGrabClip;

        [SerializeField]
        [Tooltip("Played when a card is released or successfully placed.")]
        private AudioClip cardDropClip;

        [SerializeField]
        [Tooltip("Played when a deck is shuffled or a deal begins.")]
        private AudioClip cardShuffleClip;

        [SerializeField]
        [Tooltip("Played for invalid moves, rejected drops, or snap-back feedback.")]
        private AudioClip invalidMoveClip;

        [SerializeField]
        [Tooltip("Played for UI button interactions.")]
        private AudioClip buttonClickClip;

        [SerializeField]
        [Tooltip("Win sound played when a game is completed successfully.")]
        private AudioClip gameWinClip;

        [SerializeField]
        [Tooltip("Played when a game starts.")]
        private AudioClip gameStartClip;


        [Header("Ambient Music")]
        [SerializeField]
        [Tooltip("Optional background music to play during gameplay.")]
        private AudioClip ambientMusicClip;

        [SerializeField]
        [Tooltip("Volume for the ambient music.")]
        [Range(0f, 1f)]
        private float ambientMusicVolume = 0.5f;

        // -------------------------------------------- Audio Settings --------------------------------------------

        [Header("Per-Clip Volume Controls")]
        [SerializeField]
        [Tooltip("Playback volume for the card grab sound.")]
        [Range(0f, 1f)]
        private float cardGrabVolume = 0.75f;

        [SerializeField]
        [Tooltip("Playback volume for the card drop sound.")]
        [Range(0f, 1f)]
        private float cardDropVolume = 0.7f;

        [SerializeField]
        [Tooltip("Playback volume for the shuffle/deal sound.")]
        [Range(0f, 1f)]
        private float cardShuffleVolume = 0.65f;

        [SerializeField]
        [Tooltip("Playback volume for the invalid move snap-back sound.")]
        [Range(0f, 1f)]
        private float invalidMoveVolume = 0.6f;

        [SerializeField]
        [Tooltip("Playback volume for the button click sound.")]
        [Range(0f, 1f)]
        private float buttonClickVolume = 0.55f;

        [SerializeField]
        [Tooltip("Playback volume for the win sound.")]
        [Range(0f, 1f)]
        private float gameWinVolume = 0.8f;

        [SerializeField]
        [Tooltip("Playback volume for the game start sound.")]
        [Range(0f, 1f)]
        private float gameStartVolume = 0.75f;

        private readonly HashSet<SpatialCardInteractable> _trackedSpatialCards = new HashSet<SpatialCardInteractable>();

        /// <summary>
        /// Initializes the internal <see cref="AudioSource"/> and ensures it is configured for one-shot playback.
        /// </summary>
        private void Awake() {
            if (audioSource == null) {
                audioSource = GetComponent<AudioSource>();
            }

            if (audioSource == null) {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            audioSource.dopplerLevel = 0f;

            if (ambientAudioSource != null && ambientMusicClip != null) {
                ambientAudioSource.clip = ambientMusicClip;
                ambientAudioSource.loop = true;
                ambientAudioSource.volume = Mathf.Clamp01(ambientMusicVolume * masterVolume);
                ambientAudioSource.Play();
            }
        }

        /// <summary>
        /// Cleans up event subscriptions when the service is destroyed.
        /// </summary>
        private void OnDestroy() {
            foreach (var interactable in _trackedSpatialCards) {
                if (interactable == null) {
                    continue;
                }

                interactable.OnCardGrabbed -= HandleCardGrabbed;
                interactable.OnCardDropped -= HandleCardDropped;
            }

            _trackedSpatialCards.Clear();
        }

        /// <inheritdoc />
        public void PlayCardGrab() {
            PlayOneShot(cardGrabClip, cardGrabVolume);
        }

        /// <inheritdoc />
        public void PlayCardDrop() {
            PlayOneShot(cardDropClip, cardDropVolume);
        }

        /// <inheritdoc />
        public void PlayShuffle() {
            PlayOneShot(cardShuffleClip, cardShuffleVolume);
        }

        /// <inheritdoc />
        public void PlayInvalidMove() {
            PlayOneShot(invalidMoveClip, invalidMoveVolume);
        }

        /// <inheritdoc />
        public void PlayButtonClick() {
            PlayOneShot(buttonClickClip, buttonClickVolume);
        }

        /// <inheritdoc />
        public void PlayVictory() {
            PlayOneShot(gameWinClip, gameWinVolume);
        }

        /// <inheritdoc />
        public void PlayGameStart() {
            PlayOneShot(gameStartClip, gameStartVolume);
        }

        /// <inheritdoc />
        public void SetMasterVolume(float volume) {
            masterVolume = Mathf.Clamp01(volume);
        }

        /// <inheritdoc />
        public void AttachToSpatialCard(SpatialCardInteractable interactable) {
            if (interactable == null || !_trackedSpatialCards.Add(interactable)) {
                return;
            }

            interactable.OnCardGrabbed += HandleCardGrabbed;
            interactable.OnCardDropped += HandleCardDropped;
        }

        /// <inheritdoc />
        public void DetachFromSpatialCard(SpatialCardInteractable interactable) {
            if (interactable == null || !_trackedSpatialCards.Remove(interactable)) {
                return;
            }

            interactable.OnCardGrabbed -= HandleCardGrabbed;
            interactable.OnCardDropped -= HandleCardDropped;
        }

        /// <summary>
        /// Handles the card grab event from a tracked spatial card and plays the grab sound.
        /// </summary>
        /// <param name="interactable">The spatial card interactable that raised the event.</param>
        private void HandleCardGrabbed(SpatialCardInteractable interactable) {
            _ = interactable;
            PlayCardGrab();
        }

        /// <summary>
        /// Handles the card drop event from a tracked spatial card and plays the drop sound.
        /// </summary>
        /// <param name="interactable">The spatial card interactable that raised the event.</param>
        private void HandleCardDropped(SpatialCardInteractable interactable) {
            _ = interactable;
            PlayCardDrop();
        }

        /// <summary>
        /// Plays a single clip with the configured volume and a subtle random pitch so repeated playback does not sound mechanical.
        /// </summary>
        /// <param name="clip">The clip to play.</param>
        /// <param name="clipVolume">The target playback volume for the clip.</param>
        private void PlayOneShot(AudioClip clip, float clipVolume) {
            if (clip == null || audioSource == null) {
                return;
            }

            audioSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
            audioSource.volume = Mathf.Clamp01(clipVolume * masterVolume);
            audioSource.PlayOneShot(clip);
        }
    }
}
