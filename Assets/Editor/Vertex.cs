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

  public double[] Position
  {
    get
    {
      return new double[3] { Coords.x, Coords.y, Coords.z };
    }
    set
    {
      Coords = new Vector3((float)value[0], (float)value[1], (float)value[2]); 
    }
  }
}
