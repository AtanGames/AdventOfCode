namespace AdventOfCode;

public abstract class DayBase
{
    protected virtual int Year => 2025;
    protected abstract int Day { get; }
    
    public abstract void Run();

    protected string[,] GetAocInputAsTable(string separator = " ")
    {
        var lines = GetAocInputAsLines();
        
        int rowCount = lines.Length;
        int colCount = lines[0].Split(separator, StringSplitOptions.RemoveEmptyEntries).Length;
        string[,] table = new string[colCount, rowCount];
        
        for (int y = 0; y < rowCount; y++)
        {
            var line = lines[y];
            var segments = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            
            for (int x = 0; x < colCount; x++)
            {
                table[x, y] = segments[x];
            }
        }
        
        return table;
    }
    
    protected string[] GetAocInputAsLines()
    {
        string input = GetAocInput();
        return input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
    }
    
    protected virtual string GetAocInput()
    {
        FileFetch fetcher = new FileFetch($"https://adventofcode.com/{Year}/day/{Day}/input");
        return fetcher.FetchAsString();
    }
}