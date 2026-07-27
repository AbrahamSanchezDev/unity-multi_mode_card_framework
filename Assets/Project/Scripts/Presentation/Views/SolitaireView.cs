using System;
using System.Collections.Generic;
using UnityEngine;
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
        public event Action<CardData, int> OnTableauDropRequested;
        public event Action<CardData, int> OnFoundationDropRequested;

        [Header("3D Spatial Table Anchors")]
        [SerializeField] private CardsGraphics cardsGraphics;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Transform stockAnchor;
        [SerializeField] private Transform wasteAnchor;
        [SerializeField] private Transform[] foundationAnchors = new Transform[4];
        [SerializeField] private Transform[] tableauAnchors = new Transform[7];

        [Header("Cascade Offsets")]
        [SerializeField] private Vector3 tableauCascadeOffset = new Vector3(0f, 0f, -0.035f); // Downwards cascade on board
        [SerializeField] private float cardThicknessOffset = 0.002f; // Y-height offset to avoid Z-fighting

        private readonly List<GameObject> _spawnedCards = new();

        private void OnEnable() {
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null) return;

            _root = uiDocument.rootVisualElement;
            if (_root == null) return;

            _lblWalletBalance = _root.Q<Label>("lbl-wallet-balance");
            _btnRestart = _root.Q<Button>("btn-restart");
            _btnMenu = _root.Q<Button>("btn-hamburger-menu");
            _btnDraw = _root.Q<Button>("btn-draw");

            if (_btnRestart != null) _btnRestart.clicked += HandleRestartClicked;
            if (_btnMenu != null) _btnMenu.clicked += HandleMenuClicked;
            if (_btnDraw != null) _btnDraw.clicked += HandleDrawClicked;
        }

        private void OnDisable() {
            if (_btnRestart != null) _btnRestart.clicked -= HandleRestartClicked;
            if (_btnMenu != null) _btnMenu.clicked -= HandleMenuClicked;
            if (_btnDraw != null) _btnDraw.clicked -= HandleDrawClicked;
        }

        private void HandleRestartClicked() => OnRestartRequested?.Invoke();
        private void HandleMenuClicked() => OnMenuRequested?.Invoke();
        private void HandleDrawClicked() => OnStockTapped?.Invoke();

        public void RenderLayout(List<CardData>[] tableau, List<CardData>[] foundation, List<CardData> stock, List<CardData> waste) {
            ClearTable();

            // 1. Render Tableau Columns (7 cascades)
            for (int col = 0; col < tableau.Length && col < tableauAnchors.Length; col++) {
                var columnCards = tableau[col];
                Transform anchor = tableauAnchors[col];
                if (anchor == null) continue;

                for (int i = 0; i < columnCards.Count; i++) {
                    CardData card = columnCards[i];
                    Vector3 position = anchor.position + (tableauCascadeOffset * i) + (Vector3.up * (i * cardThicknessOffset));
                    Quaternion rotation = anchor.rotation;

                    // Klondike rule: Only top card of column starts face up
                    bool isFaceUp = (i == columnCards.Count - 1);
                    SpawnCard(card, position, rotation, isFaceUp, anchor);
                }
            }

            // 2. Render Stock Pile Stack
            if (stockAnchor != null) {
                for (int i = 0; i < stock.Count; i++) {
                    Vector3 position = stockAnchor.position + (Vector3.up * (i * cardThicknessOffset));
                    SpawnCard(stock[i], position, stockAnchor.rotation, isFaceUp: false, stockAnchor);
                }
            }

            // 3. Render Waste Pile
            if (wasteAnchor != null) {
                for (int i = 0; i < waste.Count; i++) {
                    Vector3 position = wasteAnchor.position + (Vector3.up * (i * cardThicknessOffset));
                    SpawnCard(waste[i], position, wasteAnchor.rotation, isFaceUp: true, wasteAnchor);
                }
            }

            // 4. Render Foundations
            for (int f = 0; f < foundation.Length && f < foundationAnchors.Length; f++) {
                var fCards = foundation[f];
                Transform anchor = foundationAnchors[f];
                if (anchor == null) continue;

                for (int i = 0; i < fCards.Count; i++) {
                    Vector3 position = anchor.position + (Vector3.up * (i * cardThicknessOffset));
                    SpawnCard(fCards[i], position, anchor.rotation, isFaceUp: true, anchor);
                }
            }
        }

        private GameObject SpawnCard(CardData cardData, Vector3 position, Quaternion rotation, bool isFaceUp, Transform theParent) {
            if (cardPrefab == null) {
                Debug.LogError("[SolitaireView] cardPrefab is not assigned in the Inspector!");
                return null;
            }

            GameObject cardInstance = Instantiate(cardPrefab, position, rotation, theParent);

            // Set face orientation (Rotate 180 on Z or X if face down based on your prefab mesh design)
            if (!isFaceUp) {
                cardInstance.transform.Rotate(180f, 0f, 0, Space.Self);
            }

            // Apply texture graphics if component is present
            var faceGenerator = cardInstance.GetComponent<CardFaceGenerator>();
            if (faceGenerator && isFaceUp) {
                faceGenerator.GenerateCard(cardData, cardsGraphics);
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
        }
    }
}