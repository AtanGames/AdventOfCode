namespace AdventOfCode.Days2025;

public class Day6 : DayBase
{
    protected override int Day => 6;
    
    protected override string GetAocInput()
    {
        return base.GetAocInput();

        return "123 328  51 64 \n 45 64  387 23 \n  6 98  215 314\n*   +   *   +  ";
    }
    
    public override void Run()
    {
        var lines = GetAocInputAsLines();
        
        List<string[]> columns = new List<string[]>();
        int xLength = lines[0].Length;

        int cLen = 0;
        for (int x = 0; x < xLength; x++)
        {
            bool allSpace = true;
            
            for (int y = 0; y < lines.Length; y++)
            {
                if (lines[y][x] != ' ' && x != xLength - 1)
                {
                    allSpace = false;
                    cLen++;
                    break;
                }
            }

            //Only took 10min to figure this out...
            int additionalLen = (x == xLength - 1) ? 1 : 0;
            
            if (allSpace)
            {
                List<string> col = new List<string>();
                
                for (int y = 0; y < lines.Length; y++)
                {
                    var segment = lines[y].Substring(x - cLen, cLen + additionalLen);
                    col.Add(segment);
                }
                
                columns.Add(col.ToArray());
                cLen = 0;
            }
        }
        
        Console.WriteLine("Seperated input into " + columns.Count + " columns.");

        //output column for debug
        
        foreach (var s in columns)
        {
            Console.WriteLine("Column:");
            foreach (var line in s)
            {
                Console.WriteLine(line);
            }
        }
        
        long sum = 0;
        
        foreach (var col in columns)
        {
            int yLen = col.Length;
            int xLen = col[0].Length;

            List<long> nums = new List<long>();
            
            for (int numIndex = 0; numIndex < xLen; numIndex++)
            {
                long targetNum = 0;
                int digitIndex = 0;
                
                for (int y = yLen - 2; y >= 0; y--)
                {
                    var c = col[y][numIndex];
                    if (c == ' ')
                        continue;
                    
                    var digit = long.Parse(c.ToString());
                    
                    targetNum += digit * (long)Math.Pow(10, digitIndex);
                    
                    digitIndex++;
                }
                
                nums.Add(targetNum);
            }
            
            Console.WriteLine("Numbers: " + string.Join(", ", nums));
            
            var op = ParseOperation(col[yLen - 1][0].ToString());
            var result = ApplyOperation(nums.ToArray(), op);
            Console.WriteLine($"Column result: {result} operation: {op}");
            sum += result;
        }
        
        Console.WriteLine("Total sum: " + sum);

        /* Part 1
        var lines = GetAocInputAsTable();
        
        var xLength = lines.GetLength(0);
        var yLength = lines.GetLength(1);
        long sum = 0;
        
        for (int x = 0; x < xLength; x++)
        {
            List<long> nums = new List<long>();
            
            for (int y = 0; y < yLength - 1; y++)
            {
                var num = long.Parse(lines[x, y]);
                nums.Add(num);
            }
            
            var op = ParseOperation(lines[x, yLength - 1]);
            
            var result = ApplyOperation(nums.ToArray(), op);
            sum += result;
            
            Console.WriteLine($"Line {x + 1} result: {result} operation: {op}");
        }
        
        Console.WriteLine("Total sum: " + sum);*/
    }

    private long ApplyOperation(long[] num, Operation op)
    {
        return op switch
        {
            Operation.Add => num.Sum(),
            Operation.Multiply => num.Aggregate(1L, (a, b) => a * b),
            _ => throw new Exception("Unknown operation")
        };
    }
    
    private Operation ParseOperation(string op)
    {
        return op switch
        {
            "+" => Operation.Add,
            "*" => Operation.Multiply,
            _ => throw new Exception("Unknown operation: " + op)
        };
    }
    
    private enum Operation
    {
        Add, Multiply
    }
}