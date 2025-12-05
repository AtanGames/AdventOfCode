namespace AdventOfCode.Days2025;

public class Day1 : DayBase
{
    protected override int Day => 1;

    public override void Run()
    {
        string input = GetAocInput();
        
        string[] lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        
        int dialPos = 50;
        int zeroCount = 0;
        int loopCount = 0;
        
        foreach (var line in lines)
        {
            var dir = line[0];
            var steps = int.Parse(line[1..]);
            
            var offset = dir == 'L' ? -steps : steps;
            
            if (dialPos == 0 && offset < 0)
                loopCount--;
            
            dialPos += offset;
            
            while (dialPos >= 100)
            {
                if (dialPos != 100)
                    loopCount++;
                
                dialPos -= 100;
            }
            
            while (dialPos < 0)
            {
                loopCount++;
                dialPos += 100;
            }
            
            Console.WriteLine($"Moved {dir}{steps}, new position: {dialPos}");
            Console.WriteLine($"Loops: {loopCount}, Zeros: {zeroCount}");
            
            if (dialPos == 0)
                zeroCount++;
        }
        
        Console.WriteLine($"Final dial position: {dialPos}");
        Console.WriteLine($"Number of times dial hit zero: {zeroCount}");
        Console.WriteLine($"Number of loops completed: {loopCount}");
        Console.WriteLine($"Result: {zeroCount + loopCount}");
    }
}