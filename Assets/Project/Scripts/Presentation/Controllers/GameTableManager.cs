using System;
using System.Collections.Generic;
using UnityEngine;
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
        }

        [Header("Setup Configurations")]
        [SerializeField] private List<TableConfiguration> tables;
        [SerializeField] private Transform playerTransform; // Reference to the VR Rig or Main Camera assembly

        private string _activeGameName = "Blackjack"; // Default starting table

        private void Start() {
            // Initialize the default starting view configuration
            SwitchTable(_activeGameName);
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
            Debug.Log($"[TableManager] Shifting game room environment focus to: {_activeGameName}");

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
                        Debug.Log($"[TableManager] UI Document for '{table.GameName}' set to {(isActiveTarget ? "Visible" : "Hidden")}.");
                    }
                }
            }
        }
    }
}