namespace OR
{
  public class MipVariable
  {
    public int Id { get; }
    public string Name { get; }
    public double Lower { get; private set; }
    public double Upper { get; private set; }
    public MipVarType Type { get; }
    public double ObjCoeff { get; set; } = 0.0;
    public MipVariable(int id, string name, double lower = double.NegativeInfinity, double upper = double.PositiveInfinity, MipVarType type = MipVarType.Continuous)
    {
      Id = id;
      Name = name ?? throw new ArgumentNullException(nameof(name));
      Lower = double.IsNegativeInfinity(lower) ? -1e9 : lower;
      Upper = double.IsPositiveInfinity(upper) ? 1e9 : upper;
      Type = type;
    }
    public void SetBounds(double lower, double upper)
    {
      Lower = lower;
      Upper = upper;
    }
    public override string ToString() => $"{Name}({Type}) [{Lower},{Upper}] obj={ObjCoeff}";
  }

}