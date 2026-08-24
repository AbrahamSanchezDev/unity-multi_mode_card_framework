using System.Collections.Generic;
using UnityEngine;

// Centralized enforcer to clamp grabbed transforms' Y to a minimum value.
// ponytail: single LateUpdate when needed to avoid per-card LateUpdate overhead.
public class VRGrabYEnforcer : MonoBehaviour {
    private static VRGrabYEnforcer _instance;

    private readonly Dictionary<Transform, float> _minYByTransform = new();

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this);
            return;
        }
        _instance = this;
        enabled = false; // disabled until someone registers
    }

    private void LateUpdate() {
        if (_minYByTransform.Count == 0) {
            enabled = false;
            return;
        }

        foreach (var kvp in _minYByTransform) {
            var t = kvp.Key;
            if (t == null) continue;
            float minY = kvp.Value;
            var pos = t.position;
            if (pos.y < minY) t.position = new Vector3(pos.x, minY, pos.z);
        }
    }

    public static void Register(Transform t, float minY) {
        if (t == null) return;
        EnsureInstanceExists();
        _instance._minYByTransform[t] = minY;
        _instance.enabled = true;
    }

    public static void Unregister(Transform t) {
        if (t == null) return;
        if (_instance == null) return;
        _instance._minYByTransform.Remove(t);
        if (_instance._minYByTransform.Count == 0) _instance.enabled = false;
    }

    private static void EnsureInstanceExists() {
        if (_instance != null) return;
        var go = new GameObject("VRGrabYEnforcer");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<VRGrabYEnforcer>();
    }
}