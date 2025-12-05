namespace AdventOfCode.Days;

public class Day4 : DayBase
{
    protected override int Day => 4;
    
    public override void Run()
    {
        string input = GetAocInput();
        
        //input =
        //    "..@@.@@@@.\n@@@.@.@.@@\n@@@@@.@.@@\n@.@@@@..@.\n@@.@@@@.@@\n.@@@@@@@.@\n.@.@.@.@@@\n@.@@@.@@@@\n.@@@@@@@@.\n@.@.@@@.@.";
        
        var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        
        var lengthX = lines[0].Length;
        var lengthY = lines.Length;
        
        byte[,] grid = new byte[lengthX, lengthY];

        for (int y = 0; y < lengthY; y++)
        {
            var line = lines[y];
            
            for (int x = 0; x < lengthX; x++)
            {
                var c = line[x];
                
                grid[x, lengthY - y - 1] = c == '@' ? (byte)1 : (byte)0;
            }
        }

        int liftableCount = 0;
        
        while (true)
        {
            var liftable = GetLiftPositions(grid, lengthX, lengthY);
            
            if (liftable.Count == 0)
                break;
            
            liftableCount += liftable.Count;
            
            foreach (var pos in liftable)
            {
                grid[pos.x, pos.y] = 0;
            }
        }
        
        Console.WriteLine("Liftable position count: " + liftableCount);
    }

    private static List<(int x, int y)> GetLiftPositions(byte[,] grid, int lengthX, int lengthY)
    {
        List<(int x, int y)> lift = new List<(int x, int y)>();

        for (int x = 0; x < lengthX; x++)
        {
            for (int y = 0; y < lengthY; y++)
            {
                if (grid[x, y] != 1)
                    continue;
                
                int neighborCount = 0;
                
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nX = x + dx;
                        int nY = y + dy;
                        
                        if (nX < 0 || nX >= lengthX || nY < 0 || nY >= lengthY)
                            continue;

                        if (dx == 0 && dy == 0)
                            continue;
                        
                        neighborCount += grid[nX, nY];
                    }
                }
                
                if (neighborCount < 4)
                    lift.Add((x, y));
            }
        }
        
        return lift;
    }
}