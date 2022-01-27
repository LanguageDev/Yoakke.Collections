using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Yoakke.Collections.Fuzzer;

internal class Program
{
    internal static void FuzzTreeSet<T>(int maxElements)
        where T : ITreeSet, new()
    {
        var rnd = new Random();
        for (var epoch = 0; ; ++epoch)
        {
            if (epoch % 100 == 0) Console.WriteLine($"Epoch {epoch}...");

            var tested = new T();
            var oracle = new HashSet<int>();
            try
            {
                tested.Validate(oracle);
            }
            catch(ValidationException v)
            {
                throw new FuzzerException(v, "<empty>", "ctor");
            }

            while (tested.Count < maxElements)
            {
                var testCase = tested.ToTestCaseString();

                Debug.Assert(tested.Count == oracle.Count);

                var n = rnd.Next(0, maxElements * 4);
                var operation = $"Insert({n})";
                var testedInsert = tested.Insert(n);
                var oracleInsert = oracle.Add(n);
                if (testedInsert != oracleInsert) throw new FuzzerException($"Insertion return value mismatch (oracle: {oracleInsert}, tested: {testedInsert})", testCase, operation);
                try
                {
                    tested.Validate(oracle);
                }
                catch (ValidationException v)
                {
                    throw new FuzzerException(v, testCase, operation);
                }
            }

            while (tested.Count > 0)
            {
                var testCase = tested.ToTestCaseString();

                Debug.Assert(tested.Count == oracle.Count);

                var n = rnd.Next(0, maxElements * 4);
                var operation = $"Delete({n})";
                var testedDelete = tested.Delete(n);
                var oracleDelete = oracle.Remove(n);
                if (testedDelete != oracleDelete) throw new FuzzerException($"Deletion return value mismatch (oracle: {oracleDelete}, tested: {testedDelete})", testCase, operation);
                try
                {
                    tested.Validate(oracle);
                }
                catch (ValidationException v)
                {
                    throw new FuzzerException(v, testCase, operation);
                }
            }
        }
    }

    internal static void Main(string[] args)
    {
#if true
        var tree = new AvlTreeSet
        {
            Root = new AvlTreeNode(41)
            {
                Left = new(20)
                {
                    Left = new(5)
                    {
                        Left = new(1),
                    },
                    Right = new(25)
                    {
                        Left = new(23),
                    },
                },
                Right = new(46)
                {
                    Left = new(45),
                    Right = new(57)
                    {
                        Left = new(53),
                        Right = new(58),
                    },
                },
            }.UpdateHeight()
        };
        tree.Delete(41);
#else
        try
        {
            // FuzzTreeSet<BstTreeSet>(100);
            FuzzTreeSet<AvlTreeSet>(100);
        }
        catch (FuzzerException f)
        {
            Console.WriteLine(f.Message);
        }
#endif
    }
}
