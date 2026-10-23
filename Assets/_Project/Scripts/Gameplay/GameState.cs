namespace VertigoCase.Gameplay
{
    public enum GameState
    {
        ReadyToSpin,
        Spinning,
        // Just arrived on a super zone: its special reward must be claimed before the golden spin.
        SuperRewardOffer,
        BombHit,
        CashedOut
    }
}
