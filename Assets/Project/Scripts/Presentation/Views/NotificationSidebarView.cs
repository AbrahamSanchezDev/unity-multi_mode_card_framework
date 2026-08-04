using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using CardFramework.Core.Models;
using CardFramework.Core.Managers;
using VContainer;
using System.Threading.Tasks;
using CardFramework.Cloud.Interfaces;

namespace CardFramework.Presentation.Views {
    [RequireComponent(typeof(UIDocument))]
    public class NotificationSidebarView : MonoBehaviour {
        public event Action<NotificationItem> OnClaimRewardRequested;
        public event Action<NotificationItem> OnClaimRewardCompleted;

        private VisualElement _root;
        private VisualElement _sidebarContainer;
        private ScrollView _notificationsScroll;
        private Button _btnToggleSidebar;

        // Modal Pop-up references for reading/claiming mail details
        private VisualElement _mailModalOverlay;
        private Label _lblModalTitle;
        private Label _lblModalBody;
        private Button _btnClaimReward;
        private Button _btnCloseModal;
        private VisualElement _sidebarBody;

        private NotificationItem _selectedActiveItem;
        private CloudMailboxManager _mailboxManager;
        private ICloudService _authService; // Swapped to Interface contract
        private Coroutine _cooldownTimerCoroutine;
        private bool _isSidebarOpen = false;
        private bool _isInitialized = false;

        [Inject]
        public void Construct(CloudMailboxManager mailboxManager, ICloudService authService) {
            _mailboxManager = mailboxManager;
            _authService = authService;

            if (_authService.IsAuthenticated) {
                _ = LoadNotificationsAsync();
            }
            else {
                _authService.OnAuthenticationSuccess += OnLoginSuccess;
            }
        }

        private void OnEnable() {
            InitUi();
        }

        private void OnDisable() {
            if (_authService != null) {
                _authService.OnAuthenticationSuccess -= OnLoginSuccess;
            }
            StopCooldownTimer();
        }

        private void OnLoginSuccess() {
            // Safe to execute since UI elements are fully mapped out now
            _ = LoadNotificationsAsync();

            if (_authService != null) {
                _authService.OnAuthenticationSuccess -= OnLoginSuccess;
            }
        }

        private void InitUi() {
            if (_isInitialized) return;

            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null) return;

            _root = uiDocument.rootVisualElement;

            // Structural elements queries
            _sidebarContainer = _root.Q<VisualElement>("sidebar-container");
            _sidebarBody = _root.Q<VisualElement>("sidebar-body");
            _notificationsScroll = _root.Q<ScrollView>("notifications-scroll");
            _btnToggleSidebar = _root.Q<Button>("btn-toggle-sidebar");

            // Detail Modal elements queries
            _mailModalOverlay = _root.Q<VisualElement>("mail-modal-overlay");
            _lblModalTitle = _root.Q<Label>("lbl-modal-title");
            _lblModalBody = _root.Q<Label>("lbl-modal-body");
            _btnClaimReward = _root.Q<Button>("btn-claim-reward");
            _btnCloseModal = _root.Q<Button>("btn-close-modal");

            // Assign operational click callbacks
            if (_btnToggleSidebar != null) _btnToggleSidebar.clicked += ToggleSidebarState;
            if (_btnCloseModal != null) _btnCloseModal.clicked += () => OpenMailModal(false);
            if (_btnClaimReward != null) _btnClaimReward.clicked += HandleClaimClicked;

            // Set clean starting presentation layers layout
            OpenMailModal(false);
            SetSidebarOpenState(false);

            // Populates base structure immediately so the drawer doesn't crash or look broken before login finishes
            // LoadMockNotifications();
            _isInitialized = true;
        }

        private async Task LoadNotificationsAsync() {
            bool dailyRewardClaimed = false;

            // Double check initialization boundaries
            if (_notificationsScroll == null) return;

            if (_mailboxManager != null) {
                // Query live server information safely via PlayFab cloud context tracking maps
                dailyRewardClaimed = await _mailboxManager.IsDailyRewardClaimedAsync();
            }

            var testItems = new List<NotificationItem>
            {
                new NotificationItem
                {
                    Id = "daily_reward_01",
                    Type = NotificationType.Mail,
                    Title = "DAILY CASINO BONUS",
                    Description = "Claim your daily gold stack. This offer checks Server side clock directly to verify authentication.",
                    RewardAmount = 500,
                    IsClaimed = dailyRewardClaimed,
                    Timestamp = DateTime.UtcNow
                },
                new NotificationItem
                {
                    Id = "welcome_achievement",
                    Type = NotificationType.Achievement,
                    Title = "WELCOME TO THE CARDS ROOM",
                    Description = "Congratulations! You successfully checked into the casino central multi-game arena.",
                    RewardAmount = 0,
                    IsClaimed = true,
                    Timestamp = DateTime.UtcNow.AddMinutes(-30)
                }
            };

            PopulateNotifications(testItems);
        }

        private void ToggleSidebarState() {
            SetSidebarOpenState(!_isSidebarOpen);
        }

        private void SetSidebarOpenState(bool open) {
            _isSidebarOpen = open;
            if (_sidebarContainer == null || _sidebarBody == null) return;

            if (_isSidebarOpen) {
                _sidebarContainer.RemoveFromClassList("sidebar-collapsed");
                _sidebarContainer.AddToClassList("sidebar-expanded");
                _sidebarBody.pickingMode = PickingMode.Position;
            }
            else {
                _sidebarContainer.RemoveFromClassList("sidebar-expanded");
                _sidebarContainer.AddToClassList("sidebar-collapsed");
                _sidebarBody.pickingMode = PickingMode.Ignore;
            }
        }

        public void PopulateNotifications(List<NotificationItem> items) {
            if (_notificationsScroll == null) return;
            _notificationsScroll.Clear();

            foreach (var item in items) {
                var itemRow = new Button();
                itemRow.AddToClassList("notification-item-row");

                var iconElement = new VisualElement();
                iconElement.AddToClassList("notification-icon");
                iconElement.AddToClassList(GetIconClass(item.Type));
                itemRow.Add(iconElement);

                var textContainer = new VisualElement();
                textContainer.AddToClassList("notification-text-container");

                var titleLabel = new Label(item.Title);
                titleLabel.AddToClassList("notification-item-title");
                textContainer.Add(titleLabel);

                var subtitleLabel = new Label(item.IsClaimed ? "Claimed" : "Tap to open");
                subtitleLabel.AddToClassList("notification-item-subtitle");
                textContainer.Add(subtitleLabel);

                itemRow.Add(textContainer);

                itemRow.clicked += () => OnNotificationItemClicked(item);
                _notificationsScroll.Add(itemRow);
            }
        }

        private async void OnNotificationItemClicked(NotificationItem item) {
            _selectedActiveItem = item;

            if (_lblModalTitle != null) _lblModalTitle.text = item.Title;
            if (_lblModalBody != null) _lblModalBody.text = item.Description;

            StopCooldownTimer();

            if (item.IsClaimed && item.RewardAmount > 0) {
                // Item is already claimed — trigger the live server countdown loop
                _btnClaimReward.style.display = DisplayStyle.Flex;
                _btnClaimReward.SetEnabled(false);

                if (_mailboxManager != null) {
                    TimeSpan initialCooldown = await _mailboxManager.GetRemainingCooldownAsync();
                    _cooldownTimerCoroutine = StartCoroutine(RunCooldownTimerRoutine(initialCooldown));
                }
            }
            else if (item.RewardAmount > 0 && !item.IsClaimed) {
                // Item is ready to be claimed
                _btnClaimReward.style.display = DisplayStyle.Flex;
                _btnClaimReward.SetEnabled(true);
                _btnClaimReward.text = $"CLAIM {item.RewardAmount} GOLD";
            }
            else {
                _btnClaimReward.style.display = DisplayStyle.None;
            }

            OpenMailModal(true);
        }

        private IEnumerator RunCooldownTimerRoutine(TimeSpan initialCooldown) {
            TimeSpan remaining = initialCooldown;

            // Direct C# style overrides to fix low contrast on disabled state
            _btnClaimReward.style.color = new StyleColor(new Color(0.9f, 0.75f, 0.3f)); // Gold/Yellow tint to match header
            _btnClaimReward.style.backgroundColor = new StyleColor(new Color(0.12f, 0.12f, 0.12f, 0.9f)); // Dark background container

            while (remaining.TotalSeconds > 0) {
                _btnClaimReward.text = $"NEXT CLAIM IN: {FormatTimeSpan(remaining)}";
                yield return new WaitForSecondsRealtime(1.0f);
                remaining = remaining.Subtract(TimeSpan.FromSeconds(1));
            }
            
            // Reset styles back when available again
            _btnClaimReward.style.color = StyleKeyword.Null;
            _btnClaimReward.style.backgroundColor = StyleKeyword.Null;

            // Cooldown finished while modal was open
            _btnClaimReward.text = "REFRESHING...";
            _ = LoadNotificationsAsync();

            if (_selectedActiveItem != null) {
                _selectedActiveItem.IsClaimed = false;
                _btnClaimReward.SetEnabled(true);
                _btnClaimReward.text = $"CLAIM {_selectedActiveItem.RewardAmount} GOLD";
            }
        }

        private string FormatTimeSpan(TimeSpan span) {
            if (span.TotalHours >= 1) {
                return $"{span.Hours:D2}h {span.Minutes:D2}m {span.Seconds:D2}s";
            }
            return $"{span.Minutes:D2}m {span.Seconds:D2}s";
        }

        private void StopCooldownTimer() {
            if (_cooldownTimerCoroutine != null) {
                StopCoroutine(_cooldownTimerCoroutine);
                _cooldownTimerCoroutine = null;
            }
        }

        private async void HandleClaimClicked() {
            if (_selectedActiveItem == null || _mailboxManager == null) return;
            OnClaimRewardRequested?.Invoke(_selectedActiveItem);

            _btnClaimReward.SetEnabled(false);
            _btnClaimReward.text = "PROCESSING...";

            bool success = await _mailboxManager.TryClaimDailyRewardAsync(_selectedActiveItem.RewardAmount);

            if (success) {
                _selectedActiveItem.IsClaimed = true;
                OnClaimRewardCompleted?.Invoke(_selectedActiveItem);
                await LoadNotificationsAsync();

                // Immediately switch over to the live countdown timer upon successful claim
                TimeSpan initialCooldown = await _mailboxManager.GetRemainingCooldownAsync();
                _cooldownTimerCoroutine = StartCoroutine(RunCooldownTimerRoutine(initialCooldown));
            }
            else {
                _btnClaimReward.text = "CLAIM FAILED";
                _btnClaimReward.SetEnabled(true);
            }
        }

        private void OpenMailModal(bool open) {
            if (_mailModalOverlay == null) return;
            if (!open) StopCooldownTimer();
            _mailModalOverlay.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private string GetIconClass(NotificationType type) {
            return type switch {
                NotificationType.Mail => "icon-mail",
                NotificationType.Achievement => "icon-achievement",
                NotificationType.Event => "icon-event",
                _ => "icon-mail"
            };
        }

        private void LoadMockNotifications() {
            var testItems = new List<NotificationItem>
            {
                new NotificationItem
                {
                    Id = "daily_reward_01",
                    Type = NotificationType.Mail,
                    Title = "DAILY CASINO BONUS",
                    Description = "Claim your daily gold stack. This offer checks Server side clock directly to verify authentication.",
                    RewardAmount = 500,
                    IsClaimed = false,
                    Timestamp = DateTime.UtcNow
                },
                new NotificationItem
                {
                    Id = "welcome_achievement",
                    Type = NotificationType.Achievement,
                    Title = "WELCOME TO THE CARDS ROOM",
                    Description = "Congratulations! You successfully checked into the casino central multi-game arena.",
                    RewardAmount = 0,
                    IsClaimed = true,
                    Timestamp = DateTime.UtcNow.AddMinutes(-30)
                }
            };

            PopulateNotifications(testItems);
        }
    }
}