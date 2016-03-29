using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;


[DebuggerDisplay("{Vertex1} | {Vertex2} | {Vertex3}")]
public class Triangle
{
  public Vertex Vertex1 { get; set; }
  public Vertex Vertex2 { get; set; }
  public Vertex Vertex3 { get; set; }
}
