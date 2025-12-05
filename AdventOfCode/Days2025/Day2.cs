namespace AdventOfCode.Days2025;

public class Day2 : DayBase
{
    protected override int Day => 2;
    
    public override void Run()
    {
        string input = GetAocInput();
        
        //input =
        //    "11-22,95-115,998-1012,1188511880-1188511890,222220-222224,1698522-1698528,446443-446449,38593856-38593862,565653-565659,824824821-824824827,2121212118-2121212124";
        
        var ranges = input.Split(',');

        long result = 0;
        
        foreach (var range in ranges)
        {
            var nums = range.Split('-');
            var start = long.Parse(nums[0]);
            var end = long.Parse(nums[1]);
            
            Console.WriteLine("Range: " + range + " Start: " + start + " End: " + end);

            for (long i = start; i <= end; i++)
            {
                string iString = i.ToString();
                int digitCount = iString.ToString().Length;
                int maxRepeat = digitCount / 2;

                for (int numIndex = 1; numIndex <= maxRepeat; numIndex++)
                {
                    if (digitCount % numIndex != 0)
                        continue;

                    int patternCount = digitCount / numIndex;
                    
                    long pattern = long.Parse(iString.Substring(0, numIndex));
                    
                    bool isMatch = true;

                    for (int patternIndex = 1; patternIndex < patternCount; patternIndex++)
                    {
                        long nextPattern = long.Parse(iString.Substring(patternIndex * numIndex, numIndex));
                        if (nextPattern != pattern)
                        {
                            isMatch = false;
                            break;
                        }
                    }

                    if (isMatch)
                    {
                        Console.WriteLine($"Number {i} has a repeating pattern of {pattern}");
                        result += i;
                        break;
                    }
                }
                
                // if (digitCount % 2 != 0)
                //     continue;
                //
                // int halfLength = digitCount / 2;
                // long pattern = long.Parse(iString.Substring(0, halfLength));
                // long pattern2 = long.Parse(iString.Substring(halfLength, halfLength));
                //
                // if (pattern == pattern2)
                // {
                //     Console.WriteLine($"Found pattern {pattern} in number {i}");
                //     result += i;
                // }
            }
        }
        
        Console.WriteLine("Final Result: " + result);
    }
}