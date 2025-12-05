namespace AdventOfCode.Days;

public class Day3 : DayBase
{
    protected override int Day => 3;
    
    public override void Run()
    {
        string input = GetAocInput();
        
        //input = "987654321111111\n811111111111119\n234234234234278\n818181911112111";
        
        var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        long sum = 0;
        
        foreach (var line in lines)
        {
            // for (int x1 = 0; x1 < line.Length; x1++)
            // {
            //     for (int x2 = x1 + 1; x2 < line.Length; x2++)
            //     {
            //         var c1 = line[x1];
            //         var c2 = line[x2];
            //
            //         string num = c1.ToString() + c2.ToString();
            //         int numValue = int.Parse(num);
            //         
            //         if (numValue > maxNum)
            //             maxNum = numValue;
            //     }
            // }
            
            long maxNum = 0;
            int requiredChars = 12;
            int minIndex = 0;

            for (int c = requiredChars; c > 0; c--)
            {
                int maxCharNum = -1;
                int maxCharIndex = 0;

                for (int x1 = minIndex; x1 < line.Length - (c - 1); x1++)
                {
                    int n = int.Parse(line[x1].ToString());
                    
                    if (n > maxCharNum)
                    {
                        maxCharNum = n;
                        maxCharIndex = x1;
                    }
                }
                
                maxNum += maxCharNum * TenPow(c - 1);
                minIndex = maxCharIndex + 1;
            }
            
            Console.WriteLine("Line: " + line + " Max Pair: " + maxNum);
            sum += maxNum;
        }
        
        Console.WriteLine("Total Sum: " + sum);
    }
    
    long TenPow(int x)
    {
        long result = 1;
        for (int i = 0; i < x; i++)
        {
            result *= 10;
        }
        return result;
    }
}