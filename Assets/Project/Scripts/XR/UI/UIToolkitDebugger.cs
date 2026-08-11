using UnityEngine;
using UnityEngine.UIElements;

public class UIToolkitDebugger : MonoBehaviour
{
    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        root.RegisterCallback<PointerDownEvent>(evt => Debug.Log("UI Toolkit received PointerDown!"), TrickleDown.TrickleDown);
        root.RegisterCallback<ClickEvent>(evt => Debug.Log("UI Toolkit received Click!"), TrickleDown.TrickleDown);
    }
}