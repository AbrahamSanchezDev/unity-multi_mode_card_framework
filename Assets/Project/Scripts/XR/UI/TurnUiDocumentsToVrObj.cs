using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TurnUiDocumentsToVrObj : MonoBehaviour, IViewInitObj {

    public static UnityEvent RePositionUi = new UnityEvent();
    [SerializeField] private TurnUiDocumentsToVrObjData data;
    [SerializeField] private Transform vrObjParent;
    [SerializeField] private VisualTreeAsset vrUiVersion;

    private bool _initialized = false;

    protected void Awake() {
#if VR
        Init();
#endif
    }

    protected void OnEnable() {
        RePositionUi.AddListener(RepositionUi);
    }
    protected void OnDisable() {
        RePositionUi.RemoveListener(RepositionUi);
    }

    private void RepositionUi() {
        if (data.copyTransform && data.keepParent == false) {
            SetParentPos();
        }
    }
    private void SetParentPos() {
        if (data.copyTransform && data.keepParent == false) {
            transform.SetParent(vrObjParent, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            transform.SetParent(null, true);
        }
    }

    public void Init() {
        if (_initialized) return;
        _initialized = true;

        var uiDocs = GetComponentsInChildren<UIDocument>(true);
        foreach (var doc in uiDocs) {
            doc.panelSettings = data.vrPanelSettings;
            if (data.copyTransform) {
                doc.transform.SetParent(vrObjParent, false);
                doc.transform.localPosition = Vector3.zero;
                doc.transform.localRotation = Quaternion.identity;
                doc.transform.localScale = Vector3.one;
                if (data.keepParent == false)
                    doc.transform.SetParent(null, true);
            }

            if (data.addVrComponents) {
                var interactable = AddOrGetComponent<XRSimpleInteractable>(doc.gameObject);
                var collider = AddOrGetComponent<BoxCollider>(doc.gameObject);
                collider.enabled = true;

                var filter = AddOrGetComponent<XRPokeFilter>(doc.gameObject);
                if (filter) {
                    filter.pokeInteractable = interactable;
                    filter.pokeCollider = collider;
                }

                if (vrUiVersion) {
                    doc.visualTreeAsset = vrUiVersion;
                }
            }
            if (data.changeWorldSpaceDimensions) {
                doc.worldSpaceSize = data.worldScale;
            }

        }
    }

    private T AddOrGetComponent<T>(GameObject obj) where T : Component {
        var theComponent = obj.GetComponent<T>();
        if (theComponent == null) {
            obj.AddComponent<T>();
        }
        return theComponent;
    }
}