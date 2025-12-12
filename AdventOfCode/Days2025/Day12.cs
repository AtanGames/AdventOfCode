namespace AdventOfCode.Days2025;

public class Day12 : DayBase
{
    protected override int Day => 12;

    public Tree[] trees;
    public Present[] presents;
    
    protected override string GetAocInput()
    {
        return base.GetAocInput();

        return
            "0:\n###\n##.\n##.\n\n1:\n###\n##.\n.##\n\n2:\n.##\n###\n##.\n\n3:\n##.\n###\n##.\n\n4:\n###\n#..\n###\n\n5:\n###\n.#.\n###\n\n4x4: 0 0 0 0 2 0\n12x5: 1 0 1 0 2 2\n12x5: 1 0 1 0 3 2";
    }

    public override void Run()
    {
        ParseInput();

        int validTrees = 0;
        
        for (var i = 0; i < trees.Length; i++)
        {
            var tree = trees[i];
            
            var size = tree.SizeX * tree.SizeY;

            int presentSize = 0;

            for (int j = 0; j < tree.Counts.Length; j++)
            {
                var count = tree.Counts[j];
                
                var present = presents[j];
                
                var shape = present.Shape;
                int sizeX = shape.GetLength(0);
                int sizeY = shape.GetLength(1);

                for (int x = 0; x < sizeX; x++)
                {
                    for (int y = 0; y < sizeY; y++)
                    {
                        if (shape[x, y])
                            presentSize += count;
                    }
                }
            }
            
            int remaining = size - presentSize;
            
            Console.WriteLine("Tree " + (i + 1) + ": Size " + size + " Present size: " + presentSize + " Remaining: " + remaining);
            
            if (remaining >= 0)
                validTrees++;
        }
        
        Console.WriteLine("Valid trees: " + validTrees + " / " + trees.Length);
    }

    private void ParseInput()
    {
        var lines = GetAocInput();

        var parts = lines.Split("\n\n");
        
        List<Present> presents = new List<Present>();

        for (int i = 0; i < parts.Length - 1; i++)
        {
            var part = parts[i];
            
            var partLines = part.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            int sizeY = partLines.Length - 1;
            int sizeX = partLines[1].Length;
            bool[,] shape = new bool[sizeX, sizeY];
            
            for (int j = 1; j < partLines.Length; j++)
            {
                var line = partLines[j];
                
                for (int x = 0; x < line.Length; x++)
                {
                    shape[x, j - 1] = line[x] == '#';
                }
            }
            
            presents.Add(new Present
            {
                Shape = shape
            });
        }
        
        this.presents = presents.ToArray();
        
        var treeLines = parts[parts.Length - 1].Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        
        trees = new Tree[treeLines.Length];
        
        for (var i = 0; i < treeLines.Length; i++)
        {
            var line = treeLines[i];
            
            var segments = line.Split(": ");
            var sizeParts = segments[0].Split('x');
            int sizeX = int.Parse(sizeParts[0]);
            int sizeY = int.Parse(sizeParts[1]);
            
            var presentIndices = segments[1].Split(' ');
            ushort[] counts = new ushort[presentIndices.Length];
            for (var j = 0; j < presentIndices.Length; j++)
                counts[j] = ushort.Parse(presentIndices[j]);
            
            trees[i] = new Tree
            {
                SizeX = sizeX,
                SizeY = sizeY,
                Counts = counts
            };
        }
    }
}

public struct Present
{
    public bool[,] Shape;
}

public struct Tree
{
    public int SizeX;
    public int SizeY;

    public ushort[] Counts;
}