using System;
using System.Collections.Generic;

namespace VertigoCase.Gameplay
{
    // Decides which slice wins BEFORE the wheel animates; the view only plays the result.
    public sealed class WheelSpinResolver
    {
        private readonly IRandomProvider _random;

        public WheelSpinResolver(IRandomProvider random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        // Picks an index with probability proportional to its weight (weights 40,30,10,20 → 40%,30%,10%,20%).
        public int PickSliceIndex(IReadOnlyList<int> weights)
        {
            if (weights == null || weights.Count == 0) throw new ArgumentException("Wheel has no slices.", nameof(weights));

            int total = 0;
            for (int i = 0; i < weights.Count; i++)
            {
                if (weights[i] < 0) throw new ArgumentException($"Slice {i} has a negative weight.", nameof(weights));
                total += weights[i];
            }
            if (total == 0) throw new ArgumentException("All slice weights are zero.", nameof(weights));

            // Lay the weights end to end on a ruler [0, total) and see where the roll lands.
            int roll = _random.Range(0, total);
            for (int i = 0; i < weights.Count; i++)
            {
                if (roll < weights[i]) return i;
                roll -= weights[i];
            }

            throw new InvalidOperationException("Unreachable: roll is always below the total weight.");
        }
    }
}
