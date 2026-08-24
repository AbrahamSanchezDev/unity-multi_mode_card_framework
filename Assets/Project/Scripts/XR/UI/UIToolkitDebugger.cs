using UnityEngine;
using UnityEngine.UIElements;

public class UIToolkitDebugger : MonoBehaviour
{
    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        root.RegisterCallback<PointerDownEvent>(evt => Debug.Log($"UI Toolkit received PointerDown! {gameObject.name}"), TrickleDown.TrickleDown);
        root.RegisterCallback<ClickEvent>(evt => Debug.Log($"UI Toolkit received Click! {gameObject.name}"), TrickleDown.TrickleDown);
    }
}