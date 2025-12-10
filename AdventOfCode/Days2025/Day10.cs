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
        
        JoltageSolveV4();
        
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
    }
    
    private ushort[] targetState;
    private ushort[] tempState;
    private int maxButtonWeight;
    private ushort[][] buttonArray;
    private Dictionary<long, int> visitedStates;
    private PriorityQueue<(ushort[], int), int> priorityQueue;
    
    private void JoltageSolveV4()
    {
        long sum = 0;
        
        visitedStates = new Dictionary<long, int>();
        priorityQueue = new PriorityQueue<(ushort[], int), int>();

        for (int i = 0; i < joltage.Length; i++)
        {
            var joltages = joltage[i].ToArray();
            var buttons = buttonPresses[i];
            tempState = new ushort[joltages.Length];
            visitedStates.Clear();
            priorityQueue.Clear();
            
            maxButtonWeight = 1;
            foreach(var b in buttons) maxButtonWeight = Math.Max(maxButtonWeight, b.Count);
            
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
        
        while (priorityQueue.Count > 0)
        {
            var item = priorityQueue.Dequeue();
            var currentState = item.Item1;
            int targetDepth = item.Item2 + 1;
            
            if (IsTarget(currentState))
                return targetDepth - 1;
            
            for (int i = 0; i < buttonArray.Length; i++)
            {
                var button = buttonArray[i];
            
                ApplyButton(currentState, button);
                
                int distance = CalculatePriority(tempState);
                
                if (distance == -1)
                    continue;
                
                var hash = HashState(tempState);

                if (visitedStates.TryGetValue(hash, out var cDepth))
                {
                    if (cDepth <= targetDepth)
                        continue;
                    
                    visitedStates[hash] = targetDepth;
                }
                else
                    visitedStates[hash] = targetDepth;
                
                priorityQueue.Enqueue(((ushort[])tempState.Clone(), targetDepth), distance);
            }
        }

        throw new InvalidOperationException();
    }

    private void ApplyButton(ushort[] current, ushort[] button)
    {
        Array.Copy(current, tempState, current.Length);
        
        for (int j = 0; j < button.Length; j++)
        {
            var index = button[j];
            tempState[index] += 1;
        }
    }

    private int CalculatePriority(ushort[] current)
    {
        int maxDiff = 0;
        int sumDiff = 0;

        for (int i = 0; i < current.Length; i++)
        {
            int diff = targetState[i] - current[i];
            if (diff > maxDiff) maxDiff = diff;
            sumDiff += diff;
            
            if (diff < 0)
                return -1;
        }
        
        int stepsByMass = (sumDiff + maxButtonWeight - 1) / maxButtonWeight;
    
        return Math.Max(maxDiff, stepsByMass);
    }
    
    private bool IsTarget(ushort[] current)
    {
        for (int i = 0; i < current.Length; i++)
        {
            if (current[i] != targetState[i])
                return false;
        }

        return true;
    }
    
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

        for (int i = 0; i < targetStates.Length; i++)
        {
            var joltages = joltage[i].ToArray();
            targetState = StateToInt(joltages);
            
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
            
            Console.WriteLine("Buttons: " + string.Join(", ", currentButtons.Select(b => "[" + string.Join("-", b) + "]"))); 
            Console.WriteLine("Target joltage: " + string.Join(", ", joltages));
            
            //Ill just restrict steps and slowly make them higher if that makes sense
            for (int s = 10; ; s += 10)
            {
                bestSteps = s;
                int[] state = new int[joltages.Length];
                visitedStates.Clear();
            
                SearchRec(state, joltages, 0);
                
                if (bestSteps < s)
                    break;
                
                Console.WriteLine("Increasing step limit to " + (s + 10));
            }
            
            Console.WriteLine("Best steps for target " + i + ": " + bestSteps);
            
            sum += bestSteps;
        }
        
        Console.WriteLine("Sum of best steps: " + sum);
    }

    private int bestSteps;
    private long targetState;
    private int[][] currentButtons;
    private HashSet<long> visitedStates;
    
    private void SearchRec(int[] states, int[] target, int steps)
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

            SearchRec(states, target, steps + 1);

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
    }*/
    
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
    long HashState(ushort[] values)
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