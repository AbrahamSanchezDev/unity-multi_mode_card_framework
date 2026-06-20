// Template: Advanced Unit Test Class
// Use this template for complex classes with dependencies, mocking, and state management

using NUnit.Framework;
using NSubstitute;
using UnityEngine;
using YourNamespace; // Replace with your namespace

namespace Tests
{
    /// <summary>
    /// Advanced tests for [ComplexClassName] using mocks, state verification, and dependency injection.
    /// 
    /// Coverage target: 85%+ statements, 80%+ branches, 95%+ methods
    /// 
    /// Test scenarios:
    /// 1. Interaction testing (method calls on dependencies)
    /// 2. State management (before/after state)
    /// 3. Mocked external dependencies
    /// 4. Exception handling and recovery
    /// </summary>
    [TestFixture]
    public class ComplexClassNameTests
    {
        // ────────────────────────────────────────────────────────
        // Dependencies (Mocked)
        // ────────────────────────────────────────────────────────
        
        private IRepository mockRepository;
        private ILogger mockLogger;
        private IEventPublisher mockPublisher;
        
        // ────────────────────────────────────────────────────────
        // System Under Test
        // ────────────────────────────────────────────────────────
        
        private ComplexClassName sut;
        
        // ────────────────────────────────────────────────────────
        // Setup & Teardown
        // ────────────────────────────────────────────────────────
        
        [SetUp]
        public void Setup()
        {
            // Create substitutes (mocks) for all dependencies
            mockRepository = Substitute.For<IRepository>();
            mockLogger = Substitute.For<ILogger>();
            mockPublisher = Substitute.For<IEventPublisher>();
            
            // Configure default mock behavior
            mockRepository.GetById(Arg.Any<int>()).Returns((ItemData)null);
            mockLogger.Log(Arg.Any<string>()).Returns(true);
            
            // Create SUT with mocked dependencies
            sut = new ComplexClassName(mockRepository, mockLogger, mockPublisher);
        }
        
        [TearDown]
        public void Teardown()
        {
            sut = null;
            mockRepository = null;
            mockLogger = null;
            mockPublisher = null;
        }
        
        // ────────────────────────────────────────────────────────
        // Test: Dependency Interaction - Happy Path
        // ────────────────────────────────────────────────────────
        
        [Test]
        [Category("Interaction")]
        public void LoadItem_WithValidId_FetchesFromRepository()
        {
            // Arrange
            int itemId = 42;
            var itemData = new ItemData { Id = itemId, Name = "TestItem" };
            mockRepository.GetById(itemId).Returns(itemData);
            
            // Act
            var result = sut.LoadItem(itemId);
            
            // Assert - Check result
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("TestItem"));
            
            // Assert - Verify repository was called
            mockRepository.Received(1).GetById(itemId);
        }
        
        [Test]
        [Category("Interaction")]
        public void SaveItem_CallsRepositoryAndPublishesEvent()
        {
            // Arrange
            var item = new ItemData { Id = 1, Name = "NewItem" };
            
            // Act
            sut.SaveItem(item);
            
            // Assert - Verify repository was called with correct item
            mockRepository.Received(1).Save(Arg.Is<ItemData>(x => x.Id == item.Id && x.Name == item.Name));
            
            // Assert - Verify event was published
            mockPublisher.Received(1).Publish(Arg.Any<ItemSavedEvent>());
        }
        
        // ────────────────────────────────────────────────────────
        // Test: Dependency Interaction - Error Cases
        // ────────────────────────────────────────────────────────
        
        [Test]
        [Category("Error Handling")]
        public void LoadItem_WhenRepositoryThrows_LogsError()
        {
            // Arrange
            int itemId = 99;
            mockRepository.GetById(itemId).Returns(x => { throw new RepositoryException("Not found"); });
            
            // Act & Assert
            Assert.Throws<RepositoryException>(() => sut.LoadItem(itemId));
            
            // Verify error was logged
            mockLogger.Received(1).LogError(Arg.Any<string>());
        }
        
        [Test]
        [Category("Error Handling")]
        public void SaveItem_WhenRepositoryFails_DoesNotPublishEvent()
        {
            // Arrange
            var item = new ItemData { Id = 1, Name = "Item" };
            mockRepository.Save(item).Returns(x => { throw new RepositoryException("Save failed"); });
            
            // Act & Assert
            Assert.Throws<RepositoryException>(() => sut.SaveItem(item));
            
            // Verify event was NOT published (critical for consistency)
            mockPublisher.DidNotReceive().Publish(Arg.Any<ItemSavedEvent>());
        }
        
        // ────────────────────────────────────────────────────────
        // Test: Dependency Interaction - Argument Verification
        // ────────────────────────────────────────────────────────
        
        [Test]
        [Category("Interaction")]
        public void ProcessMultipleItems_CallsRepositoryForEach()
        {
            // Arrange
            var items = new[] { 1, 2, 3, 4, 5 };
            mockRepository.GetById(Arg.Any<int>()).Returns(new ItemData());
            
            // Act
            sut.ProcessMultiple(items);
            
            // Assert - Verify GetById was called for each item
            mockRepository.Received(5).GetById(Arg.Any<int>());
            
            // Assert - Verify specific IDs were requested
            foreach (var id in items)
            {
                mockRepository.Received(1).GetById(id);
            }
        }
        
        [Test]
        [Category("Interaction")]
        public void UpdateItem_PassesModifiedDataToRepository()
        {
            // Arrange
            var original = new ItemData { Id = 1, Name = "Original", Health = 100 };
            
            // Act
            sut.UpdateItem(original, health: 50);
            
            // Assert - Verify repository.Save was called with modified item
            mockRepository.Received(1).Save(Arg.Is<ItemData>(x => 
                x.Id == 1 && 
                x.Name == "Original" &&
                x.Health == 50));
        }
        
        // ────────────────────────────────────────────────────────
        // Test: State Verification (Before/After)
        // ────────────────────────────────────────────────────────
        
        [Test]
        [Category("State")]
        public void Initialize_SetsCorrectInitialState()
        {
            // Arrange
            // (Fresh setup in SetUp method)
            
            // Assert - Verify initial state
            Assert.That(sut.IsInitialized, Is.False);
            Assert.That(sut.ItemCount, Is.EqualTo(0));
        }
        
        [Test]
        [Category("State")]
        public void LoadAllItems_PopulatesInternalCache()
        {
            // Arrange
            mockRepository.GetAll().Returns(new[]
            {
                new ItemData { Id = 1, Name = "Item1" },
                new ItemData { Id = 2, Name = "Item2" }
            });
            
            // Act
            sut.LoadAllItems();
            
            // Assert - Verify internal state was updated
            Assert.That(sut.ItemCount, Is.EqualTo(2));
            Assert.That(sut.GetCachedItem(1), Is.Not.Null);
            Assert.That(sut.GetCachedItem(2), Is.Not.Null);
        }
        
        [Test]
        [Category("State")]
        public void ClearCache_ResetsState()
        {
            // Arrange
            mockRepository.GetAll().Returns(new[] { new ItemData { Id = 1 } });
            sut.LoadAllItems();
            Assert.That(sut.ItemCount, Is.GreaterThan(0), "Cache should be populated");
            
            // Act
            sut.ClearCache();
            
            // Assert
            Assert.That(sut.ItemCount, Is.EqualTo(0), "Cache should be cleared");
        }
        
        // ────────────────────────────────────────────────────────
        // Test: Method Chaining / Sequencing
        // ────────────────────────────────────────────────────────
        
        [Test]
        [Category("Sequencing")]
        public void SequenceOfOperations_MaintainsCorrectOrder()
        {
            // Arrange
            var callOrder = new List<string>();
            
            mockRepository.When(x => x.GetById(Arg.Any<int>())).Do(x => callOrder.Add("GetById"));
            mockRepository.When(x => x.Save(Arg.Any<ItemData>())).Do(x => callOrder.Add("Save"));
            mockPublisher.When(x => x.Publish(Arg.Any<IEvent>())).Do(x => callOrder.Add("Publish"));
            
            // Act
            var item = new ItemData { Id = 1 };
            sut.LoadAndSave(item);
            
            // Assert
            Assert.That(callOrder, Is.EqualTo(new[] { "GetById", "Save", "Publish" }),
                "Operations should execute in the correct order");
        }
        
        // ────────────────────────────────────────────────────────
        // Test: Multiple Return Values (Sequencing)
        // ────────────────────────────────────────────────────────
        
        [Test]
        [Category("Interaction")]
        public void FetchNextItem_ReturnsSequenceOfDifferentItems()
        {
            // Arrange - Configure mock to return different items on successive calls
            mockRepository.GetNext()
                .Returns(
                    new ItemData { Id = 1, Name = "First" },
                    new ItemData { Id = 2, Name = "Second" },
                    new ItemData { Id = 3, Name = "Third" }
                );
            
            // Act
            var first = sut.FetchNextItem();
            var second = sut.FetchNextItem();
            var third = sut.FetchNextItem();
            
            // Assert
            Assert.That(first.Name, Is.EqualTo("First"));
            Assert.That(second.Name, Is.EqualTo("Second"));
            Assert.That(third.Name, Is.EqualTo("Third"));
        }
        
        // ────────────────────────────────────────────────────────
        // Test: Out Parameters
        // ────────────────────────────────────────────────────────
        
        [Test]
        [Category("Interaction")]
        public void TryLoadItem_WhenSuccessful_SetsOutParameter()
        {
            // Arrange
            var itemData = new ItemData { Id = 5, Name = "Found" };
            mockRepository.TryGet(5, out Arg.Any<ItemData>())
                .Returns(x => { x[1] = itemData; return true; });
            
            // Act
            bool success = sut.TryLoadItem(5, out ItemData result);
            
            // Assert
            Assert.That(success, Is.True);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Found"));
        }
        
        [Test]
        [Category("Interaction")]
        public void TryLoadItem_WhenFails_OutParameterIsNull()
        {
            // Arrange
            mockRepository.TryGet(999, out Arg.Any<ItemData>())
                .Returns(x => { x[1] = null; return false; });
            
            // Act
            bool success = sut.TryLoadItem(999, out ItemData result);
            
            // Assert
            Assert.That(success, Is.False);
            Assert.That(result, Is.Null);
        }
        
        // ────────────────────────────────────────────────────────
        // Test: Conditional Logging/Side Effects
        // ────────────────────────────────────────────────────────
        
        [Test]
        [Category("Side Effects")]
        public void ProcessItem_WhenHealthLow_LogsWarning()
        {
            // Arrange
            var item = new ItemData { Id = 1, Health = 10 };
            mockRepository.GetById(1).Returns(item);
            
            // Act
            sut.ProcessItem(1);
            
            // Assert - Verify warning was logged
            mockLogger.Received(1).LogWarning(Arg.Any<string>());
        }
        
        [Test]
        [Category("Side Effects")]
        public void ProcessItem_WhenHealthOk_DoesNotLog()
        {
            // Arrange
            var item = new ItemData { Id = 1, Health = 100 };
            mockRepository.GetById(1).Returns(item);
            
            // Act
            sut.ProcessItem(1);
            
            // Assert - Verify warning was NOT logged
            mockLogger.DidNotReceive().LogWarning(Arg.Any<string>());
        }
        
        // ────────────────────────────────────────────────────────
        // Test: No Calls (Verification of Non-Execution)
        // ────────────────────────────────────────────────────────
        
        [Test]
        [Category("Interaction")]
        public void CacheHit_DoesNotCallRepository()
        {
            // Arrange
            var item = new ItemData { Id = 1 };
            mockRepository.GetById(1).Returns(item);
            sut.LoadItem(1); // Load and cache
            
            // Reset call tracking
            mockRepository.ClearReceivedCalls();
            
            // Act - Load again (should hit cache)
            sut.LoadItem(1);
            
            // Assert - Verify repository was NOT called again
            mockRepository.DidNotReceive().GetById(1);
        }
        
        // ────────────────────────────────────────────────────────
        // Test: Predicate Matching (Complex Arg Validation)
        // ────────────────────────────────────────────────────────
        
        [Test]
        [Category("Interaction")]
        public void BatchProcess_CallsRepositoryWithValidatedItems()
        {
            // Arrange
            var items = new[] { 
                new ItemData { Id = 1, Health = 50 },
                new ItemData { Id = 2, Health = 100 }
            };
            
            // Act
            sut.BatchProcess(items);
            
            // Assert - Verify Save was called only for items with Health > 30
            mockRepository.Received(2).Save(Arg.Is<ItemData>(x => x.Health > 30));
        }
        
        // ────────────────────────────────────────────────────────
        // Test: Exception Flow with State Rollback
        // ────────────────────────────────────────────────────────
        
        [Test]
        [Category("Error Handling")]
        public void ProcessAndCommit_OnFailure_RollsBackState()
        {
            // Arrange
            var initialCount = sut.ItemCount;
            mockRepository.Save(Arg.Any<ItemData>()).Returns(x => { throw new RepositoryException(); });
            
            // Act & Assert
            Assert.Throws<RepositoryException>(() => sut.ProcessAndCommit(new ItemData()));
            
            // Assert - Verify state was rolled back
            Assert.That(sut.ItemCount, Is.EqualTo(initialCount),
                "State should be rolled back on failure");
        }
    }
}
