using System;

namespace VertigoCase.Gameplay
{
    // Decides what kind of zone a zone number is. Plain C# so it can be unit tested without Unity.
    public sealed class ZoneRules
    {
        private readonly int _safeInterval;
        private readonly int _superInterval;

        public ZoneRules(int safeInterval, int superInterval)
        {
            if (safeInterval <= 0) throw new ArgumentOutOfRangeException(nameof(safeInterval));
            if (superInterval <= 0) throw new ArgumentOutOfRangeException(nameof(superInterval));
            // Super wins over safe, so if every safe zone is also a super zone there is never a safe zone.
            if (safeInterval % superInterval == 0)
                throw new ArgumentException("Safe interval must not be a multiple of the super interval.", nameof(safeInterval));

            _safeInterval = safeInterval;
            _superInterval = superInterval;
        }

        public ZoneType GetZoneType(int zone)
        {
            if (zone <= 0) throw new ArgumentOutOfRangeException(nameof(zone));

            // Super is checked first: zone 30 is also a multiple of 5, and super wins.
            if (zone % _superInterval == 0) return ZoneType.Super;
            if (zone % _safeInterval == 0) return ZoneType.Safe;
            return ZoneType.Normal;
        }

        // The player may only cash out on zones without a bomb.
        public bool CanLeave(int zone) => GetZoneType(zone) != ZoneType.Normal;

        // First zone of the given type at or after currentZone (for the "SAFE ZONE 15" / "SUPER ZONE 30" boxes).
        public int GetUpcomingZone(int currentZone, ZoneType type)
        {
            int zone = currentZone;
            while (GetZoneType(zone) != type)
            {
                zone++;
            }
            return zone;
        }
    }
}
