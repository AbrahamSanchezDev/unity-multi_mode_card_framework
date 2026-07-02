using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// Procedurally generates a playing card's front-face texture (corner rank labels,
/// center pips for 1-10, or center art for J/Q/K) and can either:
///   - Preview it live, rebuilt from scratch and positioned directly on top of the
///     card mesh in the Editor (Edit Mode), sized from the card's own Collider, or
///   - Generate + cache the texture once per unique card when running in Play Mode.
///
/// This replaces a static card atlas texture: instead of slicing a big sheet,
/// each card's face is composed and baked on demand from a suit icon, a rank,
/// and (for face cards) a piece of face art.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class CardFaceGenerator : MonoBehaviour {
    [System.Serializable]
    public class CardSetupData {
        [Header("Where the generated texture is applied")]
        [Tooltip("Which material slot on targetRenderer gets the generated texture (e.g. 0 = Card_Front if that's the first slot).")]
        public int materialIndex = 2;

        [Tooltip("Texture property on the material. Use \"_BaseMap\" for URP or \"_MainTex\" for the Built-in Render Pipeline.")]
        public string texturePropertyName = "_BaseMap";

        [Header("Card Dimensions Source")]
        [Tooltip("Local-space direction (relative to cardCollider's transform) that the card's front face points toward. Matches the Blender export convention where the front face normal is local +Z.")]
        public Vector3 frontFaceLocalNormal = Vector3.forward;

        [Tooltip("Small gap kept between the card surface and the generator rig in the Scene view, in world units, purely to avoid z-fighting while previewing.")]
        public float previewSurfaceOffset = -0.0027f;
        [Header("Corner Placement")]
        [Tooltip("Local offset (in the anchor's own space) applied to the suit icon relative to the rank label at that same corner, so they don't overlap.")]
        public Vector3 iconOffsetFromAnchor = new Vector3(0f, -0.01f, 0f);

        [Header("Texture Output")]
        [Tooltip("Horizontal pixel resolution of the generated texture. Height is derived automatically from the card collider's width/height ratio.")]
        public int textureWidth = 1024;

        [Tooltip("Derived automatically from the card collider every time a texture is generated. Only used directly as a fallback aspect ratio if no collider is assigned.")]
        public int textureHeight = 1434;

        public Color backgroundColor = Color.white;

        [Header("Layout - Corner Labels")]
        public Color labelColor = Color.black;
        public float cornerFontSize = 160f;
        public bool boldFont = true;
        public Vector2 cornerIconSize = new Vector2(130f, 130f);

        [Header("Layout - Center Area")]
        [Tooltip("Inset of the center area (pips or face art) from the texture edges, as a 0-1 fraction per axis.")]
        public Vector2 centerAreaMargin = new Vector2(0.15f, 0.12f);
        public Vector2 centerBigImageAreaMargin = new Vector2(0.18f, 0.08f);
        public Vector2 pipSize = new Vector2(200f, 200f);
        [Tooltip("Extra scale applied only to the Ace's single center pip.")]
        public float aceIconScale = 2.2f;
    }

    public enum CardRank {
        Ace = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13
    }

    public CardSetupData setupData = new CardSetupData();

    [Header("Card Data")]
    [Tooltip("Suit symbol used for the corner icons and, for 1-10, the center pips.")]
    public Sprite suitIcon;

    [Tooltip("Card rank. 1-10 draw pips; J/Q/K draw faceCardArt instead.")]
    public CardRank rank = CardRank.Ace;

    [Tooltip("Only used when rank is Jack, Queen or King. Drawn in the center area.")]
    public Sprite faceCardArt;

    public bool deleteRigAfterGeneration;

    [Header("Where the generated texture is applied")]
    [Tooltip("The renderer whose material will receive the generated texture (e.g. the card mesh's renderer).")]
    public Renderer targetRenderer;

    [Header("Card Dimensions Source")]
    [Tooltip("Collider representing the physical card mesh (e.g. the BoxCollider on the card). Its size drives the texture's aspect ratio and where the generator rig sits in the world. If left empty, it will try targetRenderer's Collider, then this GameObject's Collider.")]
    public Collider cardCollider;

    [Header("Corner Placement")]
    [Tooltip("Transform marking where the TOP corner's rank label + suit icon are placed. Position and rotation are both read from this transform.")]
    public Transform topCornerAnchor;

    [Tooltip("Transform marking where the BOTTOM corner's rank label + suit icon are placed. Position and rotation are both read from this transform.")]
    public Transform bottomCornerAnchor;

    [Header("Layout - Corner Labels")]
    public TMP_FontAsset labelFont;

    // Standard pip positions per rank, as normalized (x, y) within the center area.
    // y = 0 is the top row, y = 1 is the bottom row. Any pip with y > 0.5 is
    // automatically rotated 180 degrees (see BuildLayout), which is what produces
    // the "lower half is upside down" rule, including the extra rows added on 8/9/10.
    // NOTE: this pip rotation rule is independent from the top/bottom corner rotation
    // rule below - the corners use topCornerAnchor/bottomCornerAnchor instead.
    private static readonly Dictionary<int, Vector2[]> PipLayouts = new Dictionary<int, Vector2[]>{
        { 1, new[] { new Vector2(0.5f, 0.5f) } },
        { 2, new[] { new Vector2(0.5f, 0f), new Vector2(0.5f, 1f) } },
        { 3, new[] { new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1f) } },
        { 4, new[] { new Vector2(0.25f, 0f), new Vector2(0.75f, 0f), new Vector2(0.25f, 1f), new Vector2(0.75f, 1f) } },
        { 5, new[] { new Vector2(0.25f, 0f), new Vector2(0.75f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0.25f, 1f), new Vector2(0.75f, 1f) } },
        { 6, new[] { new Vector2(0.25f, 0f), new Vector2(0.75f, 0f), new Vector2(0.25f, 0.5f), new Vector2(0.75f, 0.5f), new Vector2(0.25f, 1f), new Vector2(0.75f, 1f) } },
        { 7, new[] { new Vector2(0.25f, 0f), new Vector2(0.75f, 0f), new Vector2(0.25f, 0.5f), new Vector2(0.75f, 0.5f), new Vector2(0.25f, 1f), new Vector2(0.75f, 1f), new Vector2(0.5f, 0.25f) } },
        { 8, new[] { new Vector2(0.25f, 0f), new Vector2(0.75f, 0f), new Vector2(0.25f, 0.5f), new Vector2(0.75f, 0.5f), new Vector2(0.25f, 1f), new Vector2(0.75f, 1f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.75f) } },
        { 9, new[] { new Vector2(0.25f, 0f), new Vector2(0.75f, 0f), new Vector2(0.25f, 0.33f), new Vector2(0.75f, 0.33f), new Vector2(0.5f, 0.5f), new Vector2(0.25f, 0.67f), new Vector2(0.75f, 0.67f), new Vector2(0.25f, 1f), new Vector2(0.75f, 1f) } },
        { 10, new[] { new Vector2(0.25f, 0f), new Vector2(0.75f, 0f), new Vector2(0.25f, 0.33f), new Vector2(0.75f, 0.33f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.8f), new Vector2(0.25f, 0.67f), new Vector2(0.75f, 0.67f), new Vector2(0.25f, 1f), new Vector2(0.75f, 1f) } },
    };

    // Cache: only used in Play Mode, so a given card's texture is only baked once.
    private static readonly Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();

    private const string RIG_NAME = "~CardGenRig";
    private const int GEN_LAYER = 31; // reserved, rarely-used layer for the off-screen rig

    private Color _redSuitColor = new Color(0.678f, 0.024f, 0.059f);

    // NOTE: exclude layer 31 from your gameplay/main camera's Culling Mask so this
    // generator rig never shows up doubled in the actual game view.

    // Runtime-only rendering rig. Fully destroyed and rebuilt on every generate.
    private GameObject _rigRoot;
    private Canvas _canvas;
    private Camera _renderCam;
    private RenderTexture _rt;
    private RectTransform _centerArea;
    private Image _centerFace;
    private TMP_Text _labelTL;
    private TMP_Text _labelBR;
    private Image _iconTL;
    private Image _iconBR;
    private readonly List<GameObject> _pipInstances = new List<GameObject>();

    private float _cardWorldWidth = 1f;
    private float _cardWorldHeight = 1f;

    public string CardKey => $"Card_{(suitIcon != null ? suitIcon.name : "NoSuit")}_{rank}";

    private void OnDestroy() {
        DestroyOrphanRigs();
        if (_rt != null) _rt.Release();
    }

    /// <summary>
    /// Main entry point for gameplay code (e.g. the card View in your MVC layer).
    /// In Play Mode this caches by card identity so the same card is never re-baked.
    /// Outside Play Mode it always regenerates fresh (full rig rebuild included),
    /// since it's meant for live testing.
    /// </summary>
    private Texture2D GetCardTexture() {
        if (Application.isPlaying) {
            if (_textureCache.TryGetValue(CardKey, out var cached) && cached != null)
                return cached;

            var generated = GenerateTexture();
            _textureCache[CardKey] = generated;
            return generated;
        }

        return GenerateTexture();
    }

    /// <summary>Clears the play-mode texture cache. Call this e.g. when reloading a scene or theme.</summary>
    public static void ClearCache() {
        _textureCache.Clear();
    }

    private Texture2D GenerateTexture() {
        ResolveCardCollider();
        RebuildRig();
        UpdateTextureDimensionsFromCollider();
        PositionRigOnCard();
        BuildLayout();
        PositionCorners();
        return BakeToTexture();
    }

    /// <summary>
    /// Sets the card's suit, rank, and optional face art, then generates and applies the texture to targetRenderer.
    /// </summary>
    /// <param name="theSuitIcon"></param>
    /// <param name="theRank"></param>
    /// <param name="TheFaceCardArt"></param> <summary>    /// 
    /// </summary>
    public void GenerateCard(Sprite theSuitIcon, CardRank theRank, Sprite TheFaceCardArt = null, bool blackSuit = false) {
        suitIcon = theSuitIcon;
        rank = theRank;
        faceCardArt = TheFaceCardArt;
        setupData.labelColor = blackSuit ? Color.black : _redSuitColor;
        GenerateAndApplyTexture();
    }

    [ContextMenu("Preview Card On Target Renderer")]
    private void PreviewCard() {
        GenerateAndApplyTexture();
    }

    private void GenerateAndApplyTexture() {
        var tex = GetCardTexture();
        ApplyToRenderer(tex);
        if (deleteRigAfterGeneration) {
            if (_rigRoot) {
                DestroyGO(_rigRoot);
                _rigRoot = null;
            }
            DestroyOrphanRigs();
        }
    }

    private void ApplyToRenderer(Texture2D tex) {
        if (targetRenderer == null) {
            Debug.LogWarning("CardFaceGenerator: no targetRenderer assigned, can't preview.", this);
            return;
        }

        Material[] mats = Application.isPlaying ? targetRenderer.materials : targetRenderer.sharedMaterials;
        if (mats == null || setupData.materialIndex < 0 || setupData.materialIndex >= mats.Length) {
            Debug.LogWarning($"CardFaceGenerator: materialIndex {setupData.materialIndex} is out of range (renderer has {(mats == null ? 0 : mats.Length)} material slot(s)).", this);
            return;
        }

        var mat = mats[setupData.materialIndex];
        if (mat == null) {
            Debug.LogWarning($"CardFaceGenerator: material slot {setupData.materialIndex} on targetRenderer is empty.", this);
            return;
        }

        if (mat.HasProperty(setupData.texturePropertyName)) {
            mat.SetTexture(setupData.texturePropertyName, tex);
        }
        else {
            Debug.LogWarning($"CardFaceGenerator: material '{mat.name}' has no texture property '{setupData.texturePropertyName}'.", this);
        }
    }

    [ContextMenu("Save Texture To Folder...")]
    private void SaveTextureMenu() {
#if UNITY_EDITOR
        string folder = EditorUtility.SaveFolderPanel("Select folder to save card texture", Application.dataPath + "Project/Art/Textures/Cards", "");
        if (string.IsNullOrEmpty(folder)) return;

        var tex = GenerateTexture();
        string fullPath = Path.Combine(folder, CardKey + ".png");
        SaveTextureToFile(tex, fullPath);
        AssetDatabase.Refresh();
#else
        Debug.LogWarning("Saving card textures to disk is only supported in the Unity Editor.");
#endif
    }

    public void SaveTextureToFile(Texture2D tex, string fullPath) {
        byte[] png = tex.EncodeToPNG();
        File.WriteAllBytes(fullPath, png);
        Debug.Log($"CardFaceGenerator: saved '{fullPath}'.");
    }

    private static string RankToLabel(int rankValue) {
        switch (rankValue) {
            case 1: return "A";
            case 11: return "J";
            case 12: return "Q";
            case 13: return "K";
            default: return rankValue.ToString();
        }
    }

    private void DestroyGO(GameObject go) {
        if (go == null) return;
        if (Application.isPlaying) Destroy(go);
        else DestroyImmediate(go);
    }

    // --- Rig lifecycle -----------------------------------------------------

    /// <summary>
    /// Destroys any existing generator rig - including orphaned ones left behind
    /// by a domain reload where the cached reference was lost - so every generate
    /// starts from a clean slate.
    /// </summary>
    private void DestroyOrphanRigs() {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            var child = transform.GetChild(i);
            if (child != null && child.name == RIG_NAME)
                DestroyGO(child.gameObject);
        }

        _rigRoot = null;
        _canvas = null;
        _renderCam = null;
        _centerArea = null;
        _centerFace = null;
        _labelTL = null;
        _labelBR = null;
        _iconTL = null;
        _iconBR = null;
        _pipInstances.Clear();
    }

    private void RebuildRig() {
        DestroyOrphanRigs();
        BuildRig();
    }

    // --- Collider-driven sizing ---------------------------------------------

    private void ResolveCardCollider() {
        if (cardCollider != null) return;

        if (targetRenderer != null)
            cardCollider = targetRenderer.GetComponent<Collider>();

        if (cardCollider == null)
            cardCollider = GetComponent<Collider>();
    }

    /// <summary>Reads the card's real-world width/height directly from its Collider.</summary>
    private void GetCardWorldDimensions(out float worldWidth, out float worldHeight) {
        if (cardCollider == null) {
            // No collider assigned anywhere: fall back to the current width/height
            // ratio so behavior stays sane for quick testing without a physical card.
            worldWidth = 1f;
            worldHeight = setupData.textureWidth > 0 ? (float)setupData.textureHeight / setupData.textureWidth : 1f;
            return;
        }

        if (cardCollider is BoxCollider box) {
            // Using the collider's local size * lossy scale (rather than world AABB)
            // keeps this correct even when the card is rotated in the scene.
            Vector3 scale = box.transform.lossyScale;
            worldWidth = Mathf.Abs(box.size.x * scale.x);
            worldHeight = Mathf.Abs(box.size.y * scale.y);
        }
        else {
            // Fallback for non-box colliders: world-space bounds (accurate as long
            // as the card isn't tilted off its X/Y face).
            worldWidth = cardCollider.bounds.size.x;
            worldHeight = cardCollider.bounds.size.y;
        }

        if (worldWidth <= 0f) worldWidth = 1f;
        if (worldHeight <= 0f) worldHeight = 1f;
    }

    private void UpdateTextureDimensionsFromCollider() {
        GetCardWorldDimensions(out float worldWidth, out float worldHeight);
        _cardWorldWidth = worldWidth;
        _cardWorldHeight = worldHeight;
        setupData.textureHeight = Mathf.Max(2, Mathf.RoundToInt(setupData.textureWidth * (worldHeight / worldWidth)));
    }

    /// <summary>
    /// Moves the generator rig so it sits flush against the card's front face,
    /// scaled so 1 pixel of the canvas equals the correct fraction of the card's
    /// real world size. This is what lets you see the layout directly on top of
    /// the physical card mesh while working in the Scene view.
    /// </summary>
    private void PositionRigOnCard() {
        if (cardCollider == null) return;

        Transform cardT = cardCollider.transform;
        Vector3 worldNormal = cardT.TransformDirection(setupData.frontFaceLocalNormal).normalized;
        Vector3 worldUp = cardT.up;

        Vector3 extents = cardCollider.bounds.extents;
        float offsetDistance = Vector3.Dot(extents, new Vector3(Mathf.Abs(worldNormal.x), Mathf.Abs(worldNormal.y), Mathf.Abs(worldNormal.z)))
                                + setupData.previewSurfaceOffset;

        _rigRoot.transform.position = cardCollider.bounds.center + worldNormal * offsetDistance;
        _rigRoot.transform.rotation = Quaternion.LookRotation(worldNormal, worldUp);

        // The canvas is authored in pixel-space (sizeDelta = textureWidth x textureHeight);
        // scaling it down to the card's real dimensions makes 1 UI pixel map to the
        // correct slice of the physical card, so the rig visually matches the mesh.
        var canvasRect = _canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(setupData.textureWidth, setupData.textureHeight);
        canvasRect.localPosition = Vector3.zero;
        canvasRect.localRotation = Quaternion.identity;
        _canvas.transform.localScale = new Vector3(_cardWorldWidth / setupData.textureWidth, _cardWorldHeight / setupData.textureHeight, 1f);

        // Camera sits a fixed distance in front of the canvas along the rig's local
        // forward axis, framed exactly to the card's real height.
        _renderCam.transform.localPosition = new Vector3(0f, 0f, -Mathf.Max(0.5f, _cardWorldHeight * 2f));
        _renderCam.transform.localRotation = Quaternion.identity;
        _renderCam.orthographic = true;
        _renderCam.orthographicSize = _cardWorldHeight / 2f;
    }

    // --- Layout building -----------------------------------------------------

    private void BuildLayout() {
        int rankValue = (int)rank;
        string label = RankToLabel(rankValue);
        bool isFaceCard = rankValue >= 11 && rankValue <= 13;

        _labelTL.text = label;
        _labelBR.text = label;
        _labelTL.color = setupData.labelColor;
        _labelBR.color = setupData.labelColor;

        if (_iconTL != null) _iconTL.sprite = suitIcon;
        if (_iconBR != null) _iconBR.sprite = suitIcon;

        for (int i = _pipInstances.Count - 1; i >= 0; i--)
            DestroyGO(_pipInstances[i]);
        _pipInstances.Clear();

        _centerFace.gameObject.SetActive(isFaceCard);
        _centerArea.gameObject.SetActive(!isFaceCard);

        if (isFaceCard) {
            _centerFace.sprite = faceCardArt;
        }
        else if (PipLayouts.TryGetValue(rankValue, out var positions)) {
            foreach (var pos in positions) {
                var pipGO = new GameObject("Pip", typeof(RectTransform), typeof(Image));
                pipGO.layer = GEN_LAYER;
                pipGO.transform.SetParent(_centerArea, false);

                var pipRect = pipGO.GetComponent<RectTransform>();
                pipRect.anchorMin = pos;
                pipRect.anchorMax = pos;
                pipRect.pivot = new Vector2(0.5f, 0.5f);
                pipRect.anchoredPosition = Vector2.zero;

                float scale = rankValue == 1 ? setupData.aceIconScale : 1f;
                pipRect.sizeDelta = setupData.pipSize * scale;

                // The core rule: anything in the lower half of the layout is upside down.
                // For 8, 9 and 10 the extra rows fall past y = 0.5 too, so this same
                // line naturally covers "the last two rows are upside down" as well.
                bool rotated = pos.y > 0.5f;
                pipRect.localEulerAngles = rotated ? Vector3.zero : new Vector3(0f, 0f, 180f);

                var img = pipGO.GetComponent<Image>();
                img.sprite = suitIcon;
                img.preserveAspect = true;
                img.raycastTarget = false;

                _pipInstances.Add(pipGO);
            }
        }
    }

    /// <summary>
    /// Places the top and bottom rank label + suit icon using explicit anchor
    /// transforms instead of computed padding. Position comes straight from the
    /// anchor; rotation is the anchor's own rotation plus a fixed twist per corner
    /// (top = -180, bottom = 0).
    /// </summary>
    private void PositionCorners() {
        PositionCornerElement(_labelTL, _iconTL, topCornerAnchor, -180f, true);
        PositionCornerElement(_labelBR, _iconBR, bottomCornerAnchor, 0f, false);
    }

    private void PositionCornerElement(TMP_Text label, Image icon, Transform anchor, float extraRotationZ, bool top) {
        if (anchor == null) return;

        Quaternion finalRotation = anchor.rotation * Quaternion.Euler(0f, 0f, extraRotationZ);

        var labelRect = label.rectTransform;
        labelRect.position = anchor.position;
        labelRect.rotation = finalRotation;

        var iconRect = icon.rectTransform;
        iconRect.position = anchor.TransformPoint(top ? -setupData.iconOffsetFromAnchor : setupData.iconOffsetFromAnchor);
        iconRect.rotation = finalRotation;
    }

    private Texture2D BakeToTexture() {
        if (_rt == null || _rt.width != setupData.textureWidth || _rt.height != setupData.textureHeight) {
            if (_rt != null) _rt.Release();
            _rt = new RenderTexture(setupData.textureWidth, setupData.textureHeight, 16, RenderTextureFormat.ARGB32);
        }

        _renderCam.backgroundColor = setupData.backgroundColor;
        _renderCam.targetTexture = _rt;
        _renderCam.Render();

        var prevActive = RenderTexture.active;
        RenderTexture.active = _rt;

        var tex = new Texture2D(setupData.textureWidth, setupData.textureHeight, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, setupData.textureWidth, setupData.textureHeight), 0, 0);
        tex.Apply();
        tex.name = CardKey;

        RenderTexture.active = prevActive;
        _renderCam.targetTexture = null;

        return tex;
    }

    private void BuildRig() {
        _rigRoot = new GameObject(RIG_NAME);
        _rigRoot.hideFlags = HideFlags.DontSave;
        _rigRoot.layer = GEN_LAYER;
        _rigRoot.transform.SetParent(transform, false);

        var canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.layer = GEN_LAYER;
        canvasGO.transform.SetParent(_rigRoot.transform, false);

        _canvas = canvasGO.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.WorldSpace;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 1f;

        var camGO = new GameObject("RenderCam", typeof(Camera));
        camGO.layer = GEN_LAYER;
        camGO.transform.SetParent(_rigRoot.transform, false);

        _renderCam = camGO.GetComponent<Camera>();
        _renderCam.clearFlags = CameraClearFlags.SolidColor;
        _renderCam.backgroundColor = setupData.backgroundColor;
        _renderCam.cullingMask = 1 << GEN_LAYER;
        _renderCam.orthographic = true;
        _renderCam.nearClipPlane = 0.01f;
        _renderCam.farClipPlane = 10f;

        // Background
        var bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGO.layer = GEN_LAYER;
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        bgGO.GetComponent<Image>().color = setupData.backgroundColor;

        // Center area (pips live here for 1-10)
        var centerGO = new GameObject("CenterArea", typeof(RectTransform));
        centerGO.layer = GEN_LAYER;
        centerGO.transform.SetParent(canvasGO.transform, false);
        _centerArea = centerGO.GetComponent<RectTransform>();
        _centerArea.anchorMin = setupData.centerAreaMargin;
        _centerArea.anchorMax = Vector2.one - setupData.centerAreaMargin;
        _centerArea.offsetMin = Vector2.zero;
        _centerArea.offsetMax = Vector2.zero;

        // Center face art (J/Q/K)
        var faceGO = new GameObject("CenterFace", typeof(RectTransform), typeof(Image));
        faceGO.layer = GEN_LAYER;
        faceGO.transform.SetParent(canvasGO.transform, false);
        _centerFace = faceGO.GetComponent<Image>();
        _centerFace.preserveAspect = true;

        _centerFace.raycastTarget = false;
        var faceRect = faceGO.GetComponent<RectTransform>();
        faceRect.anchorMin = setupData.centerAreaMargin;
        faceRect.anchorMax = Vector2.one - setupData.centerAreaMargin;
        faceRect.offsetMin = Vector2.zero;
        faceRect.offsetMax = Vector2.zero;

        if (rank == CardRank.Jack || rank == CardRank.Queen || rank == CardRank.King) {
            _centerFace.preserveAspect = false;

            faceRect.anchorMin = setupData.centerBigImageAreaMargin;
            faceRect.anchorMax = Vector2.one - setupData.centerBigImageAreaMargin;
            faceRect.offsetMin = Vector2.zero;
            faceRect.offsetMax = Vector2.zero;

            var outline = faceGO.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(5f, -5f);
        }

        // Corners - position/rotation are applied afterwards in PositionCorners(),
        // driven by topCornerAnchor / bottomCornerAnchor.
        _labelTL = CreateCornerLabel(canvasGO.transform, "LabelTopLeft");
        _iconTL = CreateCornerIcon(canvasGO.transform, "IconTopLeft");
        _labelBR = CreateCornerLabel(canvasGO.transform, "LabelBottomRight");
        _iconBR = CreateCornerIcon(canvasGO.transform, "IconBottomRight");
    }

    private TMP_Text CreateCornerLabel(Transform parent, string name) {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.layer = GEN_LAYER;
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200f, 100f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = setupData.cornerFontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = setupData.labelColor;
        tmp.raycastTarget = false;
        tmp.fontStyle = setupData.boldFont ? FontStyles.Bold : FontStyles.Normal;
        if (labelFont) {
            tmp.font = labelFont;
        }
        return tmp;
    }

    private Image CreateCornerIcon(Transform parent, string name) {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.layer = GEN_LAYER;
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = setupData.cornerIconSize;
        rt.pivot = new Vector2(0.5f, 0.5f);

        var img = go.GetComponent<Image>();
        img.preserveAspect = true;
        img.raycastTarget = false;
        return img;
    }
}