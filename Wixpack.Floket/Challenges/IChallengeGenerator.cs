namespace Wixpack.Floket.Challenges;

public interface IChallengeGenerator
{
    (string Prompt, string Answer) Generate();
}

public sealed class MixedChallengeGenerator : IChallengeGenerator
{
    private static readonly Random Rng = Random.Shared;
    private static readonly string[] EmojiPairs =
    [
        "🍎", "🍌", "🍇", "🍊", "🍓", "🥝", "🍑", "🍒"
    ];

    public (string Prompt, string Answer) Generate()
    {
        return Rng.Next(0, 3) switch
        {
            0 => MathChallenge(),
            1 => EmojiChallenge(),
            _ => SequenceChallenge()
        };
    }

    private static (string, string) MathChallenge()
    {
        var a = Rng.Next(2, 15);
        var b = Rng.Next(1, 12);
        return Rng.Next(0, 3) switch
        {
            0 => ($"What is {a} + {b}?", (a + b).ToString()),
            1 => ($"What is {a + b} − {b}?", a.ToString()),
            _ => ($"What is {a} × {b}?", (a * b).ToString())
        };
    }

    private static (string, string) EmojiChallenge()
    {
        var pick = EmojiPairs[Rng.Next(EmojiPairs.Length)];
        return ($"Tap the matching emoji: {pick}", pick);
    }

    private static (string, string) SequenceChallenge()
    {
        var start = Rng.Next(1, 8);
        var answer = (start + 3).ToString();
        return ($"What comes next? {start}, {start + 1}, {start + 2}, ?", answer);
    }
}
