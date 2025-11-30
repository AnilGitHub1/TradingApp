namespace OR
{
  public class MipModelSolution
  {
    public bool IsOptimal { get; }
    public double ObjectiveValue { get; }
    public Dictionary<int, double> VariableValues { get; }

    public MipModelSolution(bool isOptimal, double objectiveValue, Dictionary<int, double> values)
    {
      IsOptimal = isOptimal;
      ObjectiveValue = objectiveValue;
      VariableValues = values;
    }
  }
}