namespace AdventOfCode;

public abstract class DayBase
{
    protected virtual int Year => 2025;
    protected abstract int Day { get; }
    
    public abstract void Run();
    
    protected string GetAocInput()
    {
        FileFetch fetcher = new FileFetch($"https://adventofcode.com/{Year}/day/{Day}/input");
        return fetcher.FetchAsString();
    }
}