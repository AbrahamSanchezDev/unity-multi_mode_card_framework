using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace CardFramework.Presentation.Controllers {

    public class GameTableManager : MonoBehaviour {
        [Serializable]
        public struct TableConfiguration {
            public string GameName;                  // Name matching the UI carousel parameter ("Blackjack", "Solitaire", etc.)
            public Transform PlayerSpawnPoint;       // Position and rotation where the player/VR rig will move
            public GameObject TableVisualsRoot;      // Physical table assets, chairs, lights to toggle

            public GameObject[] VisualObjsForGame;

            public UIDocument TableUiDocument;        // Optional VR world-space UI linked to this specific table
            public bool AllowZoom;                   // Enables scroll-based zoom for this table
            [Range(0f, 1f)] 
            public float MaxZoomOutDistance;         // Optional per-table zoom-out limit from the spawn point
        }

        [Header("Setup Configurations")]
        [SerializeField] private List<TableConfiguration> tables;
        [SerializeField] private Transform playerTransform; // Reference to the VR Rig or Main Camera assembly
        [SerializeField] private InputActionReference zoomInputReference; // Optional input action for scroll wheel zoom
        [SerializeField] private float zoomSpeed = 0.25f;
        [SerializeField] private float defaultMaxZoomOutDistance = 4f;

        private string _activeGameName = "Blackjack"; // Default starting table
        private Vector3 _zoomOffsetFromSpawn = Vector3.zero;
        private TableConfiguration? _activeTable;

        private void Start() {
            // Initialize the default starting view configuration
            //SwitchTable(_activeGameName);
        }

        private void Update() {
            if (_activeTable == null || playerTransform == null) {
                return;
            }

            var activeTable = _activeTable.Value;
            if (!activeTable.AllowZoom || activeTable.PlayerSpawnPoint == null) {
                return;
            }

            float scrollDelta = ReadZoomScrollDelta();
            if (Mathf.Abs(scrollDelta) < 0.0001f) {
                return;
            }

            float maxZoomOutDistance = activeTable.MaxZoomOutDistance > 0f
                ? activeTable.MaxZoomOutDistance
                : defaultMaxZoomOutDistance;

            float currentDistance = _zoomOffsetFromSpawn.magnitude;
            float nextDistance = currentDistance - (scrollDelta * zoomSpeed);
            nextDistance = Mathf.Clamp(nextDistance, 0f, maxZoomOutDistance);

            if (Mathf.Abs(nextDistance - currentDistance) < 0.0001f) {
                return;
            }

            Vector3 zoomDirection = -playerTransform.forward;
            _zoomOffsetFromSpawn = zoomDirection.normalized * nextDistance;
            playerTransform.position = activeTable.PlayerSpawnPoint.position + _zoomOffsetFromSpawn;
        }

        /// <summary>
        /// Task-4.3.1: Moves the player context to the designated table coordinates 
        /// and toggles table-specific spatial environments seamlessly.
        /// </summary>
        public void SwitchTable(string gameName) {
            if (string.IsNullOrEmpty(gameName)) return;

            TableConfiguration? targetTable = null;

            // Search for the matching structural configuration loop
            foreach (var table in tables) {
                if (table.GameName.Equals(gameName, StringComparison.OrdinalIgnoreCase)) {
                    targetTable = table;
                    break;
                }
            }

            if (targetTable == null) {
                Debug.LogWarning($"[TableManager] Requested table target layout '{gameName}' not found in configuration maps.");
                return;
            }

            _activeGameName = gameName;
            _activeTable = targetTable;
            _zoomOffsetFromSpawn = Vector3.zero;

            // Execute actual physical spatial movements
            if (playerTransform != null && targetTable.Value.PlayerSpawnPoint != null) {
                playerTransform.position = targetTable.Value.PlayerSpawnPoint.position;
                playerTransform.rotation = targetTable.Value.PlayerSpawnPoint.rotation;
            }

            // Optimize runtime overhead by enabling only active visual blocks
            foreach (var table in tables) {
                bool isActiveTarget = table.GameName.Equals(_activeGameName, StringComparison.OrdinalIgnoreCase);

                if (table.TableVisualsRoot != null) {
                    table.TableVisualsRoot.SetActive(isActiveTarget);
                }
                if (table.VisualObjsForGame != null) {
                    for (int i = 0; i < table.VisualObjsForGame.Length; i++) {
                        table.VisualObjsForGame[i].SetActive(isActiveTarget);
                    }
                }

                if (table.TableUiDocument != null) {
                    // table.TableUiDocument.enabled = isActiveTarget;
                    var root = table.TableUiDocument.rootVisualElement;
                    if (root != null) {
                        root.style.display = isActiveTarget ? DisplayStyle.Flex : DisplayStyle.None;
                    }
                }
            }
        }

        private float ReadZoomScrollDelta() {
            if (zoomInputReference != null && zoomInputReference.action != null) {
                return zoomInputReference.action.ReadValue<Vector2>().y;
            }

            return UnityEngine.Input.GetAxis("Mouse ScrollWheel");
        }
    }
}