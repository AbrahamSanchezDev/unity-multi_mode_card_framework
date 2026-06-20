// Template: Basic Unit Test Class
// Use this template for simple classes without heavy dependencies

using NUnit.Framework;
using UnityEngine;
using YourNamespace; // Replace with your namespace

namespace Tests
{
    /// <summary>
    /// Tests for [ClassName] to achieve high code coverage.
    /// 
    /// Coverage target: 80%+ statements, 75%+ branches, 90%+ methods
    /// 
    /// Test scenarios:
    /// 1. Happy path (normal operation)
    /// 2. Edge cases (empty, null, boundary values)
    /// 3. Error conditions (invalid input, exceptions)
    /// </summary>
    [TestFixture]
    public class YourClassNameTests
    {
        // ────────────────────────────────────────────────────────
        // Setup & Teardown
        // ────────────────────────────────────────────────────────
        
        private YourClassName sut; // System Under Test
        
        [SetUp]
        public void Setup()
        {
            // Initialize test subject before each test
            sut = new YourClassName();
        }
        
        [TearDown]
        public void Teardown()
        {
            // Clean up after each test (if needed)
            sut = null;
        }
        
        // ────────────────────────────────────────────────────────
        // Test: Constructor
        // ────────────────────────────────────────────────────────
        
        [Test]
        [Category("Constructor")]
        public void Constructor_InitializesWithDefaultValues()
        {
            // Arrange
            // (Done in Setup)
            
            // Act
            // (Already created in Setup)
            
            // Assert
            Assert.That(sut, Is.Not.Null, "Constructor should create instance");
            Assert.That(sut.SomeProperty, Is.EqualTo(expectedDefaultValue));
        }
        
        // ────────────────────────────────────────────────────────
        // Test: Property Methods
        // ────────────────────────────────────────────────────────
        
        [Test]
        [Category("Property")]
        public void PropertyName_Get_ReturnsCurrentValue()
        {
            // Arrange
            var expectedValue = 42;
            sut.PropertyName = expectedValue;
            
            // Act
            var actualValue = sut.PropertyName;
            
            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }
        
        [Test]
        [Category("Property")]
        public void PropertyName_Set_UpdatesValue()
        {
            // Arrange
            var newValue = 100;
            
            // Act
            sut.PropertyName = newValue;
            
            // Assert
            Assert.That(sut.PropertyName, Is.EqualTo(newValue));
        }
        
        // ────────────────────────────────────────────────────────
        // Test: Public Methods - Happy Path
        // ────────────────────────────────────────────────────────
        
        [Test]
        [Category("Happy Path")]
        public void MethodName_WithValidInput_ReturnsExpectedResult()
        {
            // Arrange
            var input = "validInput";
            var expected = "expectedOutput";
            
            // Act
            var result = sut.MethodName(input);
            
            // Assert
            Assert.That(result, Is.EqualTo(expected),
                "MethodName should process input correctly");
        }
        
        // ────────────────────────────────────────────────────────
        // Test: Public Methods - Edge Cases
        // ────────────────────────────────────────────────────────
        
        [Test]
        [Category("Edge Cases")]
        public void MethodName_WithEmptyInput_ReturnsDefault()
        {
            // Arrange
            var emptyInput = "";
            
            // Act
            var result = sut.MethodName(emptyInput);
            
            // Assert
            Assert.That(result, Is.EqualTo(default(string)),
                "Should handle empty input gracefully");
        }
        
        [Test]
        [Category("Edge Cases")]
        public void MethodName_WithNullInput_DoesNotThrow()
        {
            // Arrange
            string nullInput = null;
            
            // Act & Assert
            Assert.DoesNotThrow(() => sut.MethodName(nullInput),
                "Should not throw when input is null");
        }
        
        [Test]
        [Category("Edge Cases")]
        public void MethodName_WithBoundaryValue_ReturnsCorrectResult()
        {
            // Arrange
            var boundaryValue = 0;
            
            // Act
            var result = sut.MethodName(boundaryValue);
            
            // Assert
            Assert.That(result, Is.GreaterThanOrEqualTo(0),
                "Should handle boundary value correctly");
        }
        
        // ────────────────────────────────────────────────────────
        // Test: Public Methods - Error Conditions
        // ────────────────────────────────────────────────────────
        
        [Test]
        [Category("Error Handling")]
        public void MethodName_WithInvalidInput_ThrowsException()
        {
            // Arrange
            var invalidInput = -1;
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => sut.MethodName(invalidInput),
                "Should throw ArgumentException for invalid input");
        }
        
        [Test]
        [Category("Error Handling")]
        public void MethodName_WhenStateInvalid_ReturnsFalse()
        {
            // Arrange
            sut.IsReady = false; // Invalid state
            
            // Act
            var result = sut.MethodName();
            
            // Assert
            Assert.That(result, Is.False,
                "Should return false when state is invalid");
        }
        
        // ────────────────────────────────────────────────────────
        // Test: Conditional Branches
        // ────────────────────────────────────────────────────────
        
        [Test]
        [Category("Branches")]
        public void MethodWithCondition_WhenConditionTrue_ExecutesCorrectPath()
        {
            // Arrange
            sut.SomeCondition = true;
            
            // Act
            var result = sut.MethodWithCondition();
            
            // Assert
            Assert.That(result, Is.EqualTo(expectedWhenTrue));
        }
        
        [Test]
        [Category("Branches")]
        public void MethodWithCondition_WhenConditionFalse_ExecutesAlternatePath()
        {
            // Arrange
            sut.SomeCondition = false;
            
            // Act
            var result = sut.MethodWithCondition();
            
            // Assert
            Assert.That(result, Is.EqualTo(expectedWhenFalse));
        }
        
        // ────────────────────────────────────────────────────────
        // Test: Collections/Loops
        // ────────────────────────────────────────────────────────
        
        [Test]
        [Category("Collections")]
        public void MethodWithLoop_WithEmptyCollection_ReturnsZero()
        {
            // Arrange
            var emptyList = new List<int>();
            
            // Act
            var result = sut.MethodWithLoop(emptyList);
            
            // Assert
            Assert.That(result, Is.EqualTo(0));
        }
        
        [Test]
        [Category("Collections")]
        public void MethodWithLoop_WithMultipleItems_ProcessesAll()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3, 4, 5 };
            var expected = items.Sum();
            
            // Act
            var result = sut.MethodWithLoop(items);
            
            // Assert
            Assert.That(result, Is.EqualTo(expected),
                "Should process all items in collection");
        }
        
        // ────────────────────────────────────────────────────────
        // Test: State Transitions
        // ────────────────────────────────────────────────────────
        
        [Test]
        [Category("State")]
        public void MethodChangingState_BeforeCall_HasOldState()
        {
            // Arrange & Assert (verify initial state)
            Assert.That(sut.CurrentState, Is.EqualTo(State.Initial));
        }
        
        [Test]
        [Category("State")]
        public void MethodChangingState_AfterCall_HasNewState()
        {
            // Arrange
            Assert.That(sut.CurrentState, Is.EqualTo(State.Initial));
            
            // Act
            sut.ChangeState(State.Active);
            
            // Assert
            Assert.That(sut.CurrentState, Is.EqualTo(State.Active),
                "State should transition after method call");
        }
        
        // ────────────────────────────────────────────────────────
        // Test: Integration with Other Components
        // ────────────────────────────────────────────────────────
        
        [Test]
        [Category("Integration")]
        public void MethodCallingOtherComponent_ReceivesCorrectResult()
        {
            // Arrange
            var dependency = new SomeDependency();
            sut.SetDependency(dependency);
            
            // Act
            var result = sut.MethodUsingDependency();
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(dependency.WasUsed, Is.True,
                "Should use dependency");
        }
        
        // ────────────────────────────────────────────────────────
        // Parameterized Tests (using [TestCase])
        // ────────────────────────────────────────────────────────
        
        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(2, 4)]
        [TestCase(3, 9)]
        [TestCase(-1, 1)]
        [Category("Parameterized")]
        public void MethodWithManyInputs_ReturnsCorrectResult(int input, int expected)
        {
            // Arrange
            // (Input and expected handled by TestCase)
            
            // Act
            var result = sut.Square(input);
            
            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }
        
        // ────────────────────────────────────────────────────────
        // Coverage Gap Tests (add specific tests as gaps are found)
        // ────────────────────────────────────────────────────────
        
        // [Test]
        // [Category("Coverage Gaps")]
        // [Ignore("TODO: Test for uncovered line 45")]
        // public void CovergageGap_TODO_UnknownPath()
        // {
        //     // Arrange
        //     
        //     // Act
        //     
        //     // Assert
        // }
    }
}
