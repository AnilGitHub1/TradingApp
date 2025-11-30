public class SlopeRange
{
  public double Min { get; set; }
  public double Max { get; set; }
  public SlopeRange(double min, double max)
  {
    Min = min;
    Max = max;
  }
  public bool Intersects(SlopeRange other)
  {
    return !(other.Min > Max || other.Max < Min);
  }
  public SlopeRange Intersection(SlopeRange other)
  {
    if (!Intersects(other))
      throw new InvalidOperationException("No intersection between slope ranges.");

    return new SlopeRange(Math.Max(Min, other.Min), Math.Min(Max, other.Max));
  }
}