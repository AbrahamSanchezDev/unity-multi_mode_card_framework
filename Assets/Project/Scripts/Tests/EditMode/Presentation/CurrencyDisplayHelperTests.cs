using System;
using CardFramework.Core.Interfaces;
using CardFramework.Presentation;
using NUnit.Framework;

namespace CardFramework.Tests.EditMode.Presentation {
    public class CurrencyDisplayHelperTests {
        [Test]
        public void FormatBalance_ReturnsExpectedLabelText() {
            Assert.AreEqual("Balance: 250 GD", CurrencyDisplayHelper.FormatBalance(250, "GD"));
        }

        [Test]
        public void Bind_UpdatesFromExistingBalanceAndFutureEvents() {
            var economyService = new MockEconomyService(100);
            int? latestBalance = null;

            using (var binder = new CurrencyDisplayHelper(economyService, balance => latestBalance = balance)) {
                Assert.AreEqual(100, latestBalance);

                economyService.TriggerBalanceUpdate(320);
                Assert.AreEqual(320, latestBalance);
            }

            economyService.TriggerBalanceUpdate(999);
            Assert.AreEqual(320, latestBalance);
        }

        private sealed class MockEconomyService : IEconomyService {
            public event Action<int> OnBalanceUpdated;
            public event Action<string> OnEconomyError;

            public MockEconomyService(int initialBalance) {
                CurrentGold = initialBalance;
            }

            public int CurrentGold { get; private set; }

            public void RefreshBalance() => OnBalanceUpdated?.Invoke(CurrentGold);
            public void CreditGold(int amount) {
                CurrentGold += amount;
                OnBalanceUpdated?.Invoke(CurrentGold);
            }

            public void DebitGold(int amount) {
                CurrentGold -= amount;
                OnBalanceUpdated?.Invoke(CurrentGold);
            }

            public void TriggerBalanceUpdate(int newBalance) {
                CurrentGold = newBalance;
                OnBalanceUpdated?.Invoke(newBalance);
            }

            public void TriggerEconomyError(string errorMessage) {
                OnEconomyError?.Invoke(errorMessage);
            }
        }
    }
}
