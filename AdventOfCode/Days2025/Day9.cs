namespace AdventOfCode.Days2025;

public class Day9 : DayBase
{
    protected override int Day => 9;

    protected override string GetAocInput()
    {
        return base.GetAocInput();

        //return "7,1\n11,1\n11,7\n9,7\n9,5\n2,5\n2,3\n7,3";
    }

    public override void Run()
    {
        var input = GetAocInputAsLines();
        
        var originPoints = input.Select(t => t.Split(',')).Select(t => (int.Parse(t[0]), int.Parse(t[1]))).ToArray();

        // Just quantize the points to reduce grid size and hope for the best
        int quant = 100;
        var points = originPoints.Select(t => (t.Item1 / quant, t.Item2 / quant)).ToArray();
        
        var minX = points.Min(t => t.Item1);
        var maxX = points.Max(t => t.Item1);
        var minY = points.Min(t => t.Item2);
        var maxY = points.Max(t => t.Item2);
        
        int xLength = maxX - minX + 1;
        int yLength = maxY - minY + 1;
        
        Console.WriteLine("Grid size: " + xLength + " x " + yLength);
        
        var grid = new byte[xLength, yLength];

        for (var i = 0; i < points.Length; i++)
        {
            var (x, y) = points[i];
            
            var localX = x - minX;
            var localY = y - minY;

            grid[localX, localY] = 1;
            
            var (nextX, nextY) = points[(i + 1) % points.Length];

            if (nextX == x)
            {
                for (int dy = y + 1; dy < nextY; dy++)
                {
                    var ly = dy - minY;
                    grid[localX, ly] = 2;
                }

                for (int dy = y - 1; dy > nextY; dy--)
                {
                    var ly = dy - minY;
                    grid[localX, ly] = 2;
                }
            }
            else if (nextY == y)
            {
                for (int dx = x + 1; dx < nextX; dx++)
                {
                    var lx = dx - minX;
                    grid[lx, localY] = 2;
                }
                
                for (int dx = x - 1; dx > nextX; dx--)
                {
                    var lx = dx - minX;
                    grid[lx, localY] = 2;
                }
            }
            else
            {
                throw new Exception("Non-straight line!");
            }
        }

        var startPoint = points[0];
        startPoint.Item1 -= 10000 / quant;
        startPoint.Item2 += 1000 / quant;
        
        //OutputGrid(grid);
        
        FloodFillGrid(grid, startPoint.Item1 - minX, startPoint.Item2 - minY);

        //OutputGrid(grid);

        long maxArea = 0;
        int p1Index = 0;
        int p2Index = 0;
        
        for (int i = 0; i < points.Length; i++)
        {
            for (int j = 0; j < points.Length; j++)
            {
                if (i == j)
                    continue;
                
                var (x1, y1) = points[i];
                var (x2, y2) = points[j];

                long area = (Math.Abs(x1 - x2) + 1) * (long)(Math.Abs(y1 - y2) + 1);

                if (area <= maxArea)
                    continue;
                
                bool valid = true;
                
                var startX = Math.Min(x1, x2) - minX;
                var endX = Math.Max(x1, x2) - minX;
                
                var startY = Math.Min(y1, y2) - minY;
                var endY = Math.Max(y1, y2) - minY;
                
                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        if (x == x1 && y == y1)
                            continue;
                        
                        if (x == x2 && y == y2)
                            continue;
                        
                        if (grid[x, y] == 0)
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (!valid)
                        break;
                }

                if (valid)
                {
                    maxArea = area;
                    p1Index = i;
                    p2Index = j;
                }
            }
        }
        
        Console.WriteLine("Max area: " + maxArea + " between points " + p1Index + " and " + p2Index);
        
        var realPoint1 = (originPoints[p1Index].Item1, originPoints[p1Index].Item2);
        var realPoint2 = (originPoints[p2Index].Item1, originPoints[p2Index].Item2);
        var realArea = (Math.Abs(realPoint1.Item1 - realPoint2.Item1) + 1) * (long)(Math.Abs(realPoint1.Item2 - realPoint2.Item2) + 1);
        
        Console.WriteLine("Real area: " + realArea + " between points " + realPoint1 + " and " + realPoint2);
        
        // Part 1
        // long maxArea = 0;
        // int cIndex = 0;
        // int otherIndex = 0;
        //
        // for (int i = 0; i < coords.Length; i++)
        // {
        //     for (int j = 0; j < coords.Length; j++)
        //     {
        //         if (i == j)
        //             continue;
        //         
        //         long area = coords[i].AreaWith(coords[j]);
        //         
        //         if (area > maxArea)
        //         {
        //             maxArea = area;
        //             cIndex = i;
        //             otherIndex = j;
        //         }
        //     }
        // }
        //
        // Console.WriteLine("Max area: " + maxArea + " between coords " + cIndex + " and others.");
        //
        // coords[cIndex].AreaWith(coords[otherIndex]);
    }

    private static void FloodFillGrid(byte[,] grid, int startX, int startY)
    {
        int fillValue = 2;
        
        int lengthX = grid.GetLength(0);
        int lengthY = grid.GetLength(1);
        
        Queue<(int x, int y)> toFill = new Queue<(int x, int y)>();
        
        toFill.Enqueue((startX, startY));
        
        while (toFill.Count > 0)
        {
            var (x, y) = toFill.Dequeue();

            if (x < 0 || x >= lengthX || y < 0 || y >= lengthY)
                continue;
            
            if (grid[x, y] != 0)
                continue;

            grid[x, y] = (byte)fillValue;

            toFill.Enqueue((x + 1, y));
            toFill.Enqueue((x - 1, y));
            toFill.Enqueue((x, y + 1));
            toFill.Enqueue((x, y - 1));
        }
    }

    private static void OutputGrid(byte[,] grid)
    {
        int lengthX = grid.GetLength(0);
        int lengthY = grid.GetLength(1);
        
        for (int y = 0; y < lengthY; y++)
        {
            for (int x = 0; x < lengthX; x++)
            {
                var c = grid[x, y] switch
                {
                    0 => '.',
                    1 => '#',
                    2 => 'X',
                    _ => '?'
                };
                
                Console.Write(c);
            }
            
            Console.WriteLine();
        }
    }
}