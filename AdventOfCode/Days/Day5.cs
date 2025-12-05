namespace AdventOfCode.Days;

public class Day5 : DayBase
{
    protected override int Day => 5;
    
    public override void Run()
    {
        string input = GetAocInput();
        
        //input = "3-5\n10-14\n16-20\n12-18\n\n1\n5\n8\n11\n17\n32";
        
        var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        
        List<(long start, long end)> ranges = new List<(long start, long end)>();
        List<long> ids = new List<long>();

        foreach (var line in lines)
        {
            if (line.Contains('-'))
            {
                var parts = line.Split('-');
                long start = long.Parse(parts[0]);
                long end = long.Parse(parts[1]);
                ranges.Add((start, end));
            }
            else
                ids.Add(long.Parse(line));
        }
        
        for (int i = ranges.Count - 1; i >= 0; i--)
        {
            var (currentStart, currentEnd) = ranges[i];
            var originalStart = currentStart;
            var originalEnd = currentEnd;
            
            for (int j = 0; j < ranges.Count; j++)
            {
                if (i == j)
                    continue;
                
                var (otherStart, otherEnd) = ranges[j];
                
                if (currentStart <= otherEnd && currentStart >= otherStart)
                {
                    currentStart = Math.Max(currentStart, otherEnd + 1);
                }

                if (currentEnd >= otherStart && currentEnd <= otherEnd)
                {
                    currentEnd = Math.Min(currentEnd, otherStart - 1);
                }
            }
            
            Console.WriteLine("Merged range: " + originalStart + "-" + originalEnd + " to " + currentStart + "-" + currentEnd);

            if (currentStart > currentEnd)
                ranges.RemoveAt(i);
            else
                ranges[i] = (currentStart, currentEnd);
        }

        long idCount = 0;
        
        foreach (var (start, end) in ranges)
        {
            Console.WriteLine("Counting IDs in range: " + start + "-" + end);
            idCount += (end - start + 1);
        }
        
        Console.WriteLine("Total valid IDs: " + idCount);
        
        // int validCount = 0;
        //
        // foreach (var id in ids)
        // {
        //     bool isValid = false;
        //     Console.WriteLine("Checking ID: " + id);
        //     
        //     foreach (var range in ranges)
        //     {
        //         if (id >= range.start && id <= range.end)
        //         {
        //             Console.WriteLine($"ID {id} is valid in range {range.start}-{range.end}");
        //             isValid = true;
        //             break;
        //         }
        //     }
        //     
        //     if (isValid)
        //         validCount++;
        // }
        //
        // Console.WriteLine("Total valid IDs: " + validCount);
    }
}