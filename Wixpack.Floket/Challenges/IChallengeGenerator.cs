namespace Wixpack.Floket.Challenges;

public interface IChallengeGenerator
{
    (string Prompt, string Answer) Generate();
}

/// <summary>
/// Simple human-solvable math challenges. Hard for naive bots, easy for people.
/// </summary>
public sealed class MathChallengeGenerator : IChallengeGenerator
{
    private static readonly Random Rng = Random.Shared;

    public (string Prompt, string Answer) Generate()
    {
        var a = Rng.Next(2, 15);
        var b = Rng.Next(1, 12);
        var op = Rng.Next(0, 3);

        return op switch
        {
            0 => ($"What is {a} + {b}?", (a + b).ToString()),
            1 => ($"What is {a + b} − {b}?", a.ToString()),
            _ => ($"What is {a} × {b}?", (a * b).ToString())
        };
    }
}
