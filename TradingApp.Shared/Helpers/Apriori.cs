using System.Collections.Concurrent;

namespace TradingApp.Shared.Helpers
{
  public static class Apriori
  {
    // Given prevOrder (sorted int[] lists) and prevHash (keys), produce next-level sets (k = prevK+1)
    // parallelVerify: if true, candidate verification runs in parallel
    private static List<int[]> GenerateNextOrderTrendlines(List<int[]> prevOrder, HashSet<string> prevHash, bool parallelVerify)
    {
        int prevK = prevOrder.Count > 0 ? prevOrder[0].Length : 0;
        if (prevK < 1) return new List<int[]>();
        int nextK = prevK + 1;
        var groups = new Dictionary<string, List<int[]>>();
        // Group by first (k-2) elements (for prevK==3, k-2 = 1)
        int prefixLen = prevK - 1; // k-2
        foreach (var s in prevOrder)
        {
            var prefix = string.Join('#', s.Take(prefixLen));
            if (!groups.TryGetValue(prefix, out var list)) { list = new List<int[]>(); groups[prefix] = list; }
            list.Add(s);
        }

        // Generate candidate keys (dedupe with HashSet)
        var candidateKeys = new HashSet<string>();
        var candidates = new List<int[]>();

        foreach (var kv in groups)
        {
            var list = kv.Value;
            int m = list.Count;
            // pairwise combine list items: they share first (k-2) elements by grouping
            for (int i = 0; i < m; i++)
            {
                for (int j = i + 1; j < m; j++)
                {
                    // union of two sorted arrays of length prevK, they differ only in last positions generally
                    // For safety, we'll create merged array and sort (cost is small compared to overall checks)
                    int[] merged = new int[nextK];
                    // copy elements from first
                    Array.Copy(list[i], 0, merged, 0, prevK);
                    // copy last element from second's tail (or just append then sort)
                    merged[prevK] = list[j][prevK - 1];
                    Array.Sort(merged);
                    string kkey = KeyOf(merged);
                    if (candidateKeys.Add(kkey))
                        candidates.Add(merged);
                }
            }
        }

        if (candidates.Count == 0) return new List<int[]>();

        var validated = new ConcurrentBag<int[]>();

        Action<int[]> verifyOne = candidate =>
        {
            // For candidate of length nextK, ensure every (nextK) subsets of length prevK exist in prevHash
            bool ok = true;
            // generate each subset by skipping one element
            for (int skip = 0; skip < nextK; skip++)
            {
                // build subset key quickly
                // avoid allocations by using a small array
                int idx = 0;
                var subset = new int[prevK];
                for (int t = 0; t < nextK; t++)
                {
                    if (t == skip) continue;
                    subset[idx++] = candidate[t];
                }
                string subkey = KeyOf(subset);
                if (!prevHash.Contains(subkey)) { ok = false; break; }
            }
            if (ok) validated.Add(candidate);
        };

        if (parallelVerify)
        {
            Parallel.ForEach(candidates, verifyOne);
        }
        else
        {
            foreach (var c in candidates) verifyOne(c);
        }

        return validated.ToList();
    }
    public static Dictionary<int, List<int[]>> FindAllOrdersTrendlinesApriori(List<int[]> triplets, out int maxK, bool parallelVerify = true)
    {
        var levels = new Dictionary<int, List<int[]>>();
        maxK = 3;
        if (triplets == null || triplets.Count() == 0) return levels;

        levels[3] = triplets; 

        // build hash for prevOrder membership check
        var prevOrder = triplets;
        var prevHash = new HashSet<string>(prevOrder.Select(KeyOf));
        int k = 3;
        while (true)
        {
            var nextOrder = GenerateNextOrderTrendlines(prevOrder, prevHash, parallelVerify);
            if (nextOrder == null || nextOrder.Count == 0) break;
            k++;
            levels[k] = nextOrder;
            maxK = k;
            // prepare for next iteration
            prevOrder = nextOrder;
            prevHash = new HashSet<string>(prevOrder.Select(KeyOf));
        }

        return levels;
    }
    private static string KeyOf(int[] arr) => string.Join('#', arr);
  }
}