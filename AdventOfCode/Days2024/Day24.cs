using System.Diagnostics;

namespace AdventOfCode.Days2024;

public class Day24 : DayBase
{
    protected override int Day => 24;
    protected override int Year => 2024;

    private long ExpectedResultOrigin => 48063513640678;
    
    private Dictionary<string, byte> _variables;
    private List<(string varA, string varB, Operation op, string res)> _operations;

    private byte[] _varCache;
    private byte[] _varTemp;
    private ResultSave[] _resultCache;
    
    public override void Run()
    {
        LoadInput();

        OptSolver();
    }

    private void LoadInput()
    {
        var lines = GetAocInputAsLines();
        
        //var testInput =
        //    "x00: 1\nx01: 0\nx02: 1\nx03: 1\nx04: 0\ny00: 1\ny01: 1\ny02: 1\ny03: 1\ny04: 1\n\nntg XOR fgs -> mjb\ny02 OR x01 -> tnw\nkwq OR kpj -> z05\nx00 OR x03 -> fst\ntgd XOR rvg -> z01\nvdt OR tnw -> bfw\nbfw AND frj -> z10\nffh OR nrd -> bqk\ny00 AND y03 -> djm\ny03 OR y00 -> psh\nbqk OR frj -> z08\ntnw OR fst -> frj\ngnj AND tgd -> z11\nbfw XOR mjb -> z00\nx03 OR x00 -> vdt\ngnj AND wpb -> z02\nx04 AND y00 -> kjc\ndjm OR pbm -> qhw\nnrd AND vdt -> hwm\nkjc AND fst -> rvg\ny04 OR y02 -> fgs\ny01 AND x02 -> pbm\nntg OR kjc -> kwq\npsh XOR fgs -> tgd\nqhw XOR tgd -> z09\npbm OR djm -> kpj\nx03 XOR y03 -> ffh\nx00 XOR y04 -> ntg\nbfw OR bqk -> z06\nnrd XOR fgs -> wpb\nfrj XOR qhw -> z04\nbqk OR frj -> z07\ny03 OR x01 -> nrd\nhwm AND bqk -> z03\ntgd XOR rvg -> z12\ntnw OR pbm -> gnj";
        //lines = testInput.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        
        _variables = new Dictionary<string, byte>();
        _operations = new List<(string varA, string varB, Operation op, string res)>();
        
        foreach (var line in lines)
        {
            if (line.Contains("->"))
            {
                var parts = line.Split("->", StringSplitOptions.TrimEntries);
                
                var expr = parts[0];
                var resultVar = parts[1];
                
                var exprParts = expr.Split(' ', StringSplitOptions.TrimEntries);
                var op = ParseOperation(exprParts[1]);
                var varA = exprParts[0];
                var varB = exprParts[2];
                
                _operations.Add((varA, varB, op, resultVar));
            }
            else if (line.Contains(':'))
            {
                var parts = line.Split(':', StringSplitOptions.TrimEntries);
                
                var varName = parts[0];
                var value = byte.Parse(parts[1]);
                
                _variables[varName] = value;
            }
        }
    }
    
    /// <summary>
    /// I know its messy
    /// </summary>
    private void OptSolver()
    {
        bool progress;

        List<(bool done, string varA, string varB, Operation op, string res)> opCache =
            new List<(bool done, string varA, string varB, Operation op, string res)>();
        List<int> opOrder = new List<int>();
        var varCache = new Dictionary<string, byte>(_variables);
        
        foreach (var valueTuple in _operations)
        {
            opCache.Add((false, valueTuple.varA, valueTuple.varB, valueTuple.op, valueTuple.res));
        }
        
        Console.WriteLine("Starting operation load...");;
        
        do
        {
            progress = false;
            
            for (var i = opCache.Count - 1; i >= 0; i--)
            {
                if (opCache[i].done)
                    continue;
                
                var varA = opCache[i].varA;
                var varB = opCache[i].varB;
                
                if (!varCache.ContainsKey(varA) ||
                    !varCache.ContainsKey(varB))
                    continue;
                
                var valA = varCache[varA];
                var valB = varCache[varB];
                
                var result = ComputeOperation(valA, valB, opCache[i].op);
                
                varCache[opCache[i].res] = result;
                
                opCache[i] = (true, varA, varB, opCache[i].op, opCache[i].res);
                opOrder.Add(i);
                
                progress = true;
            }
            
            Console.WriteLine("Step completed with " + opOrder.Count + " operations loaded.");
        } while (progress);
        
        Console.WriteLine("Order of operations loaded.");

        int[] opOrderArray = opOrder.ToArray();
        
        int varIndex = 0;

        List<byte> variableCacheList = new List<byte>();
        List<OptOperation> optOperations = new List<OptOperation>();
        Dictionary<string, int> varToIndex = new Dictionary<string, int>();
        
        //Setup inputs
        foreach (var (key, value) in _variables)
        {
            varToIndex[key] = varIndex;
            variableCacheList.Add(value);
            varIndex++;
        }
        
        //Go over operations
        for (var i = 0; i < opOrderArray.Length; i++)
        {
            var op = opCache[opOrderArray[i]];
            
            int varAIndex = varToIndex[op.varA];
            int varBIndex = varToIndex[op.varB];
            
            if (!varToIndex.ContainsKey(op.res))
            {
                varToIndex[op.res] = varIndex;
                variableCacheList.Add(0);
                varIndex++;
            }
            
            int resultIndex = varToIndex[op.res];
            
            optOperations.Add(new OptOperation
            {
                VarAIndex = varAIndex,
                VarBIndex = varBIndex,
                Op = op.op,
                ResultIndex = resultIndex
            });
        }
        
        //Setup result
        
        List<ResultSave> resultCacheList = new List<ResultSave>();
        
        long expected = GetExpectedResult();
        string binary = Convert.ToString(expected, 2);

        for (int i = 0; i < binary.Length; i++)
        {
            var val = binary[binary.Length - 1 - i] == '1' ? (byte)1 : (byte)0;
            
            string varName = "z" + i.ToString("D2");
            
            if (!varToIndex.TryGetValue(varName, out var varIdx))
                throw new InvalidOperationException();

            resultCacheList.Add(new ResultSave
            {
                VarIndex = varIdx,
                Value = val
            });
        }
        
        _resultCache = resultCacheList.ToArray();
        
        //Now its fast enough to brute force (hopefully)

        OptOperation[] opArray = optOperations.ToArray();
        _varCache = variableCacheList.ToArray();
        _varTemp = new byte[_varCache.Length];
        
        Console.WriteLine("Operations loaded.");

        long controlCount = 0;
        int numCount = opArray.Length;
        
        for (int i = 0; i < numCount - 1; i++)
        {
            for (int j = i + 1; j < numCount; j++)
            {
                for (int k = j + 1; k < numCount - 1; k++)
                {
                    for (int l = k + 1; l < numCount; l++)
                    {
                        for (int m = l + 1; m < numCount - 1; m++)
                        {
                            for (int n = m + 1; n < numCount; n++)
                            {
                                for (int o = n + 1; o < numCount - 1; o++)
                                {
                                    for (int p = o + 1; p < numCount; p++)
                                    {
                                        controlCount++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        Console.WriteLine("Total combinations to check: " + controlCount + " with count " + numCount);
        
        Stopwatch sw = new Stopwatch();
        
        sw.Start();
        
        int iterations = 100_000;
        for (int i = 0; i < iterations; i++)
        {
            SolverIter(opArray);
        }
        
        sw.Stop();
        
        double avgMicroseconds = (sw.Elapsed.TotalMilliseconds * 1000.0) / iterations;
        Console.WriteLine($"Avg time per check ≈ {avgMicroseconds:F4} µs");
        
        Console.WriteLine("Computation finished in " + sw.ElapsedMilliseconds + " ms");
    }

    private void SolverIter(OptOperation[] operations)
    {
        Buffer.BlockCopy(_varCache, 0, _varTemp, 0, _varCache.Length);
        
        for (var i = 0; i < operations.Length; i++)
        {
            var operation = operations[i];
            
            var valA = _varTemp[operation.VarAIndex];
            var valB = _varTemp[operation.VarBIndex];
            
            var result = ComputeOperation(valA, valB, operation.Op);
            
            _varTemp[operation.ResultIndex] = result;
        }
        
        //Check result

        for (int i = 0; i < _resultCache.Length; i++)
        {
            var resSave = _resultCache[i];
            
            if (_varTemp[resSave.VarIndex] != resSave.Value)
                return;
        }
        
        Console.WriteLine("Found matching result!");
    }
    
    private long GetExpectedResult()
    {
        long num1 = 0;
        long num2 = 0;
        
        foreach (var (key, value) in _variables)
        {
            if (key.StartsWith('x'))
            {
                var index = int.Parse(key.Substring(1));
                num1 += (long)value << index; 
            }
            else if (key.StartsWith('y'))
            {
                var index = int.Parse(key.Substring(1));
                num2 += (long)value << index; 
            }
        }
        
        return num1 + num2;
    }

    private void SimpleSolver()
    {
        bool progress;
        
        do
        {
            progress = false;
            
            for (var i = _operations.Count - 1; i >= 0; i--)
            {
                if (!_variables.ContainsKey(_operations[i].varA) ||
                    !_variables.ContainsKey(_operations[i].varB))
                    continue;
                
                var valA = _variables[_operations[i].varA];
                var valB = _variables[_operations[i].varB];
                
                var result = ComputeOperation(valA, valB, _operations[i].op);
                
                _variables[_operations[i].res] = result;
                
                _operations.RemoveAt(i);
                
                progress = true;
            }
            
            Thread.Sleep(100);
        } while (progress);
        
        Console.WriteLine("Computation finished");

        long resultNum = 0;
        
        foreach (var (key, value) in _variables)
        {
            if (!key.StartsWith('z'))
                continue;
            
            var index = int.Parse(key.Substring(1));
            resultNum += (long)value << index;
        }
        
        Console.WriteLine("Final result: " + resultNum);
    }

    private byte ComputeOperation(byte val1, byte val2, Operation op)
    {
        return op switch
        {
            Operation.OR => (byte)(val1 | val2),
            Operation.AND => (byte)(val1 & val2),
            Operation.XOR => (byte)(val1 ^ val2),
            _ => throw new InvalidOperationException()
        };
    }
    
    private Operation ParseOperation(string opStr)
    {
        return opStr switch
        {
            "AND" => Operation.AND,
            "OR" => Operation.OR,
            "XOR" => Operation.XOR,
            _ => throw new InvalidOperationException()
        };
    }
    
    private enum Operation
    {
        AND, OR, XOR
    }

    private struct OptOperation
    {
        public int VarAIndex;
        public int VarBIndex;
        public Operation Op;
        public int ResultIndex;
    }

    private struct ResultSave
    {
        public int VarIndex;
        public byte Value;
    }
}