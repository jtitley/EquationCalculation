
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace TargetNumberSolver
{
    class Program
    {
        static readonly int target = 257;
        static readonly List<char> ops = new() { '+', '-', '*', '/' };
        static readonly HashSet<string> solutions = new();

        static void Main(string[] args)
        {
            var digits = new int[] { 6, 1, 3, 5, 7, 3 };
            var perms = digits.Permutations();

            var sb = new StringBuilder();

            Parallel.ForEach(perms, perm =>
            {
                var digitList = perm.ToList();
                Evaluate(digitList, target);

                lock (solutions)
                {
                    foreach (var solution in solutions)
                    {
                        sb.AppendLine(solution);
                    }
                }
            });

            Console.WriteLine("Solutions:");
            foreach (var solution in solutions)
            {
                Console.WriteLine(solution);
            }
        }

        private static void Evaluate(List<int> digits, int target)
        {
            var ops = new List<char> { '+', '-', '*', '/' };
            var opFuncs = new Dictionary<char, Func<double, double, double>>()
            {
                { '+', (a, b) => a + b },
                { '-', (a, b) => a - b },
                { '*', (a, b) => a * b },
                { '/', (a, b) => b == 0 ? double.NaN : a / b }
            };
    
            var allNumberCombinations = GetAllNumberCombinations(digits);

            foreach (var numbers in allNumberCombinations)
            {
                foreach (var opComb in GetCombinations(ops, numbers.Count - 1))
                {
                    var expr = new StringBuilder(numbers[0].ToString());
                    double sum = numbers[0];

                    for (int i = 1; i < numbers.Count; i++)
                    {
                        var op = opComb[i - 1];
                        var num = numbers[i];
                        expr.Append(op).Append(num);

                        sum = opFuncs[op](sum, num);
                    }

                    var equation = expr.ToString();

                    Console.WriteLine($": {expr}");
                    
                    if (double.IsNaN(sum))
                    {
                        continue;
                    }                    

                    if (sum == target)
                    {
                        solutions.Add(equation);
                        Console.WriteLine(equation);
                    }
                }

                /* Now go for all possible parenthesis combinations.
                   We could generate all possible parenthetical combinations for each equation and evaluate those.
                   This will give us every single possible result that can be achieved with the given digits and operations.
                */              
                var allOperatorCombinations = GetAllOperatorCombinations(numbers.Count - 1);
                foreach (var ops2 in allOperatorCombinations)
                {
                    var allParenthesesCombinations = GetAllParenthesesCombinations(ops2);
                    foreach (var parentheses in allParenthesesCombinations)
                    {
                        var equation = BuildEquation(numbers, ops2, parentheses);
                        int result = EvaluateEquation(equation);
                        Console.WriteLine($": {equation}");
                        if (result == target)
                        {
                            solutions.Add(equation);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generates all possible combinations of the given set of integers.
        /// </summary>
        /// <param name="digits">The set of integers to generate combinations from.</param>
        /// <returns>A hashset containing all unique combinations.</returns>
        private static HashSet<List<int>> GetAllNumberCombinations(List<int> digits)
        {
            var combinations = new HashSet<List<int>>();
            var temp = new List<int>();
            GenerateCombinations(digits, 0, temp, combinations);
            return combinations;
        }

        private static void GenerateCombinations(List<int> digits, int index, List<int> temp, HashSet<List<int>> combinations)
        {
            if (temp.Count > 0)
            {
                combinations.Add(new List<int>(temp));
            }

            for (int i = index; i < digits.Count; i++)
            {
                temp.Add(digits[i]);
                GenerateCombinations(digits, i + 1, temp, combinations);
                temp.RemoveAt(temp.Count - 1);
            }
        }

        // Generate all possible combinations of all possible numbers
        private static List<List<int>> GetCombinations(int[] digits, int length)
        {
            if (length == 0) return new List<List<int>> { new List<int>() };
            if (!digits.Any()) return new List<List<int>>();

            var first = digits.First();
            var rest = digits.Skip(1).ToArray();

            var withoutFirst = GetCombinations(rest, length);
            var withFirst = GetCombinations(rest, length - 1).Select(c => new List<int> { first }.Concat(c).ToList()).ToList();

            return withoutFirst.Concat(withFirst).ToList();
        }

        // Generate all possible combinations of basic left-to-right operations
        private static List<string> GetCombinations(List<char> ops, int length)
        {
            if (length == 0) return new List<string> { string.Empty };
            if (!ops.Any()) return new List<string>();

            var first = ops.First();
            var rest = ops.Skip(1).ToList();

            var withoutFirst = GetCombinations(rest, length);
            var withFirst = GetCombinations(rest, length - 1).Select(c => first + c).ToList();

            return withoutFirst.Concat(withFirst).ToList();
        }


        /*
         * This function will generate all possible combinations of the four operators (+, -, *, /) for a given number of digits. 
         * Remember, the number of operators is always one less than the number of digits in the equation.
         */
        private static List<List<char>> GetAllOperatorCombinations(int numOperators)
        {
            var allOperatorCombinations = new List<List<char>>();
            int numCombinations = (int)Math.Pow(4, numOperators);

            for (int i = 0; i < numCombinations; i++)
            {
                var currentCombination = new List<char>();
                int x = i;
                for (int j = 0; j < numOperators; j++)
                {
                    int opIndex = x % 4;
                    x /= 4;
                    currentCombination.Add(ops[opIndex]);
                }
                allOperatorCombinations.Add(currentCombination);
            }

            return allOperatorCombinations;
        }

        /*
         * This function will generate all the different ways we can place parentheses in an equation. 
         * We'll have to be careful here to only generate valid combinations 
         * (e.g., every open parenthesis needs a corresponding close parenthesis).
         */
        private static List<List<int>> GetAllParenthesesCombinations(List<char> ops)
        {
            var allParenthesesCombinations = new List<List<int>>();
            int numOps = ops.Count;

            for (int i = 0; i < Math.Pow(2, numOps); i++)
            {
                var currentCombination = new List<int>();
                for (int j = 0; j < numOps; j++)
                {
                    if ((i & (1 << j)) != 0)
                    {
                        currentCombination.Add(j);
                    }
                }

                // Validate the parentheses combination
                if (IsValidParenthesesCombination(currentCombination))
                {
                    allParenthesesCombinations.Add(currentCombination);
                }
            }

            return allParenthesesCombinations;
        }

        // This function will take the digits, the operators, and the parentheses positions, and assemble them into a full-fledged equation.
        private static string BuildEquation(List<int> digits, List<char> ops, List<int> parentheses)
        {
            StringBuilder equation = new();
            Stack<int> openParentheses = new();

            for (int i = 0; i < digits.Count; i++)
            {
                // Add opening parenthesis if needed
                if (parentheses.Contains(i))
                {
                    equation.Append('(');
                    openParentheses.Push(i);
                }

                // Add the digit
                equation.Append(digits[i]);

                // Add closing parenthesis if we've reached a valid position
                if (openParentheses.Count > 0 && i - openParentheses.Peek() > 1)
                {
                    equation.Append(')');
                    openParentheses.Pop();
                }

                // Add the operator if there's one left
                if (i < ops.Count)
                {
                    equation.Append(ops[i]);
                }
            }

            // Make sure all opening parentheses are closed
            while (openParentheses.Count > 0)
            {
                equation.Append(')');
                openParentheses.Pop();
            }

            return equation.ToString();
        }

        /* This is a cheeky little shortcut that lets us evaluate complex equations, 
         * complete with parentheses and different operators, without having to write a full-fledged equation parser.
         */
        private static int EvaluateEquation(string equation)
        {
            DataTable table = new();
            table.Columns.Add("expression", typeof(string), equation);
            DataRow row = table.NewRow();
            table.Rows.Add(row);
            return (int)Convert.ToDouble(row["expression"]);
        }

        /*
         * The IsValidParenthesesCombination function is essential for filtering out invalid combinations of parentheses. 
         * You want to make sure that each opening parenthesis has a matching closing one and that they make sense mathematically.
        */
        private static bool IsValidParenthesesCombination(List<int> parentheses)
        {
            Stack<int> stack = new();
            foreach (var pos in parentheses)
            {
                if (stack.Count > 0 && pos - stack.Peek() > 1)
                {
                    stack.Pop();
                }
                else
                {
                    stack.Push(pos);
                }
            }
            return stack.Count == 0;
        }


    }

    public static class ExtensionClass
    {
        public static IEnumerable<IEnumerable<T>> Permutations<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return PermutationsImpl(source.ToArray());

            static IEnumerable<IEnumerable<T>> PermutationsImpl(T[] list)
            {
                int n = list.Length;
                if (n == 0)
                    yield return Enumerable.Empty<T>();

                for (int i = 0; i < n; i++)
                {
                    var element = list[i];

                    var rest = list.Take(i).Concat(list.Skip(i + 1)).ToArray();

                    foreach (var permutation in PermutationsImpl(rest))
                    {
                        yield return new[] { element }.Concat(permutation);
                    }
                }
            }
        }
    }

    // A comparer class to help .Distinct() understand how to compare lists
    class ListComparer<T> : IEqualityComparer<List<T>>
    {
        public bool Equals(List<T>? x, List<T>? y)
        {
            if (x == null || y == null)
                return false;
            return x.SequenceEqual(y);
        }

        public int GetHashCode([DisallowNull] List<T> obj)
        {
            int hashcode = 0;
            foreach (T t in obj)
            {
                if (t != null)
                    hashcode ^= t.GetHashCode();
            }
            return hashcode;
        }
    }
}
