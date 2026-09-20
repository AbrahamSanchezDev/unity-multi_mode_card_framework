using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using CardFramework.Presentation.Interfaces;

public class TurnUiDocumentsToVrObj : MonoBehaviour, IViewInitObj {

    public static UnityEvent RePositionUi = new UnityEvent();
    [SerializeField] private TurnUiDocumentsToVrObjData data;
    [SerializeField] private Transform vrObjParent;
    [SerializeField] private VisualTreeAsset vrUiVersion;

    private bool _initialized = false;
    [SerializeField] private bool startOn;


    protected IEnumerator Start() {
        yield return null;
#if VR
        Init();
        Debug.Log($"TurnUiDocumentsToVrObj: {gameObject.name} initialized");
#endif
    }

    protected void OnEnable() {
        RePositionUi.AddListener(RepositionUi);
    }
    protected void OnDisable() {
        RePositionUi.RemoveListener(RepositionUi);
    }

    private void RepositionUi() {
        if (data.copyTransform) {
            SetParentPos();
        }
    }

    private void SetParentPos() {
        if (data.copyTransform && data.keepParent == false) {
            transform.SetParent(null, true);
            transform.position = vrObjParent.position;
            transform.rotation = vrObjParent.rotation;
            transform.localScale = Vector3.one;
        }
    }

    public void Init() {
        if (_initialized) return;
        _initialized = true;
#if !VR
        Debug.Log($"TurnUiDocumentsToVrObj: {gameObject.name} is not running in VR mode, so it will not be initialized.");
        return;
#endif


        var doc = GetComponent<UIDocument>();

        doc.panelSettings = data.vrPanelSettings;
        if (data.copyTransform) {
            transform.SetParent(null, true);
            doc.transform.position = vrObjParent.position;
            doc.transform.rotation = vrObjParent.rotation;
            doc.transform.localScale = Vector3.one;
        }

        if (data.addVrComponents) {
            var interactable = AddOrGetComponent<XRSimpleInteractable>(doc.gameObject);
            var collider = AddOrGetComponent<BoxCollider>(doc.gameObject);
            var colWasActive = collider.enabled;
            collider.enabled = true;

            var filter = AddOrGetComponent<XRPokeFilter>(doc.gameObject);
            if (filter) {
                filter.pokeInteractable = interactable;
                filter.pokeCollider = collider;
            }

            if (vrUiVersion) {
                doc.visualTreeAsset = vrUiVersion;
                var windowObj = doc.gameObject.GetComponent<IWindowObj>();
                windowObj.UpdateUiReferences();
            }
            collider.enabled = colWasActive;
        }
        if (data.changeWorldSpaceDimensions) {
            doc.worldSpaceSize = data.worldScale;
        }
        UpdateVisualState();
    }

    [ContextMenu("Update Visuals")]
    private void UpdateVisualState() {
        var windowObj = gameObject.GetComponent<IWindowObj>();
        if (windowObj != null) {
            windowObj.ShowUi(false);
            windowObj.ShowUi(startOn);
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