using UnityEngine;
using CardFramework.Presentation.Interfaces;

namespace CardFramework.Presentation.Views {
    /// <summary>
    /// Persistent runtime settings for gameplay that are stored in PlayerPrefs.
    /// </summary>
    public class GameSettingsService : MonoBehaviour, IGameSettingsService {
        public static GameSettingsService Instance { get; private set; }
        public static CardDisplayType CurrentCardDisplayType => Instance != null ? Instance.CardDisplayType : CardDisplayType.EasyRead;

        public event System.Action<CardDisplayType> OnCardDisplayTypeChanged;

        private const string CardDisplayTypeKey = "GameSettings_CardDisplayType";

        [SerializeField]
        private CardDisplayType _cardDisplayType = CardDisplayType.EasyRead;

        public CardDisplayType CardDisplayType {
            get => _cardDisplayType;
            set {
                if (_cardDisplayType == value) return;
                _cardDisplayType = value;
                OnCardDisplayTypeChanged?.Invoke(_cardDisplayType);
            }
        }

        private void Awake() {
            Instance = this;
            Load();
        }

        private void OnDestroy() {
            if (Instance == this) {
                Instance = null;
            }
            Save();
        }

        public void Load() {
            if (PlayerPrefs.HasKey(CardDisplayTypeKey)) {
                string savedValue = PlayerPrefs.GetString(CardDisplayTypeKey, CardDisplayType.EasyRead.ToString());
                if (System.Enum.TryParse(savedValue, out CardDisplayType parsed)) {
                    _cardDisplayType = parsed;
                }
                else {
                    _cardDisplayType = CardDisplayType.EasyRead;
                }
            }
            else {
                _cardDisplayType = CardDisplayType.EasyRead;
            }
        }

        public void Save() {
            PlayerPrefs.SetString(CardDisplayTypeKey, _cardDisplayType.ToString());
            PlayerPrefs.Save();
        }
    }
}
