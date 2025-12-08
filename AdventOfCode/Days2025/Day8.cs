using System.Numerics;

namespace AdventOfCode.Days2025;

public class Day8 : DayBase
{
    protected override int Day => 8;

    protected override string GetAocInput()
    {
        return base.GetAocInput();

        //return
        //    "162,817,812\n57,618,57\n906,360,560\n592,479,940\n352,342,300\n466,668,158\n542,29,236\n431,825,988\n739,650,466\n52,470,668\n216,146,977\n819,987,18\n117,168,530\n805,96,715\n346,949,466\n970,615,88\n941,993,340\n862,61,35\n984,92,344\n425,690,689";
    }

    public override void Run()
    {
        var lines = GetAocInputAsLines();
        
        Vector3[] points = new Vector3[lines.Length];
        int[] circuits = new int[lines.Length];
        
        for (int i = 0; i < lines.Length; i++)
        {
            var segments = lines[i].Split(',', StringSplitOptions.RemoveEmptyEntries);
            points[i] = new Vector3(
                int.Parse(segments[0]),
                int.Parse(segments[1]),
                int.Parse(segments[2])
            );
        }
        
        HashSet<(int, int)> excluded = new HashSet<(int, int)>();

        int circuitIndex = 1;
        
        int iterationIndex = 0;
        (int, int) lastLink = (-1, -1);
        
        while (true)
        {
            int zeroCount = GetZeroCount(circuits);
            int circuitCount = GetCircuitCount(circuits);
            
            Console.WriteLine($"[{iterationIndex}] Current zero count: " + zeroCount + ", circuit count: " + circuitCount);
            iterationIndex++;
            
            if (zeroCount == 0 && circuitCount == 1)
            {
                Console.WriteLine("Last link: " + lastLink);
                break;
            }
            
            var (index1, index2) = FindClosestLink(points, excluded);
            lastLink = (index1, index2);

            excluded.Add((index1, index2));

            bool circuit1 = circuits[index1] != 0;
            bool circuit2 = circuits[index2] != 0;

            if (circuit1 && circuit2)
            {
                int c1 = circuits[index1];
                int c2 = circuits[index2];

                if (c1 != c2)
                {
                    for (int j = 0; j < circuits.Length; j++)
                    {
                        if (circuits[j] == c2)
                            circuits[j] = c1;
                    }
                }
            }
            else if (circuit1)
            {
                circuits[index2] = circuits[index1];
            }
            else if (circuit2)
            {
                circuits[index1] = circuits[index2];
            }
            else
            {
                circuits[index1] = circuitIndex;
                circuits[index2] = circuitIndex;
            
                circuitIndex++;
            }
        }
        
        int[] circuitCounts = new int[circuitIndex + 1];
        
        for (int i = 0; i < circuits.Length; i++)
        {
            int c = circuits[i];
            circuitCounts[c]++;
        }
        
        //Part 1
        //var sortedCirc = circuitCounts.ToList();
        //sortedCirc.Sort((a, b) => b.CompareTo(a));
        //Console.WriteLine("Top 3 circuit sizes: " + sortedCirc[1] + ", " + sortedCirc[2] + ", " + sortedCirc[3]);
        //long result = (long)sortedCirc[1] * (long)sortedCirc[2] * (long)sortedCirc[3];
        //Console.WriteLine("Result: " + result);
        
        Console.WriteLine("Circuit counts: " + string.Join(", ", circuitCounts));

        var p1 = points[lastLink.Item1];
        var p2 = points[lastLink.Item2];
        var p1X = (long)p1.X;
        var p2X = (long)p2.X;

        var res = p1X * p2X;
        Console.WriteLine("Final Result: " + res);
    }

    private int GetZeroCount(int[] circuits)
    {
        int count = 0;
        
        for (int i = 0; i < circuits.Length; i++)
        {
            if (circuits[i] == 0)
                count++;
        }

        return count;
    }
    
    private int GetCircuitCount(int[] circuits)
    {
        HashSet<int> uniqueCircuits = new HashSet<int>();
        
        for (int i = 0; i < circuits.Length; i++)
        {
            if (circuits[i] != 0)
                uniqueCircuits.Add(circuits[i]);
        }

        return uniqueCircuits.Count;
    }

    private (int a, int b) FindClosestLink(Vector3[] points, HashSet<(int, int)> excluded)
    {
        int cIndex1 = -1;
        int cIndex2 = -1;
        float closestDist = float.MaxValue;
        
        for (var i = 0; i < points.Length; i++)
        {
            for (int j = i + 1; j < points.Length; j++)
            {
                if (excluded.Contains((i, j)) || excluded.Contains((j, i)))
                    continue;
                
                var dist = Vector3.DistanceSquared(points[i], points[j]);

                if (dist <= closestDist)
                {
                    closestDist = dist;
                    cIndex1 = i;
                    cIndex2 = j;
                }
            }
        }
        
        return (cIndex1, cIndex2);
    }
}