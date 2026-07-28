// File: Assets/_Project/Scripts/Presentation/Views/SolitaireView.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using CardFramework.Presentation.Interfaces;
using CardFramework.Core.Models;

namespace CardFramework.Presentation.Views {
    [RequireComponent(typeof(UIDocument))]
    public class SolitaireView : MonoBehaviour, ISolitaireView {

        private VisualElement _root;
        private Label _lblWalletBalance;
        private Button _btnRestart;
        private Button _btnMenu;
        private Button _btnDraw;

        public event Action OnStockTapped;
        public event Action OnRestartRequested;
        public event Action OnMenuRequested;
        public event Action<List<CardData>, int, int, int> OnTableauDropRequested;
        public event Action<List<CardData>, int> OnFoundationDropRequested;

        [Header("Input System Binding")]
        [SerializeField] private InputActionReference pointerPositionAction;
        [SerializeField] private InputActionReference pointerPressAction;

        [Header("3D Spatial Table Anchors")]
        [SerializeField] private CardsGraphics cardsGraphics;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Transform stockAnchor;
        [SerializeField] private Transform wasteAnchor;
        [SerializeField] private Transform[] foundationAnchors = new Transform[4];
        [SerializeField] private Transform[] tableauAnchors = new Transform[7];

        [Header("Cascade Offsets")]
        [SerializeField] private Vector3 tableauCascadeOffset = new Vector3(0f, 0f, -0.035f);
        [SerializeField] private float cardThicknessOffset = 0.002f;
        [SerializeField] private float dropDetectionRadius = 0.015f;

        private readonly List<GameObject> _spawnedCards = new();
        private readonly List<SpatialCardInteractable> _spawnedInteractables = new();
        private Camera _mainCamera;
        private bool _isDragging;
        private Plane _dragPlane;
        private Vector3 _dragStartWorldPosition;
        private Vector3 _currentDragWorldPosition;
        private List<SpatialCardInteractable> _draggedStack = new();
        private List<Vector3> _draggedOriginalPositions = new();
        private int _dragSourceColumn = -1;
        private int _dragStartIndex = -1;

        private void OnEnable() {
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument != null) {
                uiDocument.enabled = true;
                _root = uiDocument.rootVisualElement;
                if (_root != null) {
                    _lblWalletBalance = _root.Q<Label>("lbl-wallet-balance");
                    _btnRestart = _root.Q<Button>("btn-restart");
                    _btnMenu = _root.Q<Button>("btn-hamburger-menu");
                    _btnDraw = _root.Q<Button>("btn-draw");

                    if (_btnRestart != null) _btnRestart.clicked += HandleRestartClicked;
                    if (_btnMenu != null) _btnMenu.clicked += HandleMenuClicked;
                    if (_btnDraw != null) _btnDraw.clicked += HandleDrawClicked;
                }
            }

            _mainCamera = Camera.main;

            if (pointerPositionAction?.action != null) pointerPositionAction.action.Enable();
            if (pointerPressAction?.action != null) {
                pointerPressAction.action.Enable();
                pointerPressAction.action.started += HandlePointerPressed;
                pointerPressAction.action.canceled += HandlePointerReleased;
            }
        }

        private void OnDisable() {
            if (_btnRestart != null) _btnRestart.clicked -= HandleRestartClicked;
            if (_btnMenu != null) _btnMenu.clicked -= HandleMenuClicked;
            if (_btnDraw != null) _btnDraw.clicked -= HandleDrawClicked;

            if (pointerPositionAction?.action != null) pointerPositionAction.action.Disable();
            if (pointerPressAction?.action != null) {
                pointerPressAction.action.started -= HandlePointerPressed;
                pointerPressAction.action.canceled -= HandlePointerReleased;
                pointerPressAction.action.Disable();
            }
        }

        private void HandleRestartClicked() => OnRestartRequested?.Invoke();
        private void HandleMenuClicked() => OnMenuRequested?.Invoke();
        private void HandleDrawClicked() => OnStockTapped?.Invoke();

        private void Update() {
            if (!_isDragging || _mainCamera == null) return;

            Vector2 currentPointerPosition = GetPointerPosition();
            Ray ray = _mainCamera.ScreenPointToRay(currentPointerPosition);
            if (_dragPlane.Raycast(ray, out float enter)) {
                _currentDragWorldPosition = ray.GetPoint(enter);
                Vector3 dragDelta = _currentDragWorldPosition - _dragStartWorldPosition;

                for (int i = 0; i < _draggedStack.Count; i++) {
                    _draggedStack[i].SetPosition(_draggedOriginalPositions[i] + dragDelta + Vector3.up * 0.01f);
                }
            }
        }

        private Vector2 GetPointerPosition() {
            if (pointerPositionAction?.action != null && pointerPositionAction.action.enabled) {
                return pointerPositionAction.action.ReadValue<Vector2>();
            }

            if (Pointer.current != null) {
                return Pointer.current.position.ReadValue();
            }

            return Vector2.zero;
        }

        private void HandlePointerPressed(InputAction.CallbackContext context) {
            if (_isDragging || _mainCamera == null) return;
            _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            Vector2 screenPos = GetPointerPosition();
            Ray ray = _mainCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit)) {
                var interactable = hit.collider.GetComponent<SpatialCardInteractable>() ?? hit.collider.GetComponentInParent<SpatialCardInteractable>();
                if (interactable != null) {
                    if (interactable.IsFromWastePile) {
                        StartDragging(interactable);
                        Debug.Log($"[SolitaireView] Started dragging card from waste pile: {interactable.CardData.CardSuit} {interactable.CardData.CardRank}");
                    }
                    else if (interactable.CardData.IsFaceUp && (interactable.SourceColumnIndex >= 0))
                        StartDragging(interactable);
                    else {
                        Debug.Log("[SolitaireView] Pointer pressed on a card, but it is not face-up or not from a valid source.");
                    }
                }
            }
        }

        private void HandlePointerReleased(InputAction.CallbackContext context) {
            if (_isDragging) {
                EndDrag();
            }
        }

        private void StartDragging(SpatialCardInteractable interactable) {
            _draggedStack.Clear();
            _draggedOriginalPositions.Clear();
            _dragSourceColumn = interactable.SourceColumnIndex;
            _dragStartIndex = interactable.CardIndexInColumn;

            if (interactable.IsFromWastePile) {
                _draggedStack.Add(interactable);
            }
            else {
                var run = new List<SpatialCardInteractable>();
                var current = interactable;
                int currentIndex = interactable.CardIndexInColumn;

                while (current != null) {
                    run.Add(current);

                    int nextIndex = currentIndex + 1;
                    SpatialCardInteractable next = null;
                    for (int i = 0; i < _spawnedInteractables.Count; i++) {
                        var candidate = _spawnedInteractables[i];
                        if (candidate == null) continue;
                        if (candidate.SourceColumnIndex != _dragSourceColumn) continue;
                        if (candidate.CardIndexInColumn != nextIndex) continue;
                        if (!candidate.CardData.IsFaceUp) continue;
                        if (!IsValidSequence(current.CardData, candidate.CardData)) continue;

                        next = candidate;
                        break;
                    }

                    if (next == null) {
                        break;
                    }

                    current = next;
                    currentIndex = nextIndex;
                }

                _draggedStack = run;
                if (_draggedStack.Count > 0) {
                    _dragStartIndex = _draggedStack[0].CardIndexInColumn;
                }
            }

            if (_draggedStack.Count == 0) return;

            foreach (var card in _draggedStack) {
                _draggedOriginalPositions.Add(card.transform.position);
                card.SetColliderEnabled(false);
            }

            Vector2 pointerPosition = GetPointerPosition();
            Ray ray = _mainCamera.ScreenPointToRay(pointerPosition);
            _dragPlane = new Plane(Vector3.up, _draggedStack[0].transform.position);
            if (_dragPlane.Raycast(ray, out float enter)) {
                _dragStartWorldPosition = ray.GetPoint(enter);
                _currentDragWorldPosition = _dragStartWorldPosition;
            }

            _isDragging = true;
        }

        private bool IsValidSequence(CardData lowerCard, CardData upperCard) {
            bool differentColor = GetCardColor(lowerCard.CardSuit) != GetCardColor(upperCard.CardSuit);
            bool descendingRank = lowerCard.CardRank == upperCard.CardRank + 1;
            return differentColor && descendingRank;
        }

        private int GetCardColor(CardData.Suit suit) {
            return suit == CardData.Suit.Diamonds || suit == CardData.Suit.Hearts ? 0 : 1;
        }

        private void EndDrag() {
            if (!_isDragging) return;
            _isDragging = false;

            bool handled = false;
            int foundationTargetIndex = GetFoundationDropTargetIndex();
            if (foundationTargetIndex >= 0) {
                var draggedCards = new List<CardData>();
                for (int i = 0; i < _draggedStack.Count; i++) {
                    draggedCards.Add(_draggedStack[i].CardData);
                }

                OnFoundationDropRequested?.Invoke(draggedCards, foundationTargetIndex);
                handled = true;
            }

            if (!handled) {
                int targetTableauColumn = GetTableauDropTargetColumn();
                if (targetTableauColumn >= 0) {
                    var draggedCards = new List<CardData>();
                    for (int i = 0; i < _draggedStack.Count; i++) {
                        draggedCards.Add(_draggedStack[i].CardData);
                    }

                    OnTableauDropRequested?.Invoke(draggedCards, _dragSourceColumn, _dragStartIndex, targetTableauColumn);
                    handled = true;
                }
            }

            if (!handled) {
                for (int i = 0; i < _draggedStack.Count; i++) {
                    _draggedStack[i].ResetToOriginalPosition();
                }
            }

            foreach (var card in _draggedStack) {
                card.SetColliderEnabled(true);
            }

            _draggedStack.Clear();
            _draggedOriginalPositions.Clear();
            _dragSourceColumn = -1;
            _dragStartIndex = -1;
        }

        private int GetFoundationDropTargetIndex() {
            if (_mainCamera == null) return -1;

            Vector2 screenPos = GetPointerPosition();
            Ray ray = _mainCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit)) {
                var target = hit.collider.GetComponent<FoundationDropTarget>() ?? hit.collider.GetComponentInParent<FoundationDropTarget>();
                if (target != null) {
                    return target.FoundationIndex;
                }
            }

            for (int f = 0; f < foundationAnchors.Length; f++) {
                if (foundationAnchors[f] != null) {
                    var target = foundationAnchors[f].GetComponent<FoundationDropTarget>();
                    if (target != null && Vector3.Distance(_currentDragWorldPosition, foundationAnchors[f].position) <= dropDetectionRadius) {
                        return target.FoundationIndex;
                    }
                }
            }

            return -1;
        }

        private int GetTableauDropTargetColumn() {
            if (_mainCamera == null) return -1;

            Vector2 screenPos = GetPointerPosition();
            Ray ray = _mainCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit)) {
                var interactable = hit.collider.GetComponent<SpatialCardInteractable>() ?? hit.collider.GetComponentInParent<SpatialCardInteractable>();
                if (interactable != null && interactable.SourceColumnIndex >= 0) {
                    return interactable.SourceColumnIndex;
                }
            }

            for (int col = 0; col < tableauAnchors.Length; col++) {
                if (tableauAnchors[col] != null && Vector3.Distance(_currentDragWorldPosition, tableauAnchors[col].position) <= dropDetectionRadius) {
                    return col;
                }
            }

            return -1;
        }

        public void RenderLayout(List<CardData>[] tableau, List<CardData>[] foundation, List<CardData> stock, List<CardData> waste) {
            ClearTable();

            // 1. Render Tableau Columns
            for (int col = 0; col < tableau.Length && col < tableauAnchors.Length; col++) {
                var columnCards = tableau[col];
                Transform anchor = tableauAnchors[col];
                if (anchor == null) continue;

                for (int i = 0; i < columnCards.Count; i++) {
                    CardData card = columnCards[i];
                    Vector3 position = anchor.position + (tableauCascadeOffset * i) + (Vector3.up * (i * cardThicknessOffset));
                    Quaternion rotation = anchor.rotation;

                    bool isFaceUp = card.IsFaceUp;
                    SpawnCard(card, position, rotation, isFaceUp, anchor, canInteract: isFaceUp, sourceColumnIndex: col, cardIndexInColumn: i, isFromWastePile: false);
                }
            }

            // 2. Render Stock Pile Stack
            if (stockAnchor != null) {
                for (int i = 0; i < stock.Count; i++) {
                    Vector3 position = stockAnchor.position + (Vector3.up * (i * cardThicknessOffset));
                    SpawnCard(stock[i], position, stockAnchor.rotation, isFaceUp: false, stockAnchor, canInteract: false, sourceColumnIndex: -1, cardIndexInColumn: -1, isFromWastePile: false);
                }
            }

            // 3. Render Waste Pile
            if (wasteAnchor != null) {
                for (int i = 0; i < waste.Count; i++) {
                    Vector3 position = wasteAnchor.position + (Vector3.up * (i * cardThicknessOffset));
                    bool isTopCard = (i == waste.Count - 1);
                    SpawnCard(waste[i], position, wasteAnchor.rotation, isFaceUp: true, wasteAnchor, canInteract: isTopCard, sourceColumnIndex: -1, cardIndexInColumn: -1, isFromWastePile: isTopCard);
                }
            }

            // 4. Render Foundations
            for (int f = 0; f < foundation.Length && f < foundationAnchors.Length; f++) {
                var fCards = foundation[f];
                Transform anchor = foundationAnchors[f];
                if (anchor == null) continue;

                if (fCards.Count > 0) {
                    CardData topCard = fCards[^1];
                    Vector3 position = anchor.position;
                    SpawnCard(topCard, position, anchor.rotation, isFaceUp: topCard.IsFaceUp, anchor, canInteract: false, sourceColumnIndex: -1, cardIndexInColumn: -1, isFromWastePile: false);
                }
            }
        }

        private GameObject SpawnCard(CardData cardData, Vector3 position, Quaternion rotation, bool isFaceUp, Transform theParent, bool canInteract, int sourceColumnIndex, int cardIndexInColumn, bool isFromWastePile) {
            if (cardPrefab == null) {
                Debug.LogError("[SolitaireView] cardPrefab is not assigned in the Inspector!");
                return null;
            }

            GameObject cardInstance = Instantiate(cardPrefab, position, rotation, theParent);

            if (!isFaceUp) {
                cardInstance.transform.Rotate(180f, 0f, 0, Space.Self);
            }

            var faceGenerator = cardInstance.GetComponent<CardFaceGenerator>();
            if (faceGenerator && isFaceUp) {
                faceGenerator.GenerateCard(cardData, cardsGraphics);
            }

            // Add interactable logic only for movable face-up cards
            if (canInteract) {
                var interactable = cardInstance.GetComponent<SpatialCardInteractable>();
                interactable.enabled = true;
                interactable.Initialize(cardData, sourceColumnIndex, cardIndexInColumn, isFromWastePile);
                _spawnedInteractables.Add(interactable);
            }
            else {
                var collider = cardInstance.GetComponent<Collider>();
                if (faceGenerator && collider == null) {
                    collider = faceGenerator.cardCollider;
                }
                if (collider != null && !isFaceUp) collider.enabled = false;
            }

            _spawnedCards.Add(cardInstance);
            return cardInstance;
        }

        public void UpdateWalletBalance(int balance) {
            if (_lblWalletBalance != null)
                _lblWalletBalance.text = $"Balance: {balance} GD";
        }

        public void SetInteractionState(bool canInteract) {
            if (_btnRestart != null) _btnRestart.SetEnabled(canInteract);
        }

        public void ClearTable() {
            foreach (var cardObj in _spawnedCards) {
                if (cardObj != null) Destroy(cardObj);
            }
            _spawnedCards.Clear();
            _spawnedInteractables.Clear();
        }
    }
}