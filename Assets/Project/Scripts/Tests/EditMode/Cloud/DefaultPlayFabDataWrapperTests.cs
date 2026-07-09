using NUnit.Framework;
using System;
using PlayFab.ClientModels;
using CardFramework.Cloud.PlayFab;

namespace CardFramework.Tests.EditMode.Presentation {
    [TestFixture]
    public class DefaultPlayFabDataWrapperTests {
        private DefaultPlayFabDataWrapper _wrapper;

        [SetUp]
        public void Setup() {
            _wrapper = new DefaultPlayFabDataWrapper();
        }

        [Test]
        public void Wrapper_ImplementsExpectedInterfaceBoundary() {
            // Verify our production component accurately inherits from the architectural decoupling layout boundary
            Assert.IsTrue(_wrapper is IPlayFabDataWrapper, "The production client wrapper must implement the IPlayFabDataWrapper interface abstraction contract.");
        }      

        [Test]
        public void Wrapper_UpdateUserData_HandlesNullParametersGracefully() {
            // Arrange
            var wrapper = new DefaultPlayFabDataWrapper();

            // Act & Assert: Verify that forwarding null arguments doesn't crash the wrapper
            Assert.DoesNotThrow(() => {
                wrapper.UpdateUserData(null, null, null);
            }, "The wrapper should safely pass null arguments forward to PlayFab's static runtime without throwing an exception.");
        }

        [Test]
        public void Wrapper_GetUserData_HandlesNullParametersGracefully() {
            // Arrange
            var wrapper = new DefaultPlayFabDataWrapper();

            // Act & Assert: Verify that forwarding null arguments doesn't crash the wrapper
            Assert.DoesNotThrow(() => {
                wrapper.GetUserData(null, null, null);
            }, "The wrapper boundary should cleanly forward null parameters without generating synchronous exceptions.");
        }
    }
}