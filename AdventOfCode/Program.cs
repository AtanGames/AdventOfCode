using AdventOfCode.Days2024;
using AdventOfCode.Days2025;

namespace AdventOfCode;

class Program
{
    static void Main(string[] args)
    {
        var sessionToken = File.ReadAllText("../../../../session.txt").Trim();
        FileFetch.SetSessionToken(sessionToken);
        
        new Day7().Run();
        
        Console.ReadKey();
    }
}