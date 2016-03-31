using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using MIConvexHull;

[DebuggerDisplay("{Id} ({Coords})")]
public class Vertex : IVertex
{
  public int Id { get; set; }
  public Vector3 Coords { get; set; }
  public Vector3 Normal { get; set; }
  public Color Color { get; set; }

  public double[] Position
  {
    get
    {
      return new double[2] { Coords.x, Coords.z };
    }
    set
    {
      Coords = new Vector3((float)value[0], Coords.y, (float)value[1]); 
    }
  }
}
