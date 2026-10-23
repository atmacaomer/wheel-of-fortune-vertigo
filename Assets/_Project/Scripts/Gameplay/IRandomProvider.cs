namespace VertigoCase.Gameplay
{
    // Abstraction over randomness so game rules can be tested with a predictable fake.
    public interface IRandomProvider
    {
        int Range(int minInclusive, int maxExclusive);
    }
}
