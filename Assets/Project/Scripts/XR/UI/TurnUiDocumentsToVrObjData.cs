using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "TurnUiDocumentsToVrObjData", menuName = "VR/TurnUiDocumentsToVrObjData", order = 1)]
public class TurnUiDocumentsToVrObjData : ScriptableObject {
    public PanelSettings vrPanelSettings;

    public bool copyTransform = true;
    public bool keepParent;

    public bool addVrComponents = true;
    public bool changeWorldSpaceDimensions;

    public Vector2 worldScale = new Vector2(400f, 400f);

}
