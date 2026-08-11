using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TurnUiDocumentsToVrObj : MonoBehaviour, IViewInitObj {

    public static UnityEvent RePositionUi = new UnityEvent();

    [SerializeField] private Transform vrObjParent;
    [SerializeField] private PanelSettings vrPanelSettings;

    [SerializeField] private VisualTreeAsset vrUiVersion;
    [SerializeField] private bool turnToVR = true;
    [SerializeField] private bool copyTransform = true;
    [SerializeField] private bool keepParent;

    [SerializeField] private bool addVrComponents = true;
    [SerializeField] private bool changeWorldSpaceDimensions;

    [SerializeField] private Vector2 worldScale = new Vector2(400f, 400f);
    private bool _initialized = false;

    protected void Awake() {
        Init();
    }

    protected void OnEnable() {
        RePositionUi.AddListener(RepositionUi);
    }
    protected void OnDisable() {
        RePositionUi.RemoveListener(RepositionUi);
    }

    private void RepositionUi() {
        if (copyTransform && keepParent == false) {
            SetParentPos();
        }
    }
    private void SetParentPos() {
        if (copyTransform && keepParent == false) {
            transform.SetParent(vrObjParent, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            transform.SetParent(null, true);
        }
    }

    public void Init() {
        if (_initialized || !turnToVR) return;
        _initialized = true;

        var uiDocs = GetComponentsInChildren<UIDocument>(true);
        foreach (var doc in uiDocs) {
            doc.panelSettings = vrPanelSettings;
            if (copyTransform) {
                doc.transform.SetParent(vrObjParent, false);
                doc.transform.localPosition = Vector3.zero;
                doc.transform.localRotation = Quaternion.identity;
                doc.transform.localScale = Vector3.one;
                if (keepParent == false)
                    doc.transform.SetParent(null, true);
            }

            if (addVrComponents) {
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
            if (changeWorldSpaceDimensions) {
                doc.worldSpaceSize = worldScale;
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