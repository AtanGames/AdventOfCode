using System.Diagnostics;

namespace AdventOfCode.Days2025;

/// <summary>
/// "The test isnt hard". The test:
/// </summary>
public class Day10 : DayBase
{
    protected override int Day => 10;

    private (int targetMask, int targetCount)[] targetStates;
    private List<List<ushort>>[] buttonPresses;
    private List<ushort>[] joltage;
    
    protected override string GetAocInput()
    {
        return base.GetAocInput();

        return
           "[.##.] (3) (1,3) (2) (2,3) (0,2) (0,1) {3,5,4,7}\n[...#.] (0,2,3,4) (2,3) (0,4) (0,1,2) (1,2,3,4) {7,5,12,7,2}\n[.###.#] (0,1,2,3,4) (0,3,4) (0,1,2,4,5) (1,2) {10,11,11,5,10,5}";
    }

    public override void Run()
    {
        ParseInput();
        //BruteForceSolve();
        
        Stopwatch sw = Stopwatch.StartNew();

        JoltageSolveV8();
        
        sw.Stop();
        
        Console.WriteLine("Elapsed time: " + sw.ElapsedMilliseconds + " ms");
    }
    
    private void ParseInput()
    {
        var lines = GetAocInputAsLines();

        var tmp = lines.ToList();
        tmp.Sort((t1, t2) => t1.Length.CompareTo(t2.Length));
        lines = tmp.ToArray();
        
        targetStates = new (int targetMask, int targetCount)[lines.Length];
        buttonPresses = new List<List<ushort>>[lines.Length];
        joltage = new List<ushort>[lines.Length];

        int maxV = 0;
        int maxVLength = 0;
        int maxButtonCount = 0;
        
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            
            var parts = line.Split(' ');

            var statePart = parts[0];
            statePart = statePart.Replace('[', ' ').Replace(']', ' ');
            statePart = statePart.Replace('#', '1').Replace('.', '0').Trim();

            targetStates[i].targetMask = Convert.ToInt32(statePart, 2);
            targetStates[i].targetCount = statePart.Length;

            buttonPresses[i] = new List<List<ushort>>();
            for (int j = 1; j < parts.Length - 1; j++)
            {
                var list = new List<ushort>();
                var buttonPart = parts[j];
                
                buttonPart = buttonPart.Replace('(', ' ').Replace(')', ' ').Trim();
                var buttonIndices = buttonPart.Split(',');
                
                foreach (var indexStr in buttonIndices)
                {
                    list.Add(ushort.Parse(indexStr));
                }
                
                buttonPresses[i].Add(list);
                
                if (list.Count > maxButtonCount)
                    maxButtonCount = list.Count;
            }
            
            var idkPart = parts[parts.Length - 1];
            
            if (idkPart.Length > maxVLength)
                maxVLength = idkPart.Length;
            
            idkPart = idkPart.Replace('{', ' ').Replace('}', ' ').Trim();
            var idkIndices = idkPart.Split(',');
            joltage[i] = new List<ushort>();
            foreach (var indexStr in idkIndices)
            {
                var v = ushort.Parse(indexStr);
                
                if (v > maxV)
                    maxV = v;
                
                joltage[i].Add(v);
            }
        }
        
        Console.WriteLine("Max joltage value: " + maxV + ", length: " + maxVLength);
        Console.WriteLine("Max button press count: " + maxButtonCount);
    }
    
    /// <summary>
    /// Im in pain
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private void JoltageSolveV8 ()
    {        
        long sum = 0;
        
        List<short[]> solutionsCache = new List<short[]>();
        
        for (int i = 0; i < joltage.Length; i++)
        {
            var buttons = buttonPresses[i].Select(t => t.ToArray()).ToArray();
            var b = joltage[i].ToArray();
            var a = ParseButtonMatrix(buttons, b, b.Length);
            ushort[] xMax = FindMaximums(buttons, b);

            var originalA = (double[,])a.Clone();
            
            //A * x = b
            
            Console.WriteLine("----- Processing target state " + (i + 1) + "/" + targetStates.Length);
            Console.WriteLine("Buttons: " + string.Join(", ", buttons.Select(btn => "[" + string.Join("-", btn) + "]")));
            Console.WriteLine("Target joltage: " + string.Join(", ", b));
            Console.WriteLine("Xmax: " + string.Join(", ", xMax));
            
            DisplayMatrix(a);
            
            Console.WriteLine("Performing Gauss elimination...");
            a = GaussElimination(a);
            
            DisplayMatrix(a);
            var freeVars = GetFreeVariables(a);
            
            Console.WriteLine("Free variables: " + string.Join(", ", freeVars));

            var solutions = new Queue<short[]>();
            var state = new short[buttons.Length];
            for (int j = 0; j < state.Length; j++)
                state[j] = -1;
            
            GetSolutionsRec(state, freeVars, xMax, solutions, 0);
            Console.WriteLine("Found " + solutions.Count + " solutions");

            var solution = SolveLinearSystem(a, xMax, solutions);
            var min = solution.Sum(v => v);
            
            solutionsCache.Add(solution);
            
            Console.WriteLine("Minimum solution sum for target " + (i + 1) + ": " + min);
            sum += min;

            if (!VerifySolution(originalA, solution))
                throw new InvalidOperationException();
        }
        
        Console.WriteLine("--- Solutions summary ---");
        for (int i = 0; i < solutionsCache.Count; i++)
        {
            var solution = solutionsCache[i];
            var s = solution.Sum(v => v);
            
            Console.WriteLine($"[{s}] Target " + (i + 1) + ": " + string.Join(", ", solution));
        }
        
        Console.WriteLine("Sum of steps: " + sum);
    }
    
    private short[] SolveLinearSystem (double[,] a, ushort[] xMax, Queue<short[]> solutions)
    {
        int rows = a.GetLength(0);
        int cols = a.GetLength(1);
        
        int currentMinimumSum = int.MaxValue;
        short[] currentSolution = [];
        
        while (solutions.Count > 0)
        {
            var solution = solutions.Dequeue();
            bool validSolution = true;
            
            for (int r = rows - 1; r >= 0; r--)
            {
                int pivotIndex = -1;
                
                for (int c = 0; c < cols - 1; c++)
                {
                    if (Math.Abs(a[r, c]) > 1e-5)
                    {
                        pivotIndex = c;
                        break;
                    }
                }
                
                if (pivotIndex == -1)
                    continue;
                
                double sum = a[r, cols - 1];
                
                for (int c = pivotIndex + 1; c < cols - 1; c++)
                {
                    sum -= a[r, c] * solution[c];
                }
                
                double xValue = sum / a[r, pivotIndex];
                
                if (xValue < -0.1 || xValue > xMax[pivotIndex])
                {
                    validSolution = false;
                    break;
                }
                
                if (!IsValidInt(xValue, out int intValue))
                {
                    validSolution = false;
                    break;
                }
                
                solution[pivotIndex] = (short)intValue;
            }
            
            if (validSolution && VerifySolution(a, solution))
            {
                int sum = solution.Sum(v => v);
                
                if (sum < currentMinimumSum)
                {
                    currentMinimumSum = sum;
                    currentSolution = solution;
                    Console.WriteLine("Valid solution: " + string.Join(", ", solution));
                }
            }
        }

        return currentSolution;
    }
    
    private bool VerifySolution(double[,] matrix, short[] x)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        
        for (int r = 0; r < rows; r++)
        {
            double sum = 0.0;
            
            for (int c = 0; c < cols - 1; c++)
            {
                sum += matrix[r, c] * x[c];
            }

            if (Math.Abs(sum - matrix[r, cols - 1]) > 1e-5)
                return false;
        }
        
        return true;
    }

    private bool IsValidInt(double value, out int val)
    {
        val = (int)Math.Round(value);

        //Assume it is a integer, because I SPENT ONE HOUR DEBUGGING BECAUSE OF FLOATING POINT PRECISION ISSUES
        return true;
        //return Math.Abs(value - val) < 1e-5;
    }

    private void GetSolutionsRec(short[] state, int[] freeVars, ushort[] xMax, Queue<short[]> solutions, int index)
    {
        if (index >= freeVars.Length)
        {
            solutions.Enqueue((short[])state.Clone());
            return;
        }
        
        var currentVar = freeVars[index];
        var currentMax = xMax[currentVar];
        
        for (short x = 0; x <= currentMax; x++)
        {
            state[currentVar] = x;
            
            GetSolutionsRec(state, freeVars, xMax, solutions,index + 1);
        }
    }
    
    private int[] GetFreeVariables(double[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        
        bool[] isPivotColumn = new bool[cols - 1];
        
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols - 1; c++)
                if (Math.Abs(matrix[r, c]) > 1e-5)
                {
                    isPivotColumn[c] = true;
                    break;
                }
        
        List<int> freeVars = new List<int>();
        
        for (int c = 0; c < cols - 1; c++)
            if (!isPivotColumn[c])
                freeVars.Add(c);
        
        return freeVars.ToArray();
    }
    
    private double[,] GaussElimination(double[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        int pivotRow = 0;
        
        for (int col = 0; col < cols - 1 && pivotRow < rows; col++)
        {
            double largestElement = 0;
            int largestRowIndex = -1;

            for (int r = pivotRow; r < rows; r++)
            {
                if (Math.Abs(matrix[r, col]) > largestElement)
                {
                    largestElement = Math.Abs(matrix[r, col]);
                    largestRowIndex = r;
                }
            }
            
            if (largestRowIndex == -1 || Math.Abs(largestElement) < 1e-5)
                continue;
            
            matrix = SwapRows(matrix, pivotRow, largestRowIndex);

            for (int i = (pivotRow + 1); i < rows; i++)
            {
                double factor = matrix[i, col] / matrix[pivotRow, col];
                
                matrix[i, col] = 0.0;
                
                for (int j = (col + 1); j < cols; j++)
                {
                    matrix[i, j] -= factor * matrix[pivotRow, j];
                }
            }
            
            // Console.WriteLine();
            // DisplayMatrix(matrix);
            // Console.WriteLine();
            pivotRow++;
        }
        
        return matrix;
    }

    /// <summary>
    /// Elementare zeilenoperation nr 1 ofc
    /// </summary>
    private double[,] SwapRows(double[,] matrix, int row1, int row2)
    {
        int cols = matrix.GetLength(1);
        
        for (int c = 0; c < cols; c++)
        {
            (matrix[row1, c], matrix[row2, c]) = (matrix[row2, c], matrix[row1, c]);
        }
        
        return matrix;
    }
    
    private double[,] ParseButtonMatrix(ushort[][] buttons, ushort[] target, int rows)
    {
        var matrix = new double[rows, buttons.Length + 1];
        
        for (int b = 0; b < buttons.Length; b++)
        {
            var button = buttons[b];
            
            for (int j = 0; j < button.Length; j++)
            {
                var index = button[j];
                matrix[index, b] = 1.0;
            }
        }
        
        for (int r = 0; r < rows; r++)
        {
            matrix[r, buttons.Length] = target[r];
        }
        
        return matrix;
    }
    
    private void DisplayMatrix(double[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        int[] w = new int[cols];

        for (int c = 0; c < cols; c++)
        {
            int maxLen = 0;
            for (int r = 0; r < rows; r++)
            {
                int len = matrix[r, c].ToString().Length;
                if (len > maxLen)
                {
                    maxLen = len;
                }
            }
            w[c] = maxLen;
        }

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (c == cols - 1)
                {
                    Console.Write("| ");
                }

                string val = matrix[r, c].ToString();
                Console.Write(val.PadLeft(w[c]) + " ");
            }

            Console.WriteLine();
        }
    }
    
    private ushort[] FindMaximums (ushort[][] buttonArray, ushort[] targetState)
    {
        ushort[] maxPresses = new ushort[buttonArray.Length];

        for (int b = 0; b < buttonArray.Length; b++)
        {
            var button = buttonArray[b];
            
            ushort maxPress = ushort.MaxValue;
            
            for (int j = 0; j < button.Length; j++)
            {
                var index = button[j];
                ushort remain = targetState[index];

                if (remain < maxPress)
                    maxPress = remain;
            }
            
            maxPresses[b] = (ushort)(maxPress + 1);
        }
        
        return maxPresses;
    }
    
    /*private void JoltageSolveV7()
    {
        long sum = 0;

        int threadID = 0;
        
        //Use 95 for performance testing
        int l = Math.Min(999, targetStates.Length);
        for (int i = 0; i < l; i++)
        {
            var joltages = joltage[i].ToArray();
            var buttons = buttonPresses[i].Select(t => t.ToArray()).ToArray();
            var a = ParseButtonMatrix(buttons, joltages.Length);

            Day10_Solver.Solver solver = new Day10_Solver.Solver(joltages.Length, buttons.Length, threadID);
            threadID++;
            
            Console.WriteLine("----- Processing target state " + (i + 1) + "/" + targetStates.Length);
            Console.WriteLine("Buttons: " + string.Join(", ", buttons.Select(b => "[" + string.Join("-", b) + "]")));
            Console.WriteLine("Target joltage: " + string.Join(", ", joltages));

            ushort[] xMax = FindMaximums(buttons, joltages);
            
            var xState = new short[xMax.Length];
            for (var i1 = 0; i1 < xState.Length; i1++)
                xState[i1] = -1;
            
            //DisplayButtonMatrix(a); 
            Console.WriteLine("Xmax: " + string.Join(", ", xMax));
             
            int[] orderArray = new int[joltages.Length];

            for (int r = 0; r < orderArray.Length; r++)
            {
                orderArray[r] = r;
            }
            
            //var indices = GetRowIndicesByVariableCount(a);
            var indices = Enumerable.Range(0, joltages.Length)
                .OrderBy(r => joltages[r])
                .ThenBy(r => GetVariableCountInRow(a, r))
                .ToArray();
            
            Console.WriteLine("Row computation order: " + string.Join(", ", indices));
            
            solver.Solve(a, joltages, xState, xMax, indices, 0, 0, -1);
            
            int solutionSteps = solver.GetMinSolutionSum();
            Console.WriteLine("Minimum solution sum for target " + (i + 1) + ": " + solutionSteps);
            sum += solutionSteps;
        }
        
        Console.WriteLine("Sum of steps: " + sum);
    }*/
    
   /*private void JoltageSolveV6()
    {
        long sum = 0;
        
        List<short[]> solutionsCache = new List<short[]>();
        
        //Use 95 for performance testing
        int l = Math.Min(95, targetStates.Length);
        for (int i = 0; i < l; i++)
        {
            var joltages = joltage[i].ToArray();
            var buttons = buttonPresses[i].Select(t => t.ToArray()).ToArray();
            var a = ParseButtonMatrix(buttons, joltages.Length);
            minSolutionSum = int.MaxValue;
            
            rowSolutionsCache = new Queue<ushort[]>[joltages.Length];
            varChangedCache = new List<int>[joltages.Length];
            limitsCache = new ushort[joltages.Length][];
            indicesCache = new ushort[joltages.Length][];
            for (int r = 0; r < joltages.Length; r++)
            {
                rowSolutionsCache[r] = new Queue<ushort[]>();
                varChangedCache[r] = new List<int>();
                limitsCache[r] = new ushort[buttons.Length];
                indicesCache[r] = new ushort[buttons.Length];
            }
            
            Console.WriteLine("----- Processing target state " + (i + 1) + "/" + targetStates.Length);
            Console.WriteLine("Buttons: " + string.Join(", ", buttons.Select(b => "[" + string.Join("-", b) + "]")));
            Console.WriteLine("Target joltage: " + string.Join(", ", joltages));

            ushort[] xMax = FindMaximums(buttons, joltages);
            
            var xState = new short[xMax.Length];
            for (var i1 = 0; i1 < xState.Length; i1++)
                xState[i1] = -1;
            
            //DisplayButtonMatrix(a); 
            Console.WriteLine("Xmax: " + string.Join(", ", xMax));
            
            //var indices = GetRowIndicesByVariableCount(a);
            var indices = Enumerable.Range(0, joltages.Length)
                .OrderBy(r => joltages[r])
                .ThenBy(r => GetVariableCountInRow(a, r))
                .ToArray();
            
            Console.WriteLine("Row computation order: " + string.Join(", ", indices));
            
            Solver(a, joltages, xState, xMax, indices, 0, 0);
            
            Console.WriteLine("Minimum solution sum for target " + (i + 1) + ": " + minSolutionSum);
            sum += minSolutionSum;
            
            solutionsCache.Add(bestSolution);
        }
        
        Console.WriteLine("--- Solutions summary ---");
        for (int i = 0; i < solutionsCache.Count; i++)
        {
            var solution = solutionsCache[i];
            Console.WriteLine("Target " + (i + 1) + ": " + string.Join(", ", solution));
        }
        
        Console.WriteLine("Sum of steps: " + sum);
    }
    
    private int minSolutionSum;
    private short[] bestSolution;
    private Queue<ushort[]>[] rowSolutionsCache;
    private List<int>[] varChangedCache;
    private ushort[][] limitsCache;
    private ushort[][] indicesCache;
    
    private readonly System.Diagnostics.Stopwatch progressWatch = System.Diagnostics.Stopwatch.StartNew();
    
    private void Solver(bool[][] a, ushort[] b, short[] xState, ushort[] xMax, int[] rowOrder, int r, int currentSum)
    {
        if (currentSum >= minSolutionSum)
            return;
        
        if (r >= rowOrder.Length)
        {
            if (xState.Any(v => v == -1))
                throw new InvalidOperationException();
            
            Console.WriteLine("Found new min solution: " + string.Join(", ", xState));
            minSolutionSum = currentSum;
            bestSolution = (short[])xState.Clone();
            
            return;
        }
        
        if (r == 0)
            Console.WriteLine("Starting solver recursion");;
        
        var rowIndex = rowOrder[r];
            
        //var row = GetRow(a, rowIndex);
        var targetValue = b[rowIndex];
            
        //OutputLinearEquation(row, targetValue);
            
        int rowLength = a[0].Length;
        
        ushort[] limits = limitsCache[r];
        for (int vindex = 0; vindex < rowLength; vindex++)
        {
            var v = GetRowValue(a, rowIndex, vindex);
            limits[vindex] = v ? xMax[vindex] : (ushort)0;
        }
            
        var solutionCache = rowSolutionsCache[r];
        solutionCache.Clear();
        ushort[] indices = indicesCache[r];

        for (int i = 0; i < indices.Length; i++)
            indices[i] = 0;
        
        RecursiveLinearSolver(limits, indices, xState, targetValue, solutionCache, 0, minSolutionSum, 0);
        
        if (r == 0)
            Console.WriteLine("Found " + solutionCache.Count + " solutions");
        
        //int initialCount = solutions.Count;

        List<int> varChanged = varChangedCache[rowIndex];

        int rootSolutionsProcessed = 0;
        int totalSolutions = solutionCache.Count;
        
        while (solutionCache.Count > 0)
        {
            var solution = solutionCache.Dequeue();
            varChanged.Clear();
            
            if (r == 0)
            {
                rootSolutionsProcessed++;

                if (progressWatch.ElapsedMilliseconds >= 1000)
                {
                    Console.WriteLine(
                        "Progress: " + rootSolutionsProcessed + " / " + totalSolutions +
                        " (" + (rootSolutionsProcessed * 100.0 / totalSolutions).ToString("F1") + "%)"
                    );

                    progressWatch.Restart();
                }
            }
            
            for (int j = 0; j < rowLength; j++)
            {
                var val = solution[j];
                var wasSearched = GetRowValue(a, rowIndex, j);
                
                if (wasSearched)
                {
                    if (xState[j] == -1)
                    {
                        xState[j] = (short)val;
                        currentSum += val;
                        varChanged.Add(j);
                    }
                    else if (xState[j] != val)
                    {
                        Console.WriteLine("Conflict on x" + j + ": existing " + xState[j] + ", new " + val);
                        throw new InvalidOperationException();
                    }
                }
            }
            
            Solver(a, b, xState, xMax, rowOrder, r + 1, currentSum);

            foreach (var index in varChanged)
            {
                currentSum -= xState[index];
                xState[index] = -1;
            }
        }
    }
    
    private void RecursiveLinearSolver(ushort[] limits, ushort[] indices, short[] xState, int targetResult, Queue<ushort[]> solutions, int depth, int maxSum, int currentSum)
    {
        if (currentSum >= maxSum)
            return;
        
        if (depth == limits.Length)
        {
            int result = 0;
            for (int i = 0; i < limits.Length; i++)
                result += indices[i];
            
            if (result == targetResult)
            {
                //Console.WriteLine("Found solution: " + string.Join(", ", indices));
                solutions.Enqueue((ushort[])indices.Clone());
            }
            
            return;
        }

        var x = xState[depth];
        var limit = limits[depth];

        if (x == -1)
        {
            for (ushort i = 0; i <= limit; i++)
            {
                indices[depth] = i;
                RecursiveLinearSolver(limits, indices, xState, targetResult,solutions, depth + 1, maxSum, currentSum + i);
            }
        }
        else
        {
            var val = (ushort)((limit == 0) ? 0 : x);
            indices[depth] = val;
            
            RecursiveLinearSolver(limits, indices, xState, targetResult,solutions, depth + 1, maxSum, currentSum + x);
        }
    }
    
    private void OutputLinearEquation(bool[] row, ushort targetResult)
    { 
        List<string> terms = new List<string>();
        
        for (int i = 0; i < row.Length; i++)
        {
            if (row[i])
            {
                terms.Add("x" + i);
            }
        }
        
        string equation = string.Join(" + ", terms) + " = " + targetResult;
        
        Console.WriteLine("Solving: " + equation);
    }
    
    private int GetVariableCountInRow (bool[][] matrix, int rowIndex)
    {
        var row = GetRow(matrix, rowIndex);
        int count = row.Count(b => b);
        return count;
    }
    
    private bool GetRowValue (bool[][] matrix, int rowIndex, int colIndex)
    {
        return matrix[rowIndex][colIndex];
    }
    
    private bool[] GetRow(bool[][] matrix, int rowIndex)
    {
        int cols = matrix[0].Length;
        bool[] row = new bool[cols];
        
        for (int c = 0; c < cols; c++)
        {
            row[c] = matrix[rowIndex][c];
        }
        
        return row;
    }
    
    private bool[][] ParseButtonMatrix(ushort[][] buttons, int rows)
    {
        bool[]?[] matrix = new bool[rows][];
        
        for (int b = 0; b < buttons.Length; b++)
        {
            var button = buttons[b];
            
            for (int j = 0; j < button.Length; j++)
            {
                var index = button[j];
                
                if (matrix[index] == null)
                    matrix[index] = new bool[buttons.Length];
                
                matrix[index]![b] = true;
            }
        }
        
        return matrix!;
    }
    
    private void DisplayButtonMatrix(bool[][] matrix)
    {
        int rows = matrix.Length;
        int cols = matrix[0].Length;

        for (int c = 0; c < cols; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                Console.Write(matrix[r][c] ? "1 " : "0 ");
            }
            Console.WriteLine();
        }
    }
    
    private ushort[] FindMaximums (ushort[][] buttonArray, ushort[] targetState)
    {
        ushort[] maxPresses = new ushort[buttonArray.Length];

        for (int b = 0; b < buttonArray.Length; b++)
        {
            var button = buttonArray[b];
            
            ushort maxPress = ushort.MaxValue;
            
            for (int j = 0; j < button.Length; j++)
            {
                var index = button[j];
                ushort remain = targetState[index];

                if (remain < maxPress)
                    maxPress = remain;
            }
            
            maxPresses[b] = maxPress;
        }
        
        return maxPresses;
    }
    
    //Nevermind, way to slow
    /*private void JoltageSolveV5()
    {
        /* Algorithm that I came up with that will definetly work and not blow up
         * based on A * x = b where A is the button matrix, x is the number of presses per button, and b is the target joltage
         * 
         * 1. Define maximums for all xn
         * 2. Seperate into seperate linear equations and solve them
         * 3. Cross check solutions to filter out invalid ones
         * 4. Brute force through all solution combinations
        
        
        long sum = 0;
        
        for (int i = 0; i < joltage.Length; i++)
        {
            var joltages = joltage[i].ToArray();
            var buttons = buttonPresses[i].Select(t => t.ToArray()).ToArray();
            var a = ParseButtonMatrix(buttons, joltages.Length);
            DisplayButtonMatrix(a); 
            
            Console.WriteLine("----- Processing target state " + (i + 1) + "/" + targetStates.Length);
            Console.WriteLine("Buttons: " + string.Join(", ", buttons.Select(b => "[" + string.Join("-", b) + "]")));
            Console.WriteLine("Target joltage: " + string.Join(", ", joltages));

            ushort[] xMax = FindMaximums(buttons, joltages);
            
            Console.WriteLine("Xmax: " + string.Join(", ", xMax));

            SolveLinearConstraints(a, xMax, joltages);
        }
        
        Console.WriteLine("Sum of steps: " + sum);
    }

    private bool[] GetRow(bool[,] matrix, int rowIndex)
    {
        int cols = matrix.GetLength(1);
        bool[] row = new bool[cols];
        
        for (int c = 0; c < cols; c++)
        {
            row[c] = matrix[rowIndex, c];
        }
        
        return row;
    }
    
    private bool[,] ParseButtonMatrix(ushort[][] buttons, int rows)
    {
        var matrix = new bool[rows, buttons.Length];
        
        for (int b = 0; b < buttons.Length; b++)
        {
            var button = buttons[b];
            
            for (int j = 0; j < button.Length; j++)
            {
                var index = button[j];
                matrix[index, b] = true;
            }
        }
        
        return matrix;
    }
    
    private void DisplayButtonMatrix(bool[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Console.Write(matrix[r, c] ? "1 " : "0 ");
            }
            Console.WriteLine();
        }
    }

    private void SolveLinearConstraints(bool[,] a, ushort[] xMax, ushort[] b)
    {
        int rows = a.GetLength(0);
        for (int rIndex = 0; rIndex < rows; rIndex++)
        {
            var row = GetRow(a, rIndex);
            
            Console.WriteLine("Solving row " + rIndex + ": " + string.Join(", ", row.Select(b => b ? "1" : "0")));
            
            var solutions = FindSolutions(row, xMax, b[rIndex]);
            
            Console.WriteLine("Found " + solutions.Count + " solutions for row " + rIndex);
        }
    }
    
    private List<ushort[]> FindSolutions(bool[] inputMask, ushort[] xMax, ushort targetValue)
    {
        List<ushort[]> solutions = new List<ushort[]>();
        ushort[] recLoopArray = new ushort[inputMask.Length];
        
        for (int vindex = 0; vindex < inputMask.Length; vindex++)
        {
            var v = inputMask[vindex];
            
            var maxLength = xMax[vindex];

            recLoopArray[vindex] = v ? maxLength : (ushort)0;
        }

        ushort[] indices = new ushort[inputMask.Length];
        RecursiveLinearSolver(recLoopArray, indices, targetValue, solutions, 0);
        
        return solutions;
    }

    private void RecursiveLinearSolver(ushort[] limits, ushort[] indices, int targetResult, List<ushort[]> solutions, int depth)
    {
        if (depth == limits.Length)
        {
            // int result = 0;
            // for (int i = 0; i < limits.Length; i++)
            //     result += indices[i];
            //
            // if (result == targetResult)
            // {
            //     //Console.WriteLine("Found solution: " + string.Join(", ", indices));
            //     solutions.Add((ushort[])indices.Clone());
            // }
            //
            // return;
            
            return;
        }

        for (ushort i = 0; i <= limits[depth]; i++)
        {
            indices[depth] = i;
            RecursiveLinearSolver(limits, indices, targetResult,solutions, depth + 1);
        }
    }
    
    private ushort[] FindMaximums (ushort[][] buttonArray, ushort[] targetState)
    {
        ushort[] maxPresses = new ushort[buttonArray.Length];

        for (int b = 0; b < buttonArray.Length; b++)
        {
            var button = buttonArray[b];
            
            ushort maxPress = ushort.MaxValue;
            
            for (int j = 0; j < button.Length; j++)
            {
                var index = button[j];
                ushort remain = targetState[index];

                if (remain < maxPress)
                    maxPress = remain;
            }
            
            maxPresses[b] = maxPress;
        }
        
        return maxPresses;
    }*/
    
    /*private ushort[] targetState;
    private int maxButtonWeight;
    private ushort[][] buttonArray;
    private HashSet<long> visitedStates;
    private PriorityQueue<(ushort[], int), int> priorityQueue;

    private void JoltageSolveV4()
    {
        long sum = 0;
        
        visitedStates = new HashSet<long>();
        priorityQueue = new PriorityQueue<(ushort[], int), int>();
        
        for (int i = 0; i < joltage.Length; i++)
        {
            var joltages = joltage[i].ToArray();
            var buttons = buttonPresses[i];
            visitedStates.Clear();
            priorityQueue.Clear();

            maxButtonWeight = 1;

            foreach (var b in buttons) maxButtonWeight = Math.Max(maxButtonWeight, b.Count);
            
            Console.WriteLine("----- Processing target state " + i + "/" + targetStates.Length);
            Console.WriteLine("Buttons: " + string.Join(", ", buttons.Select(b => "[" + string.Join("-", b) + "]")));
            Console.WriteLine("Target joltage: " + string.Join(", ", joltages));
            
            targetState = joltages;
            buttonArray = buttons.Select(t => t.ToArray()).ToArray();
            
            var solutionSteps = SolveRecOptFinalNoP360Max();

            Console.WriteLine("Steps for target " + i + ": " + solutionSteps);

            sum += solutionSteps;
        }
        
        Console.WriteLine("Sum of steps: " + sum);
    }


    private int SolveRecOptFinalNoP360Max()
    {
        priorityQueue.Enqueue((new ushort[targetState.Length], 0), 0);
        
        int minSteps = int.MaxValue;
        
        while (priorityQueue.Count > 0)
        {
            var item = priorityQueue.Dequeue();
            var currentState = item.Item1;
            int targetDepth = item.Item2 + 1;
            
            if (targetDepth >= minSteps)
                continue;
            
            for (int i = 0; i < buttonArray.Length; i++)
            {
                var button = buttonArray[i];
                
                var nState = ApplyButton(currentState, button);

                int distance = CalculatePriority(nState);
                
                if (distance + targetDepth >= minSteps)
                    continue;
                
                if (distance == -1)
                    continue;
                
                if (distance == 0)
                {
                    if (targetDepth < minSteps)
                    {
                        minSteps = targetDepth;
                        Console.WriteLine("New min steps: " + minSteps);
                    }
                    
                    break;
                }
                
                if (visitedStates.Add(HashState(nState)))
                    priorityQueue.Enqueue((nState, targetDepth), distance);
            }
        }
        
        return minSteps;
    }
    
    private ushort[] ApplyButton(ushort[] current, ushort[] button)
    {
        ushort[] newState = (ushort[])current.Clone();

        for (int j = 0; j < button.Length; j++)
        {
            var index = button[j];

            newState[index] += 1;
        }
        
        return newState;
    }

    private int CalculatePriority(ushort[] current)
    {
        int maxDiff = 0;
        int sumDiff = 0;
        
        for (int i = 0; i < current.Length; i++)
        {
            int diff = targetState[i] - current[i];

            if (diff > maxDiff) 
                maxDiff = diff;
            
            if (diff < 0)
                return -1;
            
            sumDiff += diff;
        }
        
        int stepsByMass = (sumDiff + maxButtonWeight - 1) / maxButtonWeight;

        return Math.Max(maxDiff, stepsByMass);
    }*/
    
    /*private int CalculatePriority(ushort[] current)
    {
        int distance = 0;

        for (int i = 0; i < targetState.Length; i++)
        {
            int remain = targetState[i] - current[i];

            if (remain > 0)
                distance += remain;

            if (remain < 0)
                return -1;
        }

        return distance;
    }*/
    
    /*private long targetState;
    private int[][] currentButtons;
    private int[] targetStateArray;
    private volatile int bestSteps;
    
    private void JoltageSolveV3()
    {
        long sum = 0;

        for (int i = 0; i < targetStates.Length; i++)
        {
            var joltages = joltage[i].ToArray();
            targetState = StateToInt(joltages);
            targetStateArray = joltages;
            
            buttonPresses[i].Sort((a, b) => b.Count - a.Count);
            currentButtons = buttonPresses[i].Select(t => t.ToArray()).ToArray();
            
            var buttonScores = new int[currentButtons.Length];

            for (int b = 0; b < currentButtons.Length; b++)
            {
                var button = currentButtons[b];
                int score = 0;

                for (int j = 0; j < button.Length; j++)
                {
                    int index = button[j];
                    score += joltages[index];
                }

                buttonScores[b] = score;
            }
            
            Array.Sort(buttonScores, currentButtons);
            Array.Reverse(currentButtons);
            
            Console.WriteLine("----- Processing target state " + i + "/" + targetStates.Length);
            Console.WriteLine("Buttons: " + string.Join(", ", currentButtons.Select(b => "[" + string.Join("-", b) + "]"))); 
            Console.WriteLine("Target joltage: " + string.Join(", ", joltages));
            
            bestSteps = int.MaxValue;
            
            var tasks = new List<Task>();
            for (int j = 0; j < currentButtons.Length; j++)
            {
                int jIndex = j;
                tasks.Add(Task.Run(() => SolveRec(new int[joltages.Length], 0, 0, jIndex)));
            }

            while (tasks.Any(t => !t.IsCompleted))
            {
                Task.Delay(1000).Wait();
                
                Console.Write(".");
            }
            
            Console.WriteLine();
            Console.WriteLine("Best steps for target " + i + ": " + bestSteps);
            
            sum += bestSteps;
        }
        
        Console.WriteLine("Sum of best steps: " + sum);
    }

    private void SolveRec (int[] states, int btnIndex, int steps, int threadId)
    {
        if (steps >= bestSteps)
            return;
        
        var state = StateToInt(states);
        
        if (state == targetState && steps < bestSteps)
        {
            bestSteps = steps;
            Console.WriteLine("New best steps: " + bestSteps + " (thread " + threadId + ")");
        }
        
        if (btnIndex >= currentButtons.Length)
            return;
        
        var buttonIndex = (btnIndex + threadId) % currentButtons.Length;
        var button = currentButtons[buttonIndex];
        var maxPress = GetMaxPress(states, button);

        for (int c = maxPress; c >= 0; c--)
        {
            for (int j = 0; j < button.Length; j++)
            {
                var index = button[j];
                states[index] += c;
            }

            SolveRec(states, btnIndex + 1, steps + c, threadId);

            for (int j = 0; j < button.Length; j++)
            {
                var index = button[j];
                states[index] -= c;
            }
        }
    }

    private int GetMaxPress(int[] state, int[] btn)
    {
        int maxPress = int.MaxValue;

        for (int i = 0; i < btn.Length; i++)
        {
            int index = btn[i];
            int remain = targetStateArray[index] - state[index];
            
            if (remain < maxPress)
                maxPress = remain;
        }

        return maxPress;
    }*/
    
    //DFS with pruning is also too slow? And Ive spent 4 hours already?
    /*private void JoltageSolveV2()
    {
        long sum = 0;
        visitedStates = new HashSet<long>();
        
        List<int[]> solutions = new List<int[]>();

        for (int i = 0; i < 37; i++)
        {
            var joltages = joltage[i].Select(t => (int)t).ToArray();
            targetState = StateToInt(joltages);
            
            buttonPresses[i].Sort((a, b) => b.Count - a.Count);
            currentButtons = buttonPresses[i].Select(t => t.Select(f => (int)f).ToArray()).ToArray();
            
            var buttonScores = new int[currentButtons.Length];

            for (int b = 0; b < currentButtons.Length; b++)
            {
                var button = currentButtons[b];
                int score = 0;

                for (int j = 0; j < button.Length; j++)
                {
                    int index = button[j];
                    score += joltages[index];
                }

                buttonScores[b] = score;
            }
            
            Array.Sort(buttonScores, currentButtons);
            Array.Reverse(currentButtons);
            
            Console.WriteLine("Buttons: " + string.Join(", ", currentButtons.Select(b => "[" + string.Join("-", b) + "]"))); 
            Console.WriteLine("Target joltage: " + string.Join(", ", joltages));
            
            //Ill just restrict steps and slowly make them higher if that makes sense
            for (int s = 10; ; s += 10)
            {
                bestSteps = s;
                int[] state = new int[joltages.Length];
                visitedStates.Clear();
            
                int[] indices = new int[currentButtons.Length];
                
                SearchRec(state, indices,joltages, 0);
                
                if (bestSteps < s)
                    break;
                
                Console.WriteLine("Increasing step limit to " + (s + 10));
            }
            
            Console.WriteLine("Best steps for target " + i + ": " + bestSteps);
            
            sum += bestSteps;
            
            solutions.Add(solution);
        }
        
        Console.WriteLine("--- Solutions summary ---");
        for (int i = 0; i < solutions.Count; i++)
        {
            var solution = solutions[i];
            var s = solution.Sum();
            
            Console.WriteLine($"[{s}] Target " + (i + 1) + ": " + string.Join(", ", solution));
        }
        
        Console.WriteLine("Sum of best steps: " + sum);
    }

    private int bestSteps;
    private long targetState;
    private int[][] currentButtons;
    private HashSet<long> visitedStates;

    private int[] solution;
    
    private void SearchRec(int[] states, int[] indices, int[] target, int steps)
    {
        if (steps >= bestSteps)
            return;
        
        var state = StateToInt(states);
        
        if (!visitedStates.Add(state))
            return;

        if (AbandonPath(states, target, steps))
            return;
        
        if (state == targetState)
        {
            if (steps < bestSteps)
            {
                bestSteps = steps;
                Console.WriteLine("New best steps: " + bestSteps);
                solution = (int[])indices.Clone();
            }

            return;
        }
        
        for (int b = 0; b < currentButtons.Length; b++)
        {
            var button = currentButtons[b];

            for (int j = 0; j < button.Length; j++)
            {
                var index = button[j];
                states[index] += 1;
            }
            
            indices[b] += 1;

            SearchRec(states, indices, target, steps + 1);
            
            indices[b] -= 1;
            
            for (int j = 0; j < button.Length; j++)
            {
                var index = button[j];
                states[index] -= 1;
            }
        }
    }
    
    private bool AbandonPath(int[] states, int[] target, int steps)
    {
        int maxRemain = 0;
        
        for (int i = 0; i < target.Length; i++)
        {
            int remain = target[i] - states[i];

            if (remain < 0)
                return true;

            if (remain > maxRemain)
                maxRemain = remain;
        }

        if (bestSteps == int.MaxValue)
            return false;

        int lowerBound = steps + maxRemain;

        return lowerBound >= bestSteps;
    }
    
    //Oh BFS is too slow? And ive spent 2 hours already?
    /*private void JoltageSolve()
    {
        long sum = 0;

        List<ushort[]> searchStates = new List<ushort[]>();
        List<ushort[]> cacheStates = new List<ushort[]>();
        
        for (int i = 0; i < targetStates.Length; i++)
        {
            var buttons = buttonPresses[i];
            
            maxIncrementPerPress = 0;
            for (int b = 0; b < buttons.Count; b++)
            {
                if (buttons[b].Count > maxIncrementPerPress)
                    maxIncrementPerPress = buttons[b].Count;
            }
            
            var joltages = joltage[i].ToArray();
            
            Console.WriteLine("----- Processing target state " + i + "/" + targetStates.Length);
            Console.WriteLine("Buttons: " + string.Join(", ", buttons.Select(b => "[" + string.Join("-", b) + "]")));
            Console.WriteLine("Target joltage: " + string.Join(", ", joltages));
            
            HashSet<long> visitedStates = new HashSet<long>();
            int joltageCount = joltages.Length;
            ushort[] tempStates = new ushort[joltageCount];
            long solutionState = HashState(joltages);
            
            bool found = false;
            int result = 0;
            
            for (int depth = 99999; ; depth += 10)
            {
                maxSteps = depth;
                int step = 0;
                Console.WriteLine();
                Console.WriteLine("Searching with depth limit: " + depth);
                
                searchStates.Clear();
                cacheStates.Clear();
                cacheStates.Add(new ushort[joltages.Length]);
                
                visitedStates.Clear();
                
                while (step < depth)
                {
                    step++;
                    
                    (searchStates, cacheStates) = (cacheStates, searchStates);
                    cacheStates.Clear();

                    for (var i1 = searchStates.Count - 1; i1 >= 0; i1--)
                    {
                        var currentState = searchStates[i1];
                        
                        for (int b = 0; b < buttons.Count; b++)
                        {
                            Array.Copy(currentState, tempStates, joltageCount);
                            var button = buttons[b];
                            
                            for (int j = 0; j < button.Count; j++)
                            {
                                var index = button[j];
                                
                                tempStates[index] += 1;
                            }
                            
                            if (AbandonPath(tempStates, joltages, step))
                                continue;
                            
                            var state = HashState(tempStates);
                            
                            if (state == solutionState)
                            {
                                found = true;
                                break;
                            }
                            
                            if (!visitedStates.Add(state))
                                continue;
                            
                            cacheStates.Add((ushort[])tempStates.Clone());
                        }
                        
                        if (found)
                            break;
                    }
                    
                    if (step % 10 == 0)
                        Console.WriteLine("Step " + step + ", states in cache: " + cacheStates.Count);
                    
                    if (found || cacheStates.Count == 0)
                        break;
                }
                
                if (found)
                {
                    result = step;
                    break;
                }
            }
            
            Console.WriteLine();
            Console.WriteLine("Steps for target " + i + ": " + result);
            sum += result;
        }
        
        Console.WriteLine("Sum of steps: " + sum);
    }

    private int maxSteps;
    private int maxIncrementPerPress;
    
    private bool AbandonPath(ushort[] states, ushort[] target, int steps)
    {
        int maxRemain = 0;
        int sumRemain = 0;
        
        for (int i = 0; i < target.Length; i++)
        {
            int remain = target[i] - states[i];

            if (remain < 0)
                return true;

            if (remain > maxRemain)
                maxRemain = remain;
            
            sumRemain += remain;
        }
        
        int h1 = maxRemain;

        int h2 = 0;
        
        if (sumRemain > 0)
            h2 = (sumRemain + maxIncrementPerPress - 1) / maxIncrementPerPress;

        int lowerBound = steps + Math.Max(h1, h2);

        return lowerBound > maxSteps;
    }*/
    
    /// <summary>
    /// Ai generated hash function
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    long StateToInt(int[] values)
    {
        unchecked
        {
            long hash = 1469598103934665603L;

            for (int i = 0; i < values.Length; i++)
            {
                hash ^= (long)values[i] + 1;
                hash *= 1099511628211L;

                hash ^= hash >> 32;
            }

            return hash;
        }
    }

    //Part 1
    /*private void BruteForceSolve()
    {
        long sum = 0;
        
        List<List<ushort>> targetButtons = new List<List<ushort>>();
        
        for (var i = 0; i < targetStates.Length; i++)
        {
            var targetState = targetStates[i];
            var buttons = buttonPresses[i];

            int count = buttons.Count;
            int max = 1 << count;

            int minPresses = int.MaxValue;
            
            for (int mask = 1; mask < max; mask++)
            {
                targetButtons.Clear();

                for (int j = 0; j < count; j++)
                {
                    if ((mask & (1 << j)) != 0)
                    {
                        targetButtons.Add(buttons[j]);
                    }
                }
                
                if (minPresses <= targetButtons.Count)
                    continue;

                if (CheckState(targetState, targetButtons))
                {
                    minPresses = targetButtons.Count;
                }
            }

            sum += minPresses;
            
            Console.WriteLine("Min presses for target " + i + ": " + minPresses);
        }
        
        Console.WriteLine("Sum of minimum presses: " + sum);
    }

    private bool CheckState((int targetMask, int targetCount) targetState, List<List<ushort>> operations)
    {
        int currentState = 0;

        foreach (var operation in operations)
        {
            foreach (var index in operation)
            {
                currentState ^= (1 << (targetState.targetCount - index - 1));
            }
        }

        return currentState == targetState.targetMask;
    }*/
}