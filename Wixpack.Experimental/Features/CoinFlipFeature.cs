namespace Wixpack.Experimental.Features;

public sealed class CoinFlipFeature
{
    public string Name => "coin-flip";
    public string Flip() => Random.Shared.Next(0, 2) == 0 ? "heads" : "tails";
}
