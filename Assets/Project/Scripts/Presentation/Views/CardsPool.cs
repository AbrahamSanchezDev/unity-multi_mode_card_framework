using System.Collections.Generic;
using UnityEngine;

namespace CardFramework.Presentation.Views {
    public class CardsPool {
        private readonly GameObject _prefab;
        private readonly Transform _poolRoot;
        private readonly Stack<GameObject> _availableCards = new Stack<GameObject>();
        private readonly HashSet<GameObject> _activeCards = new HashSet<GameObject>();

        public CardsPool(GameObject prefab, Transform poolRoot) {
            _prefab = prefab;
            _poolRoot = poolRoot;
        }

        public GameObject GetCard(Vector3 position, Quaternion rotation, Transform parent) {
            if (_prefab == null) {
                Debug.LogError("[CardsPool] No prefab assigned.");
                return null;
            }

            GameObject cardInstance = _availableCards.Count > 0 ? _availableCards.Pop() : Object.Instantiate(_prefab, position, rotation, parent);
            if (cardInstance == null) {
                return null;
            }

            cardInstance.SetActive(true);
            cardInstance.transform.SetParent(parent, false);
            cardInstance.transform.SetPositionAndRotation(position, rotation);
            _activeCards.Add(cardInstance);
            return cardInstance;
        }

        public void ReturnCard(GameObject cardInstance) {
            if (cardInstance == null || !_activeCards.Remove(cardInstance)) {
                return;
            }

            cardInstance.SetActive(false);
            if (_poolRoot != null) {
                cardInstance.transform.SetParent(_poolRoot, false);
                cardInstance.transform.localPosition = Vector3.zero;
                cardInstance.transform.localRotation = Quaternion.identity;
                cardInstance.transform.localScale = Vector3.one;
            }

            _availableCards.Push(cardInstance);
        }
    }
}
