namespace AdventOfCode.Days2025;

public class Day11 : DayBase
{
    protected override int Day => 11;
    
    private Dictionary<int, int[]> nodes;
    private Dictionary<string, int> nameToId;
    private List<(int, int[])> sortedNodes;
    
    protected override string GetAocInput()
    {
        return base.GetAocInput();

        return
            "svr: aaa bbb\naaa: fft\nfft: ccc\nbbb: tty\ntty: ccc\nccc: ddd eee\nddd: hub\nhub: fff\neee: dac\ndac: fff\nfff: ggg hhh\nggg: out\nhhh: out";
    }

    public override void Run()
    {
        ParseInput();
        TopoSortGraph();
        OutputSortedGraph();
        TopoSolver();
        //Solver();
    }

    private void ParseInput()
    {
        nameToId = new Dictionary<string, int>();
        nodes = new Dictionary<int, int[]>();
        nameToId.Add("you", 0);
        
        var lines = GetAocInputAsLines();
        int idIndex = 1;
        
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            
            var segments = line.Split(": ", StringSplitOptions.RemoveEmptyEntries);
            var name = segments[0];
            var parts = segments[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);
            
            if (!nameToId.TryGetValue(name, out var id))
            {
                id = idIndex;
                nameToId.Add(name, id);
                idIndex++;
            }
            
            int[] childIds = new int[parts.Length];
            
            for (int p = 0; p < parts.Length; p++)
            {
                var partName = parts[p];
                
                if (!nameToId.TryGetValue(partName, out var partId))
                {
                    partId = idIndex;
                    nameToId.Add(partName, partId);
                    idIndex++;
                }
                
                childIds[p] = partId;
            }
            
            nodes.Add(id, childIds);
        }
    }

    private void TopoSortGraph()
    {
        sortedNodes = new List<(int, int[])>();
        
        foreach (var (key, value) in nodes)
            sortedNodes.Add((key, value));
        
        sortedNodes.Add((nameToId["out"], []));
        
        bool progress = false;

        do
        {
            progress = false;

            for (var i = sortedNodes.Count - 1; i >= 0; i--)
            {
                var (nodeId, childIds) = sortedNodes[i];
                
                for (int j = i + 1; j < sortedNodes.Count; j++)
                {
                    var (_, otherChilds) = sortedNodes[j];
                    
                    if (otherChilds.Contains(nodeId))
                    {
                        sortedNodes.RemoveAt(i);
                        sortedNodes.Insert(j, (nodeId, childIds));
                        progress = true;
                        break;
                    }
                }
            }
        } while (progress);
    }
    
    private void OutputSortedGraph()
    {
        Console.WriteLine("Topologically sorted graph:");
        
        foreach (var (nodeId, childIds) in sortedNodes)
        {
            var name = nameToId.First(kv => kv.Value == nodeId).Key;
            var childNames = childIds.Select(id => nameToId.First(kv => kv.Value == id).Key).ToArray();
            Console.WriteLine($"{name}: {string.Join(" ", childNames)}");
        }
    }

    private void TopoSolver()
    {
        var node1 = "svr";
        var node2 = "fft";
        var node3 = "dac";
        var node4 = "out";
        
        int node1Index = GetSortedIndex(nameToId[node1]);
        int node2Index = GetSortedIndex(nameToId[node2]);
        int node3Index = GetSortedIndex(nameToId[node3]);
        int node4Index = GetSortedIndex(nameToId[node4]);
        
        long node1Paths = TopoSolverV2(node1Index, node2Index);
        long node2Paths = TopoSolverV2(node2Index, node3Index);
        long node3Paths = TopoSolverV2(node3Index, node4Index);
        
        long totalPaths = node1Paths * node2Paths * node3Paths;
        
        Console.WriteLine($"Total paths from {node1} to {node4}: {totalPaths}");
    }

    private long TopoSolverV2(int startIndex, int targetIndex)
    {
        int nodeCount = targetIndex - startIndex + 1;
        long[] paths = new long[nodeCount];
        paths[0] = 1;
        
        for (var i = startIndex; i < targetIndex; i++)
        {
            Console.WriteLine("Processing node index " + i + " / " + targetIndex);
            
            int nodeIndex = i - startIndex;
            var existingPaths = paths[nodeIndex];
            
            (int _, int[] children) = sortedNodes[i];
            
            for (var j = 0; j < children.Length; j++)
            {
                var childIndex = GetSortedIndex(children[j]);
                
                if (childIndex > targetIndex)
                    continue;
                
                int childNodeIndex = childIndex - startIndex;
                paths[childNodeIndex] += existingPaths;
            }
        }
        
        return paths[nodeCount - 1];
    }

    private int TopoSolverRec(int index, int targetIndex, int depth)
    {
        (int id, int[] children) = sortedNodes[index];

        int sum = 0;
        
        for (var i = 0; i < children.Length; i++)
        {
            var childIndex = GetSortedIndex(children[i]);
            
            if (childIndex == targetIndex)
            {
                sum++;
                continue;
            }
            
            if (childIndex > targetIndex)
                continue;
            
            if (childIndex < index)
                throw new Exception("Invalid topological order");
            
            if (depth < 5)
            {
                var spaces = new string(' ', depth * 2);
                Console.WriteLine(spaces + $"At depth {depth}, processing child {i}/{children.Length}");
            }
            
            sum += TopoSolverRec(childIndex, targetIndex, depth + 1);
        }
        
        return sum;
    }

    private int GetSortedIndex(int nodeId)
    {
        for (int i = 0; i < sortedNodes.Count; i++)
        {
            var (nId, _) = sortedNodes[i];
            
            if (nId == nodeId)
                return i;
        }

        return -1;
    }
    
    /// <summary>
    /// DFS
    /// </summary>
    private void Solver()
    {
        var startNode = "fft";
        var targetNode = "dac";
        
        outNodeId = nameToId["out"];
        dacNodeId = nameToId["dac"];
        fftNodeId = nameToId["fft"];
        
        var startNodeId = nameToId[startNode];
        var targetNodeId = nameToId[targetNode];
        
        visitedEdges = new HashSet<(int origin, int target)>();
        
        var pathCount = FindPathCountRec(startNodeId, targetNodeId, false, false, 0);
        
        Console.WriteLine("Total path count from" + startNode + " to " + targetNode + ": " + pathCount);
    }

    private int outNodeId;
    private int dacNodeId;
    private int fftNodeId;

    private HashSet<(int origin, int target)> visitedEdges;
    
    private int FindPathCountRec(int nodeId, int targetId, bool dac, bool fft, int depth)
    {
        if (nodeId == dacNodeId)
            dac = true;
        if (nodeId == fftNodeId)
            fft = true;
        
        if (!nodes.TryGetValue(nodeId, out var childs))
            return 0;

        int sum = 0;

        for (var i = 0; i < childs.Length; i++)
        {
            var childId = childs[i];

            if (childId == targetId)
            {
                //Console.WriteLine("Found path to out at depth " + depth);
                sum++;

                continue;
            }

            if (!visitedEdges.Add((nodeId, childId)))
                continue;
            
            if (depth < 10)
            {
                var spaces = new string(' ', depth * 2);
                Console.WriteLine(spaces + $"At depth {depth}, processing child {i}/{childs.Length}");
            }
            
            sum += FindPathCountRec(childId, targetId, dac, fft, depth + 1);

            visitedEdges.Remove((nodeId, childId));
        }

        return sum;
    }
}