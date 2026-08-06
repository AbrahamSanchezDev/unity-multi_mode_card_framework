using System;
using CardFramework.Presentation.Views;

namespace CardFramework.Presentation.Interfaces {
    /// <summary>
    /// Provides a centralized store for gameplay settings that can be persisted between sessions.
    /// </summary>
    public interface IGameSettingsService {
        /// <summary>
        /// Fires when the active card display type selection changes.
        /// </summary>
        event Action<CardDisplayType> OnCardDisplayTypeChanged;

        /// <summary>
        /// The currently selected card face generation display type.
        /// </summary>
        CardDisplayType CardDisplayType { get; set; }

        /// <summary>
        /// Loads persisted settings values from PlayerPrefs.
        /// </summary>
        void Load();

        /// <summary>
        /// Persists settings values to PlayerPrefs.
        /// </summary>
        void Save();
    }
}
