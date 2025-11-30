
namespace OR
{
  public class MipConstraint
  {
    public string Name { get; }
    // Linear lhs stored as variable id -> coefficient
    private readonly Dictionary<int, double> _coeffs = new();
    public IReadOnlyDictionary<int, double> Coeffs => _coeffs;
    public ConstraintSense Sense { get; }
    public double Rhs { get; private set; }

    // Optional penalty variable (e.g. for Big-M)
    public int? PenaltyVariableId { get; private set; }
    public double PenaltyMultiplier { get; private set; }

    public MipConstraint(string name, ConstraintSense sense, double rhs)
    {
      Name = name ?? "";
      Sense = sense;
      Rhs = rhs;
    }

    public void AddTerm(int varId, double coeff)
    {
      if (_coeffs.ContainsKey(varId)) _coeffs[varId] += coeff;
      else _coeffs[varId] = coeff;
    }

    public void SetRhs(double rhs) => Rhs = rhs;

    // Optional: attach a penalty variable to model: e.g. turn constraint a*x <= b + M*(1 - z)
    public void AttachPenalty(int penaltyVarId, double multiplier)
    {
      PenaltyVariableId = penaltyVarId;
      PenaltyMultiplier = multiplier;
    }

    public override string ToString()
    {
      var terms = string.Join(" + ", _coeffs.Select(kv => $"{kv.Value}*v{kv.Key}"));
      return $"{Name}: {terms} {Sense} {Rhs} {(PenaltyVariableId.HasValue ? $" + {PenaltyMultiplier}*v{PenaltyVariableId}" : "")}";
    }
  }
}