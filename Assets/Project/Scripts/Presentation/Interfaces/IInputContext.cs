using System;
using UnityEngine;

namespace CardFramework.Presentation.Interfaces {
    public interface IInputContext {
        // Triggers when a primary selection action occurs (Click, Screen Tap, or VR Trigger Press)
        event Action OnPrimarySelectPerformed;

        // Returns the screen-space pointer coordinate or VR Raycast origin depending on active hardware
        Vector2 GetPointerPosition();

        // Convenience utility to check if a selection is currently held down
        bool IsSelectHeld();
    }
}