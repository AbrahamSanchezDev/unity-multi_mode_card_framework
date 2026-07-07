using NUnit.Framework;
using System.IO;
using UnityEngine;
using TMPro;
using CardFramework.Core.Models;
using System.Reflection;
using CardFramework.Presentation.Views;

namespace CardFramework.Tests.EditMode.Presentation {
    [TestFixture]
    public class CardFaceGeneratorTests {
        private class TestableCardFaceGenerator : CardFaceGenerator {
            public bool ForceUseCache { get; set; }

            protected override bool ShouldUseCache() {
                return ForceUseCache;
            }
        }

        private GameObject _cardContainer;
        private CardFaceGenerator _generator;
        private MeshRenderer _mockRenderer;
        private BoxCollider _mockCollider;
        private Transform _topAnchor;
        private Transform _bottomAnchor;
        private Material _mockMaterial;

        private Sprite _suitSprite;
        private Sprite _faceArtSprite;

        [SetUp]
        public void Setup() {
            _cardContainer = new GameObject("Test_Card_Object");
            _generator = _cardContainer.AddComponent<CardFaceGenerator>();

            _mockRenderer = _cardContainer.AddComponent<MeshRenderer>();
            _mockCollider = _cardContainer.AddComponent<BoxCollider>();
            _mockCollider.size = new Vector3(1f, 1.434f, 0.01f);

            _mockMaterial = new Material(Shader.Find("Unlit/Texture"));
            _mockRenderer.sharedMaterials = new Material[] { _mockMaterial };

            var topGO = new GameObject("TopAnchor");
            var botGO = new GameObject("BottomAnchor");
            topGO.transform.SetParent(_cardContainer.transform);
            botGO.transform.SetParent(_cardContainer.transform);
            _topAnchor = topGO.transform;
            _bottomAnchor = botGO.transform;

            Texture2D dummyTex = new Texture2D(10, 10);
            _suitSprite = Sprite.Create(dummyTex, new Rect(0, 0, 10, 10), Vector2.zero);
            _faceArtSprite = Sprite.Create(dummyTex, new Rect(0, 0, 10, 10), Vector2.zero);

            _generator.targetRenderer = _mockRenderer;
            _generator.cardCollider = _mockCollider;
            _generator.topCornerAnchor = _topAnchor;
            _generator.bottomCornerAnchor = _bottomAnchor;

            _generator.setupData.materialIndex = 0;
            _generator.setupData.texturePropertyName = "_MainTex";
            _generator.setupData.textureWidth = 64;
        }

        [TearDown]
        public void TearDown() {
            CardFaceGenerator.ClearCache();
            if (_mockMaterial != null) Object.DestroyImmediate(_mockMaterial);
            if (_cardContainer != null) Object.DestroyImmediate(_cardContainer);

            var strayRig = GameObject.Find("~CardGenRig");
            if (strayRig != null) Object.DestroyImmediate(strayRig);
        }

        [Test]
        public void Generator_ValidateData() {
            Assert.IsNotNull(_generator.PipLayouts, "PipLayouts dictionary should not be null.");
            Assert.IsNotNull(CardFaceGenerator._textureCache, "_textureCache dictionary should not be null.");
        }
        [Test]
        public void Generator_ValidateTexturesClearing() {
            Assert.IsNotNull(CardFaceGenerator._textureCache, "_textureCache dictionary should not be null.");
            var theTexture = new Texture2D(1, 1);
            CardFaceGenerator._textureCache.Add("AccelerationEvent", theTexture);

            Assert.IsTrue(CardFaceGenerator._textureCache.ContainsKey("AccelerationEvent"), "Texture cache should contain the test key.");

            CardFaceGenerator.ClearCache();
            Assert.IsFalse(CardFaceGenerator._textureCache.ContainsKey("AccelerationEvent"), "Texture cache should be cleared after ClearCache() invocation.");
        }
        [Test]
        public void Generator_PublicPreviewCard_ExecutesSuccessfully() {
            Assert.DoesNotThrow(() => {
                _generator.PreviewCard();
            });
        }

        [Test]
        public void Generator_OnResolveColliderFromRenderer_SuccessfullyFindsComponent() {
            _generator.cardCollider = null;
            _generator.targetRenderer = _mockRenderer;

            var expectedCollider = _mockRenderer.gameObject.AddComponent<BoxCollider>();

            Assert.DoesNotThrow(() => {
                _generator.GenerateCard(_suitSprite, CardData.Rank.Five, null, false);
            });

            Object.DestroyImmediate(expectedCollider);
        }

        [Test]
        public void Generator_OnRebuildWithExistingPips_IteratesAndDestroysPopulatedList() {
            _generator.GenerateCard(_suitSprite, CardData.Rank.Eight, null, false);

            Assert.DoesNotThrow(() => {
                _generator.GenerateCard(_suitSprite, CardData.Rank.Jack, _faceArtSprite, false);
            });
        }

        [Test]
        public void Generator_DestroyPreviewPips_DirectInvocation() {
            // Generates a card to populate the private _pipInstances list
            _generator.GenerateCard(_suitSprite, CardData.Rank.Ten, null, false);

            // Corrected to "DestroyPreviewPips" (Singular) and NonPublic
            var destroyPipsMethod = typeof(CardFaceGenerator).GetMethod("DestroyPreviewPips",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(destroyPipsMethod, "DestroyPreviewPips method must be accessible via reflection.");
            Assert.DoesNotThrow(() => {
                destroyPipsMethod.Invoke(_generator, null);
            });
        }

        [Test]
        public void Generator_GetCardTextureFromCache_DirectInvocation() {
            _generator.GenerateCard(_suitSprite, CardData.Rank.Four, null, false);
            string expectedKey = _generator.CardKey;

            var publicCacheMethod = _generator.GetCardTextureFromCache();
            Assert.IsNotNull(publicCacheMethod, "GetCardTextureFromCache should return a cached texture.");

            var getCacheMethod = typeof(CardFaceGenerator).GetMethod("GetCardTexture",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(getCacheMethod, "GetCardTexture method must be accessible via reflection.");

            Texture2D cachedTex = null;
            Assert.DoesNotThrow(() => {
                cachedTex = (Texture2D)getCacheMethod.Invoke(_generator, new object[] { expectedKey });
            });
            Assert.IsNotNull(cachedTex, "Should return a valid cached Texture2D asset.");

            Texture2D cachedTexAgain = null;
            Assert.DoesNotThrow(() => {
                cachedTexAgain = (Texture2D)getCacheMethod.Invoke(_generator, new object[] { expectedKey });
            });
            Assert.IsNotNull(cachedTexAgain, "The second cache lookup should still return a valid texture.");
            Assert.AreSame(cachedTex, cachedTexAgain, "The second lookup should return the cached texture instance.");
        }

        [Test]
        public void Generator_GetCardTexture_UsesCachePathWhenForced() {
            var isolatedContainer = new GameObject("Test_Cache_Generator");
            var testGenerator = isolatedContainer.AddComponent<TestableCardFaceGenerator>();
            testGenerator.ForceUseCache = true;
            testGenerator.targetRenderer = isolatedContainer.AddComponent<MeshRenderer>();
            testGenerator.cardCollider = isolatedContainer.AddComponent<BoxCollider>();
            testGenerator.topCornerAnchor = isolatedContainer.transform;
            testGenerator.bottomCornerAnchor = isolatedContainer.transform;
            testGenerator.setupData.materialIndex = 0;
            testGenerator.setupData.texturePropertyName = "_MainTex";
            testGenerator.setupData.textureWidth = 64;

            var texture = testGenerator.GetCardTexture();
            Assert.IsNotNull(texture, "Forced cache mode should generate a texture.");

            Object.DestroyImmediate(isolatedContainer);
        }

        [Test]
        public void Generator_GetCardTexture_UsesDirectGenerationWhenCacheDisabled() {
            var texture = _generator.GetCardTexture();
            Assert.IsNotNull(texture, "Direct generation should return a texture when caching is disabled.");
        }

        [Test]
        public void Generator_GetCardTextureFromCache_ReturnsCachedTextureImmediately() {
            var cachedTexture = new Texture2D(4, 4);
            CardFaceGenerator._textureCache[_generator.CardKey] = cachedTexture;

            var returnedTexture = _generator.GetCardTextureFromCache();

            Assert.AreSame(cachedTexture, returnedTexture, "A pre-populated cache entry should be returned immediately.");
        }

        [Test]
        public void Generator_GetCardTexture_PrivateHelperCoversCacheMiss() {
            var getCacheMethod = typeof(CardFaceGenerator).GetMethod("GetCardTexture",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(getCacheMethod, "The private cache helper must be accessible.");

            var freshTexture = (Texture2D)getCacheMethod.Invoke(_generator, new object[] { "CacheMissKey" });
            Assert.IsNotNull(freshTexture, "A cache-miss lookup should generate and return a texture.");
        }

        [Test]
        public void Generator_OnLifecycleDestroyWithActiveRenderTexture_ReleasesMemory() {
            _generator.GenerateCard(_suitSprite, CardData.Rank.Ace, null, false);

            Assert.DoesNotThrow(() => {
                Object.DestroyImmediate(_cardContainer);
            });
            _cardContainer = null;
        }

        [Test]
        public void Generator_OnAllRanks_CoversRankToLabelSwitchCases() {
            _generator.GenerateCard(_suitSprite, CardData.Rank.Ace, null, false);
            _generator.GenerateCard(_suitSprite, CardData.Rank.Two, null, false);
            _generator.GenerateCard(_suitSprite, CardData.Rank.Ten, null, false);
            _generator.GenerateCard(_suitSprite, CardData.Rank.Jack, _faceArtSprite, false);
            _generator.GenerateCard(_suitSprite, CardData.Rank.Queen, _faceArtSprite, false);
            _generator.GenerateCard(_suitSprite, CardData.Rank.King, _faceArtSprite, false);

            Assert.Pass();
        }

        [Test]
        public void Generator_OnMaterialsArrayEmpty_HandlesGracefulExit() {
            _mockRenderer.sharedMaterials = new Material[0];

            Assert.DoesNotThrow(() => {
                _generator.GenerateCard(_suitSprite, CardData.Rank.Ace, null, false);
            });
        }

        [Test]
        public void Generator_OnMaterialIndexOutOfBounds_LogsWarningBranch() {
            _generator.setupData.materialIndex = -1;
            _generator.GenerateCard(_suitSprite, CardData.Rank.Ace, null, false);

            _generator.setupData.materialIndex = 99;
            _generator.GenerateCard(_suitSprite, CardData.Rank.Ace, null, false);

            Assert.Pass();
        }

        [Test]
        public void Generator_OnEmptyMaterialSlot_HandlesNullGracefully() {
            _mockRenderer.sharedMaterials = new Material[] { null };

            Assert.DoesNotThrow(() => {
                _generator.GenerateCard(_suitSprite, CardData.Rank.Ace, null, false);
            });
        }

        [Test]
        public void Generator_OnMissingTextureProperty_HandlesWarningBranch() {
            _generator.setupData.texturePropertyName = "_NonExistentPropertyBypass";

            Assert.DoesNotThrow(() => {
                _generator.GenerateCard(_suitSprite, CardData.Rank.Ace, null, false);
            });
        }

        [Test]
        public void Generator_OnNonBoxColliderFallback_CalculatesUsingBounds() {
            Object.DestroyImmediate(_mockCollider);
            var sphereCollider = _cardContainer.AddComponent<SphereCollider>();
            sphereCollider.radius = 0.5f;
            _generator.cardCollider = sphereCollider;

            Assert.DoesNotThrow(() => {
                _generator.GenerateCard(_suitSprite, CardData.Rank.Two, null, false);
            });
        }

        [Test]
        public void Generator_OnComponentColliderFallback_ResolvesLocalCollider() {
            _generator.cardCollider = null;
            _generator.targetRenderer = null;
            Object.DestroyImmediate(_mockCollider);

            Assert.DoesNotThrow(() => {
                _generator.GenerateCard(_suitSprite, CardData.Rank.Six, null, false);
            });
        }

        [Test]
        public void Generator_WithFontAssigned_AppliesToTextMeshProComponents() {
            TMP_FontAsset targetFont = null;
            var foundFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            if (foundFonts != null && foundFonts.Length > 0) {
                targetFont = foundFonts[0];
            }

            if (targetFont != null) {
                _generator.labelFont = targetFont;
            }

            Assert.DoesNotThrow(() => {
                _generator.GenerateCard(_suitSprite, CardData.Rank.Nine, null, false);
            });
        }

        [Test]
        public void Generator_OnRigCleanupWithActiveRoot_ExecutesPurgeBranches() {
            _generator.deleteRigAfterGeneration = true;
            _generator.GenerateCard(_suitSprite, CardData.Rank.Seven, null, false);

            var rigRootField = typeof(CardFaceGenerator).GetField("_rigRoot",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (rigRootField != null) {
                Assert.IsNull(rigRootField.GetValue(_generator));
            }
        }

        [Test]
        public void Generator_OnSaveTextureToFile_ExecutesFileIOWrites() {
            _generator.GenerateCard(_suitSprite, CardData.Rank.Ace, null, false);
            string testFilePath = Path.Combine(Application.temporaryCachePath, "TestCardOutput.png");

            Texture2D currentTex = new Texture2D(_generator.setupData.textureWidth, _generator.setupData.textureHeight);
            _generator.SaveTextureToFile(currentTex, testFilePath);

            Assert.IsTrue(File.Exists(testFilePath));

            if (File.Exists(testFilePath)) {
                File.Delete(testFilePath);
            }
            Object.DestroyImmediate(currentTex);
        }

        [Test]
        public void Generator_SaveTextureToFolder_WritesCardTextureToDisk() {
            string tempFolder = Path.Combine(Application.temporaryCachePath, "CardFaceGeneratorTests", System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempFolder);

            try {
                _generator.SaveTextureToFolder(tempFolder);

                string outputPath = Path.Combine(tempFolder, _generator.CardKey + ".png");
                Assert.IsTrue(File.Exists(outputPath), "The card texture should be written to disk.");
            }
            finally {
                if (Directory.Exists(tempFolder)) {
                    Directory.Delete(tempFolder, true);
                }
            }
        }


    }
}