using System;

namespace CardFramework.Presentation.Interfaces {
    public interface IModalService {
        void ShowLoading(string message);
        void ShowAlert(string title, string message, Action onConfirm = null);
        void ShowConfirmation(string title, string message, Action onConfirm, Action onCancel);
        void HideModal();
    }
}