using UnityEngine;

public class ForceOVRFocus : MonoBehaviour
{
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && Application.isEditor)
        {
            // Forces input processing to remain active in Editor even when focus shifts
            Input.simulateMouseWithTouches = true;
        }
    }
}