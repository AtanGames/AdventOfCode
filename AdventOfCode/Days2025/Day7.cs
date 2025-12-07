using System.Diagnostics;

namespace AdventOfCode.Days2025;

public class Day7 : DayBase
{
    protected override int Day => 7;

    private const byte Empty = 0;
    private const byte Source = 1;
    private const byte Splitter = 2;
    
    //private const byte Beam = 3;

    private int xLength;
    private int yLength;
    
    protected override string GetAocInput()
    { 
        return base.GetAocInput();
        
        return
            ".......S.......\n...............\n.......^.......\n...............\n......^.^......\n...............\n.....^.^.^.....\n...............\n....^.^...^....\n...............\n...^.^...^.^...\n...............\n..^...^.....^..\n...............\n.^.^.^.^.^...^.\n...............";
    }

    public override void Run()
    {
        var lines = GetAocInputAsLines();
        int lengthX = lines[0].Length;
        int lengthY = lines.Length;
        
        long[,] grid = new long[lengthX, lengthY];

        int spitterCount = 0;
        
        for (int y = 0; y < lengthY; y++)
        {
            var line = lines[y];
            
            for (int x = 0; x < lengthX; x++)
            {
                var c = line[x];
                
                grid[x, y] = c switch
                {
                    '^' => Splitter,
                    'S' => Source,
                    _ => Empty
                };
                
                if (c == '^')
                    spitterCount++;
            }
        }
        
        Console.WriteLine("Total splitters: " + spitterCount);
        Console.WriteLine("Dimensions: " + lengthX + " x " + lengthY);
        
        xLength = lengthX;
        yLength = lengthY;

        //OutputGrid(grid);
        
        //RunBeamSolve(grid);
        Stopwatch sw = Stopwatch.StartNew();
        
        var timelineCount = RunBeamSolve(grid);
        
        sw.Stop();
        
        Console.WriteLine("Total timelines activated: " + timelineCount + " in " + sw.ElapsedMilliseconds + "ms");
    }

    /// <summary>
    /// Instead of brute forcing all timelines, we can save the number of timelines using this beam
    /// and do some simple math to calculate how the number of timelines grows on split
    /// </summary>
    /// <param name="grid"></param>
    /// <returns></returns>
    private long RunBeamSolve(long[,] grid)
    {
        for (int y = 0; y < yLength; y++)
        {
            Console.WriteLine("Processing row " + y);;
            
            for (int x = 0; x < xLength; x++)
            {
                var val = grid[x, y];
                
                if (val is Source || IsBeam(val))
                {
                    long tCount = IsBeam(val) ? GetTimelineCount(val) : 1;
                    
                    SetBeam(grid, (x, y + 1), tCount);
                }
            }
        }

        long timelineCount = 0;
        
        int resultY = yLength - 1;
        for (int x = 0; x < xLength; x++)
        {
            var val = grid[x, resultY];
                
            if (IsBeam(val))
            {
                timelineCount += GetTimelineCount(val);
            }
        }
        
        return timelineCount;
    }

    private bool IsBeam(long x) => x > 2;

    private long GetTimelineCount(long x)
    {
        return Math.Max(x - 2, (long)0);
    }

    private long SetTimelineCount(long x)
    {
        return x + 2;
    }
    
    private void SetBeam(long[,] grid, (int x, int y) target, long timelineCount) 
    {
        if (target.x < 0 || target.x >= xLength ||
            target.y < 0 || target.y >= yLength)
            return;
        
        var gridValue = grid[target.x, target.y];
        
        if (gridValue == Empty)
        {
            grid[target.x, target.y] = SetTimelineCount(timelineCount);
        }
        else if (IsBeam(gridValue))
        {
            var existingTimelines = GetTimelineCount(gridValue);
            grid[target.x, target.y] = SetTimelineCount(existingTimelines + timelineCount);
        }
        else if (gridValue == Splitter)
        {
            int rX = target.x + 1;
            int lX = target.x - 1;
            
            SetBeam(grid, (rX, target.y), timelineCount);
            SetBeam(grid, (lX, target.y), timelineCount);
        }
    }
    
    // DOESNT work its to slow :(
    // private int RunTimelineSolver(byte[,] grid, int xPos, int yPos)
    // {
    //     for (int y = yPos; y < yLength; y++)
    //     {
    //         for (int x = xPos; x < xLength; x++)
    //         {
    //             xPos = 0; //Only took 20 min to figure this out...
    //             
    //             var val = grid[x, y];
    //             
    //             if (val is Source or Beam)
    //             {
    //                 int nTimelines = SetBeamRec(grid, (x, y + 1));
    //                 if (nTimelines > 0)
    //                 {
    //                     return nTimelines;
    //                 }
    //             }
    //         }
    //     }
    //
    //     //Console.WriteLine("Reached end of timeline");
    //     //OutputGrid(grid);
    //     
    //     return 1;
    // }
    //
    // private int SetBeamRec(byte[,] grid, (int x, int y) target)
    // {
    //     if (target.x < 0 || target.x >= xLength ||
    //         target.y < 0 || target.y >= yLength)
    //         return 1;
    //     
    //     var gridValue = grid[target.x, target.y];
    //     
    //     if (gridValue == Empty)
    //     {
    //         grid[target.x, target.y] = Beam;
    //         
    //         return SetBeamRec(grid, (target.x, target.y + 1));
    //     }
    //     else if (gridValue == Splitter)
    //     {
    //         int rX = target.x + 1;
    //         int lX = target.x - 1;
    //         
    //         //Console.WriteLine("Splitting timeline into two");
    //         
    //         grid[rX, target.y] = Beam;
    //         
    //         //var timelines = RunTimelineSolverRec(grid, target.x + 1, target.y - 1);
    //         var timelines = SetBeamRec(grid, (rX, target.y + 1));
    //
    //         grid[rX, target.y] = Empty;
    //
    //         int tX = target.x - 1;
    //         int tY = target.y - 1;
    //
    //         for (int y = tY; y < yLength; y++)
    //         {
    //             for (int x = tX; x < xLength; x++)
    //             {
    //                 tX = 0;
    //             
    //                 if (grid[x, y] is Beam)
    //                     grid[x, y] = Empty;
    //                 
    //             }
    //         }
    //         
    //         grid[lX, target.y] = Beam;
    //         
    //         //timelines += RunTimelineSolverRec(grid, target.x + 1, target.y - 1);
    //         timelines += SetBeamRec(grid, (lX, target.y + 1));
    //         
    //         return timelines;
    //     }
    //     
    //     return 0;
    // }
    
    // Part 1
    // private void RunBeamSolve(byte[,] grid)
    // {
    //     int xLength = grid.GetLength(0);
    //     int yLength = grid.GetLength(1);
    //     
    //     for (int y = 0; y < yLength; y++)
    //     {
    //         Console.WriteLine("Processing row " + y);;
    //         
    //         for (int x = 0; x < xLength; x++)
    //         {
    //             var val = grid[x, y];
    //             
    //             if (val is Source or Beam)
    //                 SetBeam(grid, (x, y + 1));
    //         }
    //         
    //         //OutputGrid(grid);
    //     }
    // }

    private void OutputGrid(byte[,] grid)
    {
        int xLength = grid.GetLength(0);
        int yLength = grid.GetLength(1);
        
        for (int y = 0; y < yLength; y++)
        {
            for (int x = 0; x < xLength; x++)
            {
                var c = GetGridChar(grid[x, y]);
                Console.Write(c);
            }
            Console.WriteLine();
        }
    }
    
    private char GetGridChar(byte val)
    {
        return val switch
        {
            Empty => '.',
            Source => 'S',
            Splitter => '^',
            _ => '|',
        };
    }
}