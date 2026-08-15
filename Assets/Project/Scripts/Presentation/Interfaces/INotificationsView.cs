using System;
using CardFramework.Presentation.Views;

namespace CardFramework.Presentation.Views {
    /// <summary>
    /// Control for displaying notifications to the user, such as game events or messages.
    /// </summary>
    public interface INotificationsView {

        void ToggleNotificationDisplay();
    }
}
