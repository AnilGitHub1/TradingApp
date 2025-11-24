// MipApi.cs
// Corresponds to your original Python file at: /mnt/data/cfb130f5-4bbe-4f61-863f-643108eed8d5.py
using Google.OrTools.LinearSolver;

namespace OR
{
    public static class MipSolver
    {
      // Solve a MipModel using OR-Tools CBC
      public static MipModelSolution SolveWithOrTools(MipModel model, int numThreads = 1, int timeLimitMs = 0)
      {
        if (model == null) throw new ArgumentNullException(nameof(model));

        // create solver
        Solver solver = Solver.CreateSolver("CBC_MIXED_INTEGER_PROGRAMMING");
        if (solver == null) throw new InvalidOperationException("OR-Tools solver not available.");

        // map model vars to OR-Tools variables
        var varMap = new Dictionary<int, Variable>();
        foreach (var v in model.Variables)
        {
          Variable orVar;
          switch (v.Type)
          {
            case MipVarType.Binary:
              orVar = solver.MakeIntVar(v.Lower, v.Upper, v.Name);
              break;
            case MipVarType.Integer:
              orVar = solver.MakeIntVar(v.Lower, v.Upper, v.Name);
              break;
            default:
              orVar = solver.MakeNumVar(v.Lower, v.Upper, v.Name);
              break;
          }
          varMap[v.Id] = orVar;
        }

          // add constraints
        foreach (var c in model.Constraints)
        {
          // build linear expression: sum(coeff_i * var_i) [+ penaltyVar*multiplier if present]
          LinearExpr expr = new LinearExpr(); // operator overloads allow 0.0 + var
          foreach (var kv in c.Coeffs)
          {
            var orVar = varMap[kv.Key];
            expr = expr + (kv.Value * orVar);
          }

          if (c.PenaltyVariableId.HasValue)
          {
            var penaltyOrVar = varMap[c.PenaltyVariableId.Value];
            expr = expr + (c.PenaltyMultiplier * penaltyOrVar);
          }

          // create constraint by sense
          switch (c.Sense)
          {
            case ConstraintSense.LessOrEqual:
              solver.Add(expr <= c.Rhs);
              break;
            case ConstraintSense.GreaterOrEqual:
              solver.Add(expr >= c.Rhs);
              break;
            case ConstraintSense.Equal:
              solver.Add(expr == c.Rhs);
              break;
          }
        }

        // objective
        var objective = solver.Objective();
        foreach (var kv in model.ObjectiveCoeffs)
        {
          if (!varMap.TryGetValue(kv.Key, out var orVar)) continue;
          objective.SetCoefficient(orVar, kv.Value);
        }
        if (model.ObjSense == ObjectiveSense.Maximize) objective.SetMaximization();
        else objective.SetMinimization();

        // optional tuning
        solver.SetNumThreads(numThreads);
        if (timeLimitMs > 0) solver.SetTimeLimit(timeLimitMs);

        // solve
        var status = solver.Solve();
        bool isOptimal = status == Solver.ResultStatus.OPTIMAL || status == Solver.ResultStatus.FEASIBLE;
        double objVal = isOptimal ? objective.Value() : double.NaN;

        // read variable values
        var values = new Dictionary<int, double>();
        foreach (var kv in varMap)
        {
          values[kv.Key] = kv.Value.SolutionValue();
        }

        return new MipModelSolution(isOptimal, objVal, values);
      }
    }
}
