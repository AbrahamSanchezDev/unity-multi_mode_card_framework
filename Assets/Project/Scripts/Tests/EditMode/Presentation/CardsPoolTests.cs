using NUnit.Framework;
using UnityEngine;
using CardFramework.Presentation.Views;

namespace CardFramework.Tests.EditMode.Presentation {
    [TestFixture]
    public class CardsPoolTests {
        private GameObject _root;
        private GameObject _prefab;

        [SetUp]
        public void Setup() {
            _root = new GameObject("CardsPoolRoot");
            _prefab = new GameObject("CardPrefab");
            _prefab.AddComponent<BoxCollider>();
        }

        [TearDown]
        public void TearDown() {
            if (_prefab != null) Object.DestroyImmediate(_prefab);
            if (_root != null) Object.DestroyImmediate(_root);
        }

        [Test]
        public void Pool_ReusesReturnedCardInstances() {
            var pool = new CardsPool(_prefab, _root.transform);
            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;

            GameObject first = pool.GetCard(position, rotation, _root.transform);
            Assert.IsNotNull(first, "The pool should return a card instance.");
            Assert.IsTrue(first.activeSelf, "A newly retrieved card should be active.");

            pool.ReturnCard(first);
            Assert.IsFalse(first.activeSelf, "A returned card should be deactivated.");

            GameObject second = pool.GetCard(position, rotation, _root.transform);
            Assert.AreSame(first, second, "The pool should reuse the returned card instance.");
            Assert.IsTrue(second.activeSelf, "A reused card should be activated again.");
        }
    }
}
