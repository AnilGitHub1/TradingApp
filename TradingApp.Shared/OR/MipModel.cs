namespace OR
{
  public class MipModel
  {
    private readonly List<MipVariable> _vars = new();
    private readonly List<MipConstraint> _cons = new();
    private int _nextVarId = 0;

    public IReadOnlyList<MipVariable> Variables => _vars;
    public IReadOnlyList<MipConstraint> Constraints => _cons;

    public ObjectiveSense ObjSense { get; private set; } = ObjectiveSense.Maximize;
    public Dictionary<int, double> ObjectiveCoeffs { get; } = new();

    public MipVariable AddVariable(string name, double lower = double.NegativeInfinity, double upper = double.PositiveInfinity, MipVarType type = MipVarType.Continuous, double objCoeff = 0.0)
    {
      var v = new MipVariable(_nextVarId++, name, lower, upper, type) { ObjCoeff = objCoeff };
      _vars.Add(v);
      if (Math.Abs(objCoeff) > 0) ObjectiveCoeffs[v.Id] = objCoeff;
      return v;
    }

    public MipConstraint AddConstraint(string name, ConstraintSense sense, double rhs)
    {
      var c = new MipConstraint(name, sense, rhs);
      _cons.Add(c);
      return c;
    }

    public void SetObjective(ObjectiveSense sense, Dictionary<int, double> coeffs)
    {
      ObjSense = sense;
      ObjectiveCoeffs.Clear();
      foreach (var kv in coeffs) ObjectiveCoeffs[kv.Key] = kv.Value;
    }
  }
}