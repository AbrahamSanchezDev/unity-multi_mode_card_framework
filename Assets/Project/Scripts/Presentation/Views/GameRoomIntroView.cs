using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using CardFramework.Core.Interfaces;

namespace CardFramework.Presentation.Views {
    [RequireComponent(typeof(UIDocument))]
    public class GameRoomIntroView : MonoBehaviour {
        [Header("Room Data")]
        [SerializeField] private GameRoomIntroData introData;

        [Header("Currency Display")]
        [SerializeField] private string currencyCode = "GD";

        [Header("References")]
        [SerializeField] private AudioClip introAudio;
        [SerializeField] private float audioVolume = 0.7f;

        private UIDocument _uiDocument;
        private VisualElement _root;
        private VisualElement _introRoot;
        private Label _titleLabel;
        private Label _descriptionLabel;
        private Image _heroImageElement;
        private VisualElement _modesGrid;
        private Button _modeButton0;
        private Button _modeButton1;
        private Button _modeButton2;
        private Button _modeButton3;
        private VisualElement _modeIcon0;
        private VisualElement _modeIcon1;
        private VisualElement _modeIcon2;
        private VisualElement _modeIcon3;
        private Label _modeLabel0;
        private Label _modeLabel1;
        private Label _modeLabel2;
        private Label _modeLabel3;
        private Label _modeSub0;
        private Label _modeSub1;
        private Label _modeSub2;
        private Label _modeSub3;
        private Button _randomButton;
        private Action[] _modeHandlers = new Action[4];
        private Action _randomHandler;
        private Label _currencyLabel;
        private AudioSource _audioSource;
        private CurrencyDisplayHelper _currencyDisplayHelper;
        private Coroutine _heroAnimationRoutine;

        public event Action<string> OnOptionSelected;

        private void Awake() {
            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument != null) {
                _uiDocument.enabled = true;
            }

            _audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.volume = audioVolume;
        }

        private void OnEnable() {
            if (_uiDocument != null) {
                _root = _uiDocument.rootVisualElement;
            }

            EnsureIntroData();
            BuildLayout();
            PopulateFromData();
            Show();
        }

        private void OnDisable() {
            if (_heroAnimationRoutine != null) {
                StopCoroutine(_heroAnimationRoutine);
                _heroAnimationRoutine = null;
            }
            _currencyDisplayHelper?.Dispose();
            _currencyDisplayHelper = null;

        }

        [ContextMenu("Show Intro View")]
        public void Show() {
            SetVisible(true);
        }

        [ContextMenu("Hide Intro View")]
        public void Hide() {
            SetVisible(false);
        }

        public void SetVisible(bool visible) {
            var container = _introRoot ?? _root;
            if (container == null) {
                Debug.LogWarning("[GameRoomIntroView] SetVisible() called but the visual root is null. Ensure the UIDocument is properly set up.");
                return;
            }

            if (_uiDocument != null) {
                _uiDocument.enabled = visible;
            }

            container.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (!visible) {
                Debug.Log("[GameRoomIntroView] Hidden. The intro panel is now removed from the UI.");
            }
            else {
                PlayIntroAudio();
            }
            DoHeroAnimation(visible);
        }

        public void SetData(GameRoomIntroData data) {
            introData = data;
            PopulateFromData();
        }

        public void InjectEconomy(IEconomyService economyService) {
            BindEconomy(economyService);
        }

        private void BuildLayout() {
            if (_uiDocument == null) {
                _uiDocument = GetComponent<UIDocument>();
            }

            if (_uiDocument == null) {
                Debug.LogWarning("[GameRoomIntroView] No UIDocument attached. The intro view cannot build its UI.");
                return;
            }

            if (_uiDocument.visualTreeAsset == null) {
                var visualTree = Resources.Load<VisualTreeAsset>("GameRoomIntro");
                if (visualTree != null) {
                    _uiDocument.visualTreeAsset = visualTree;
                }
                else {
                    Debug.LogWarning("[GameRoomIntroView] Could not load GameRoomIntro visual tree asset from Resources.");
                }
            }

            _root = _uiDocument.rootVisualElement;
            if (_root == null) {
                Debug.LogWarning("[GameRoomIntroView] UIDocument root is still null after assigning the visual tree asset.");
                return;
            }

            _introRoot = _root.Q<VisualElement>("intro-root");
            if (_introRoot == null) {
                _introRoot = _root;
            }
            _introRoot.style.display = DisplayStyle.None;

            _titleLabel = _root.Q<Label>("title-label");
            _descriptionLabel = _root.Q<Label>("description-label");
            _currencyLabel = _root.Q<Label>("currency-label");

            _heroImageElement = _root.Q<Image>("hero-image");
            _modesGrid = _root.Q<VisualElement>("modes-grid");

            _modeButton0 = _root.Q<Button>("option-0");
            _modeButton1 = _root.Q<Button>("option-1");
            _modeButton2 = _root.Q<Button>("option-2");
            _modeButton3 = _root.Q<Button>("option-3");

            _modeIcon0 = _root.Q<VisualElement>("option-0-icon");
            _modeIcon1 = _root.Q<VisualElement>("option-1-icon");
            _modeIcon2 = _root.Q<VisualElement>("option-2-icon");
            _modeIcon3 = _root.Q<VisualElement>("option-3-icon");

            _modeLabel0 = _root.Q<Label>("option-0-label");
            _modeLabel1 = _root.Q<Label>("option-1-label");
            _modeLabel2 = _root.Q<Label>("option-2-label");
            _modeLabel3 = _root.Q<Label>("option-3-label");

            _modeSub0 = _root.Q<Label>("option-0-sub");
            _modeSub1 = _root.Q<Label>("option-1-sub");
            _modeSub2 = _root.Q<Label>("option-2-sub");
            _modeSub3 = _root.Q<Label>("option-3-sub");

            _randomButton = _root.Q<Button>("random-button");

        }


        private IEnumerator AnimateHeroFloat() {
            const float amplitude = 15f; // px
            const float period = 6f;     // seconds, matches the original CSS
            while (_heroImageElement != null) {
                float y = -Mathf.Sin(Time.time * (2f * Mathf.PI / period)) * amplitude;
                float rot = Mathf.Sin(Time.time * (2f * Mathf.PI / period)) * 1f;
                _heroImageElement.style.translate = new Translate(0, y);
                _heroImageElement.style.rotate = new Rotate(rot);
                yield return null;
            }
        }
        private void EnsureIntroData() {
            if (introData != null) return;

            introData = ScriptableObject.CreateInstance<CardGamesRoomIntroData>();
            introData.roomTitle = "Cards Game Room";
            introData.roomDescription = "Welcome to the cards game room. Pick a table to begin.";
            introData.options = new[] {
                new GameRoomOptionData { optionId = "Blackjack", label = "Blackjack", description = "Play classic 21", accentColor = new Color(0.95f, 0.76f, 0.22f, 1f) },
                new GameRoomOptionData { optionId = "Solitaire", label = "Solitaire", description = "Relax with a solo card challenge", accentColor = new Color(0.21f, 0.56f, 0.86f, 1f) },
                new GameRoomOptionData { optionId = "TexasHoldem", label = "Texas Hold'em", description = "Face off at the poker table", accentColor = new Color(0.77f, 0.24f, 0.26f, 1f) }
            };
        }

        private void DoHeroAnimation(bool show) {
            if (_heroAnimationRoutine != null) {
                StopCoroutine(_heroAnimationRoutine);
            }
            if (show)
                _heroAnimationRoutine = StartCoroutine(AnimateHeroFloat2());
        }

        private void PopulateFromData() {
            if (_root == null || introData == null) return;

            if (_titleLabel != null) _titleLabel.text = introData.roomTitle;
            if (_descriptionLabel != null) _descriptionLabel.text = introData.roomDescription;

            // Hero image handling
            if (_heroImageElement != null) {
                if (introData.useSpriteAnimation && introData.heroFrames != null && introData.heroFrames.Length > 0) {
                    _heroImageElement.scaleMode = ScaleMode.ScaleToFit;
                    _heroImageElement.sprite = introData.heroFrames[0];
                    DoHeroAnimation(true);
                }
                else if (introData.heroSprite != null) {
                    _heroImageElement.scaleMode = ScaleMode.ScaleToFit;
                    _heroImageElement.sprite = introData.heroSprite;
                }
            }

            var modeButtons = new[] {
                (_modeButton0, _modeIcon0, _modeLabel0, _modeSub0, 0),
                (_modeButton1, _modeIcon1, _modeLabel1, _modeSub1, 1),
                (_modeButton2, _modeIcon2, _modeLabel2, _modeSub2, 2),
                (_modeButton3, _modeIcon3, _modeLabel3, _modeSub3, 3)
            };

            for (int i = 0; i < modeButtons.Length; i++) {
                var (button, icon, label, sub, index) = modeButtons[i];
                if (button == null) continue;
                if (_modeHandlers[index] != null) {
                    button.clicked -= _modeHandlers[index];
                    _modeHandlers[index] = null;
                }
                button.style.display = DisplayStyle.None;
                if (icon != null) {
                    icon.style.backgroundImage = new StyleBackground();
                    icon.style.backgroundColor = new Color(1f, 0.76f, 0.42f, 0.18f);
                }
                if (label != null) label.text = string.Empty;
                if (sub != null) sub.text = string.Empty;
            }

            if (introData.options != null) {
                int count = Mathf.Min(introData.options.Length, modeButtons.Length);
                for (int i = 0; i < count; i++) {
                    var option = introData.options[i];
                    if (option == null) continue;

                    var (button, icon, label, sub, index) = modeButtons[i];
                    if (button == null || label == null) continue;

                    button.style.display = DisplayStyle.Flex;
                    label.text = option.label;
                    if (sub != null) sub.text = option.description;
                    button.style.backgroundImage = new StyleBackground();
                    button.style.backgroundColor = new Color(0.094f, 0.110f, 0.133f, 0.90f);

                    if (icon != null) {
                        icon.style.backgroundColor = option.accentColor;
                        icon.style.opacity = 0.16f;
                        // icon.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                        if (option.icon != null) {
                            icon.style.backgroundImage = new StyleBackground(option.icon);
                            icon.style.opacity = 1f;
                        }
                    }

                    void HandleClick() => OnOptionSelected?.Invoke(option.optionId);
                    _modeHandlers[index] = HandleClick;
                    button.clicked += HandleClick;
                }
            }

            if (_randomButton != null) {
                if (_randomHandler != null) _randomButton.clicked -= _randomHandler;
                void HandleRandom() {
                    if (introData.options == null || introData.options.Length == 0) return;
                    int max = Mathf.Min(introData.options.Length, modeButtons.Length);
                    int r = UnityEngine.Random.Range(0, max);
                    OnOptionSelected?.Invoke(introData.options[r].optionId);
                }
                _randomHandler = HandleRandom;
                _randomButton.clicked += _randomHandler;
            }
        }

        private void BindEconomy(IEconomyService economyService) {
            _currencyDisplayHelper?.Dispose();
            _currencyDisplayHelper = null;
            if (_currencyLabel != null && economyService != null) {
                _currencyDisplayHelper = new CurrencyDisplayHelper(economyService, balance => {
                    _currencyLabel.text = CurrencyDisplayHelper.FormatBalance(balance, currencyCode);
                }, currencyCode);
            }
            else if (_currencyLabel != null) {
                _currencyLabel.text = CurrencyDisplayHelper.FormatBalance(0, currencyCode);
            }
        }

        private IEnumerator AnimateHeroFloat2() {
            const float amplitude = 15f; // px
            const float period = 6f;     // seconds, matches the original CSS
            while (_heroImageElement != null) {
                float y = -Mathf.Sin(Time.time * (2f * Mathf.PI / period)) * amplitude;
                float rot = Mathf.Sin(Time.time * (2f * Mathf.PI / period)) * 1f;
                _heroImageElement.style.translate = new Translate(0, y);
                _heroImageElement.style.rotate = new Rotate(rot);
                yield return null;
            }
        }
        private void PlayIntroAudio() {
            if (_audioSource == null || introAudio == null) return;
            if (_audioSource.isPlaying) {
                _audioSource.Stop();
            }
            _audioSource.PlayOneShot(introAudio);
        }
    }
}