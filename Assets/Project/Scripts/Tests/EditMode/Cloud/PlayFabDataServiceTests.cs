using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.TestTools;
using PlayFab;
using PlayFab.ClientModels;
using CardFramework.Cloud.PlayFab;

namespace CardFramework.Tests.EditMode.Cloud
{
    [TestFixture]
    public class PlayFabDataServiceTests
    {
        private MockPlayFabDataWrapper _mockWrapper;
        private PlayFabDataService _dataService;

        [SetUp]
        public void Setup()
        {
            _mockWrapper = new MockPlayFabDataWrapper();
            _dataService = new PlayFabDataService(_mockWrapper);
        }

        [Test]
        public async Task SaveDataAsync_OnSuccess_ReturnsTrue()
        {
            // Arrange
            _mockWrapper.ShouldFail = false;
            var payload = new TestPayload { Value = 100 };

            // Act
            bool result = await _dataService.SaveDataAsync("test_key", payload);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task SaveDataAsync_OnError_LogsErrorAndReturnsFalse()
        {
            // Arrange
            LogAssert.Expect(UnityEngine.LogType.Error, new System.Text.RegularExpressions.Regex(".*PlayFab Save Data Error.*"));
            _mockWrapper.ShouldFail = true;
            var payload = new TestPayload { Value = 100 };

            // Act
            bool result = await _dataService.SaveDataAsync("test_key", payload);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public async Task LoadDataAsync_WithValidKey_ReturnsDeserializedData()
        {
            // Arrange
            _mockWrapper.ShouldFail = false;
            _mockWrapper.MockedJsonReturned = "{\"Value\":999}";

            // Act
            TestPayload result = await _dataService.LoadDataAsync<TestPayload>("test_key");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(999, result.Value);
        }

        [Test]
        public async Task LoadDataAsync_WithMissingKey_ReturnsDefault()
        {
            // Arrange
            _mockWrapper.ShouldFail = false;
            _mockWrapper.MockedJsonReturned = null; // Key does not exist

            // Act
            TestPayload result = await _dataService.LoadDataAsync<TestPayload>("missing_key");

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task LoadDataAsync_OnError_LogsErrorAndReturnsDefault()
        {
            // Arrange
            LogAssert.Expect(UnityEngine.LogType.Error, new System.Text.RegularExpressions.Regex(".*PlayFab Load Data Error.*"));
            _mockWrapper.ShouldFail = true;

            // Act
            TestPayload result = await _dataService.LoadDataAsync<TestPayload>("test_key");

            // Assert
            Assert.IsNull(result);
        }

        [Serializable]
        private class TestPayload { public int Value; }

        // Controlled Mock Class for predictable pipeline outcomes
        private class MockPlayFabDataWrapper : IPlayFabDataWrapper
        {
            public bool ShouldFail;
            public string MockedJsonReturned;

            public void UpdateUserData(UpdateUserDataRequest request, Action<UpdateUserDataResult> resultCallback, Action<PlayFabError> errorCallback)
            {
                if (ShouldFail)
                    errorCallback?.Invoke(new PlayFabError { ErrorMessage = "Mocked Throttle/Network Error" });
                else
                    resultCallback?.Invoke(new UpdateUserDataResult());
            }

            public void GetUserData(GetUserDataRequest request, Action<GetUserDataResult> resultCallback, Action<PlayFabError> errorCallback)
            {
                if (ShouldFail)
                {
                    errorCallback?.Invoke(new PlayFabError { ErrorMessage = "Mocked Retrieval Failure" });
                    return;
                }

                var dictionary = new Dictionary<string, UserDataRecord>();
                if (MockedJsonReturned != null)
                {
                    dictionary.Add(request.Keys[0], new UserDataRecord { Value = MockedJsonReturned });
                }

                resultCallback?.Invoke(new GetUserDataResult { Data = dictionary });
            }
        }
    }
}