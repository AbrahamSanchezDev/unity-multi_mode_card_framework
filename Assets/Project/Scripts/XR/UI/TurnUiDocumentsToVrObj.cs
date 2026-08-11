using UnityEngine;
using UnityEngine.UIElements;

public class TurnUiDocumentsToVrObj : MonoBehaviour {

    [SerializeField] private Transform vrObjParent;
    [SerializeField] private PanelSettings vrPanelSettings;

    [SerializeField] private bool copyTransform = true;

    protected void Awake() {
        var uiDocs = GetComponentsInChildren<UIDocument>(true);
        foreach (var doc in uiDocs) {
            doc.panelSettings = vrPanelSettings;
            if (copyTransform) {
                doc.transform.SetParent(vrObjParent, false);
                doc.transform.localPosition = Vector3.zero;
                doc.transform.localRotation = Quaternion.identity;
                doc.transform.localScale = Vector3.one;
            }


        }
    }
}