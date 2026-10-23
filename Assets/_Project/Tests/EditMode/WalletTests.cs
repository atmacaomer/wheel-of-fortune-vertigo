using System.Collections.Generic;
using NUnit.Framework;
using VertigoCase.Gameplay;

namespace VertigoCase.Tests.EditMode
{
    public class WalletTests
    {
        [Test]
        public void Deposit_AddsEveryRewardOnTopOfExistingAmounts()
        {
            var wallet = new Wallet();
            wallet.Add("gold", 5);

            wallet.Deposit(new Dictionary<string, int> { ["gold"] = 10, ["cash"] = 100 });

            Assert.AreEqual(15, wallet.GetAmount("gold"));
            Assert.AreEqual(100, wallet.GetAmount("cash"));
        }

        [Test]
        public void TrySpend_NotEnough_ReturnsFalse()
        {
            var wallet = new Wallet();
            wallet.Add("gold", 3);

            Assert.IsFalse(wallet.TrySpend("gold", 5));
            Assert.AreEqual(3, wallet.GetAmount("gold"));
        }

        [Test]
        public void Changed_IsRaisedWithNewTotal()
        {
            var wallet = new Wallet();
            int lastTotal = -1;
            wallet.Changed += (id, total) => lastTotal = total;

            wallet.Add("gold", 7);

            Assert.AreEqual(7, lastTotal);
        }
    }
}
