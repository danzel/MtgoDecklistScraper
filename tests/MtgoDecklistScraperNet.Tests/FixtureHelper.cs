namespace MtgoDecklistScraperNet.Tests;

public static class FixtureHelper
{
    public static string Load(string name) =>
        File.ReadAllText(Path.Combine("Fixtures", name));
}
