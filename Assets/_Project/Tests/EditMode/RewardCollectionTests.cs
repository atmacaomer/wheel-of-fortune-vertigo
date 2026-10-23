using System;
using NUnit.Framework;
using VertigoCase.Gameplay;

namespace VertigoCase.Tests.EditMode
{
    public class RewardCollectionTests
    {
        private RewardCollection _collection;

        [SetUp]
        public void SetUp()
        {
            _collection = new RewardCollection();
        }

        [Test]
        public void Add_SameRewardTwice_SumsAmounts()
        {
            _collection.Add("gold", 10);
            _collection.Add("gold", 5);

            Assert.AreEqual(15, _collection.GetAmount("gold"));
        }

        [Test]
        public void Add_RaisesRewardChangedWithNewTotal()
        {
            string changedId = null;
            int changedTotal = 0;
            _collection.RewardChanged += (id, total) => { changedId = id; changedTotal = total; };

            _collection.Add("cash", 100);
            _collection.Add("cash", 50);

            Assert.AreEqual("cash", changedId);
            Assert.AreEqual(150, changedTotal);
        }

        [Test]
        public void Clear_RemovesEverythingAndRaisesCleared()
        {
            bool cleared = false;
            _collection.Cleared += () => cleared = true;
            _collection.Add("gold", 10);

            _collection.Clear();

            Assert.IsTrue(cleared);
            Assert.AreEqual(0, _collection.GetAmount("gold"));
            Assert.AreEqual(0, _collection.Amounts.Count);
        }

        [Test]
        public void TrySpend_Enough_SubtractsAndRaisesNewTotal()
        {
            int changedTotal = -1;
            _collection.Add("gold", 30);
            _collection.RewardChanged += (id, total) => changedTotal = total;

            Assert.IsTrue(_collection.TrySpend("gold", 20));
            Assert.AreEqual(10, _collection.GetAmount("gold"));
            Assert.AreEqual(10, changedTotal);
        }

        [Test]
        public void TrySpend_NotEnough_ReturnsFalseAndKeepsAmount()
        {
            _collection.Add("gold", 5);

            Assert.IsFalse(_collection.TrySpend("gold", 20));
            Assert.AreEqual(5, _collection.GetAmount("gold"));
        }

        [Test]
        public void TrySpend_Everything_RemovesTheReward()
        {
            _collection.Add("gold", 20);

            _collection.TrySpend("gold", 20);

            Assert.AreEqual(0, _collection.Amounts.Count);
        }

        [Test]
        public void Add_NonPositiveAmount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _collection.Add("gold", 0));
        }
    }
}
