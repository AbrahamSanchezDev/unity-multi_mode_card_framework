using UnityEngine;

public class DisableObjOnVr : MonoBehaviour {
    protected void Awake() {
#if VR
        gameObject.SetActive(false);
#endif
    }

}