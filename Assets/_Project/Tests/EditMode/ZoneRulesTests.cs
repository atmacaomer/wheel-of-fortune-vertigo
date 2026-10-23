using System;
using NUnit.Framework;
using VertigoCase.Gameplay;

namespace VertigoCase.Tests.EditMode
{
    public class ZoneRulesTests
    {
        private ZoneRules _rules;

        [SetUp]
        public void SetUp()
        {
            _rules = new ZoneRules(safeInterval: 5, superInterval: 30);
        }

        [TestCase(1, ZoneType.Normal)]
        [TestCase(4, ZoneType.Normal)]
        [TestCase(5, ZoneType.Safe)]
        [TestCase(10, ZoneType.Safe)]
        [TestCase(30, ZoneType.Super)]
        [TestCase(60, ZoneType.Super)]
        public void GetZoneType_ReturnsExpectedType(int zone, ZoneType expected)
        {
            Assert.AreEqual(expected, _rules.GetZoneType(zone));
        }

        [Test]
        public void GetZoneType_ZoneBelowOne_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _rules.GetZoneType(0));
        }

        // Every safe zone would be a super zone, so "next safe zone" would never be found.
        [TestCase(30, 30)]
        [TestCase(10, 5)]
        public void Constructor_SafeIntervalMultipleOfSuper_Throws(int safeInterval, int superInterval)
        {
            Assert.Throws<ArgumentException>(() => new ZoneRules(safeInterval, superInterval));
        }

        [TestCase(7, false)]
        [TestCase(15, true)]
        [TestCase(30, true)]
        public void CanLeave_OnlyOnSafeOrSuperZones(int zone, bool expected)
        {
            Assert.AreEqual(expected, _rules.CanLeave(zone));
        }

        [TestCase(11, 15)]
        [TestCase(15, 15)]
        [TestCase(26, 35)] // 30 is a super zone, so the next safe zone is 35
        public void GetUpcomingZone_Safe(int currentZone, int expected)
        {
            Assert.AreEqual(expected, _rules.GetUpcomingZone(currentZone, ZoneType.Safe));
        }

        [TestCase(11, 30)]
        [TestCase(31, 60)]
        public void GetUpcomingZone_Super(int currentZone, int expected)
        {
            Assert.AreEqual(expected, _rules.GetUpcomingZone(currentZone, ZoneType.Super));
        }
    }
}
