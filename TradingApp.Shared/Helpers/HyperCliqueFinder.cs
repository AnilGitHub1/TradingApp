using System.Collections.Concurrent;

namespace TradingApp.Shared.Services
{
  public class HyperCliqueFinder
  {
      /// <summary>
      /// Find all maximal hypercliques from triplets.
      /// </summary>
      public Dictionary<int, List<int[]>> FindAllLevelsFromTriplets(
          List<int[]> triplets,
          out int maxK,
          bool returnMaxOnly = false)
      {
        maxK = 3;

        if (triplets.Count == 0)
          return new Dictionary<int, List<int[]>>();

        // Triplets are already sorted & unique.
        var baseTriplets = triplets;

        // Build pair→nodes map
        var pairMap = BuildPairMap(baseTriplets);

        // Maximal cliques storage
        var maximal = new ConcurrentDictionary<string, int[]>();

        // HashSets for subset pruning
        var maximalSets = new ConcurrentBag<HashSet<int>>();

        // Memo for maximal-only to avoid duplicates
        var visitedMaximal = new ConcurrentDictionary<string, bool>();

        // ---- Helper: Key from int[] ----
        static string KeyOf(int[] arr)
        {
          // arr already sorted
          if (arr.Length == 1) return arr[0].ToString();
          return string.Join("#", arr);
        }

        // ---- Subset of max check ----
        bool IsSubsetOfMax(HashSet<int> currentSet)
        {
          foreach (var maximalSet in maximalSets)
          {
            if(currentSet.IsSubsetOf(maximalSet)) return true;
          }
          return false;
        }

        // ---- Main DFS ----
        void DfsExtend(int[] current)
        {
          // Convert to HashSet for subset check
          var currentSet = new HashSet<int>(current);
          if (IsSubsetOfMax(currentSet)) 
            return;

          int k = current.Length;

          // Build all pair sets
          List<HashSet<int>> pairSets = new List<HashSet<int>>(k * (k - 1) / 2);
          for (int i = 0; i < k; i++)
          {
            for (int j = i + 1; j < k; j++)
            {
              int a = current[i], b = current[j];
              if (a > b) { int t = a; a = b; b = t; }

              if (!pairMap.TryGetValue((a, b), out var s))
                goto CannotExtend;

              pairSets.Add(s);
            }
          }

          int ps = pairSets.Count;
          if (ps == 0)
            goto CannotExtend;

          // sort pairSets by 
          pairSets.Sort((a, b) => a.Count - b.Count);

          var smallest = pairSets[0];

          bool extended = false;

          // Loop over smallest set
          foreach (int cand in smallest)
          {
            // Check if cand belongs to each of the other pairSets
            bool ok = true;
            for (int t = 1; t < ps; t++)
            {
              if (!pairSets[t].Contains(cand))
              {
                ok = false;
                break;
              }
            }
            if (!ok) continue;

            extended = true;

            // Build new clique
            int[] newArr = new int[k + 1];
            Array.Copy(current, newArr, k);
            newArr[k] = cand;

            Array.Sort(newArr);

            DfsExtend(newArr);
          }

          if (!extended)
          {
            string key = KeyOf(current);
            if (visitedMaximal.TryAdd(key, true))
            {
              maximal.TryAdd(key, (int[])current.Clone());
              maximalSets.Add(currentSet);
            }
          }

          return;

        CannotExtend:
          {
            string key = KeyOf(current);
            if (visitedMaximal.TryAdd(key, true))
            {
              maximal.TryAdd(key, (int[])current.Clone());
              maximalSets.Add(currentSet);
            }
          }
        }

        // ---- Run DFS for each base triplet in parallel ----
        foreach(var t in baseTriplets)
        {
          int[] arr = new int[3];
          arr[0] = t[0];
          arr[1] = t[1];
          arr[2] = t[2];

          DfsExtend(arr);
        };

        // ---- Build output ----
        if (maximal.Count == 0)
          return new Dictionary<int, List<int[]>>();

        int localMax = 0;

        // compute maxK without LINQ
        foreach (var kv in maximal)
        {
          int len = kv.Value.Length;
          if (len > localMax) localMax = len;
        }
        maxK = localMax;

        var result = new Dictionary<int, List<int[]>>();

        if (returnMaxOnly)
        {
          // Only return largest cliques
          var list = new List<int[]>();

          foreach (var kv in maximal)
          {
            if (kv.Value.Length == maxK)
              list.Add((int[])kv.Value.Clone());
          }

          result[maxK] = list;
          return result;
        }

        // Return all maximal levels grouped by size
        foreach (var kv in maximal)
        {
          int len = kv.Value.Length;
          if (!result.TryGetValue(len, out var bucket))
          {
            bucket = new List<int[]>();
            result[len] = bucket;
          }
          bucket.Add((int[])kv.Value.Clone());
        }

        return result;
    }

    // ---------------- BUILD PAIR MAP ----------------
    private static Dictionary<(int, int), HashSet<int>> BuildPairMap(List<int[]> triplets)
    {
      var map = new Dictionary<(int, int), HashSet<int>>();

      foreach (var t in triplets)
      {
        int a = t[0], b = t[1], c = t[2];

        int x = a, y = b;
        if (x > y) { int temp = x; x = y; y = temp; }
        AddToMap(map, (x, y), c);

        x = a; y = c;
        if (x > y) { int temp = x; x = y; y = temp; }
        AddToMap(map, (x, y), b);

        x = b; y = c;
        if (x > y) { int temp = x; x = y; y = temp; }
        AddToMap(map, (x, y), a);
      }

      return map;
    }

    private static void AddToMap(Dictionary<(int, int), HashSet<int>> map, (int, int) key, int val)
      {
        if (!map.TryGetValue(key, out var set))
        {
          set = new HashSet<int>();
          map[key] = set;
        }
        set.Add(val);
      }
  }
}
