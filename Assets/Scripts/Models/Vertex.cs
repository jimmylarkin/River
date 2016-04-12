using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using MIConvexHull;

namespace GrumpyDev.EndlessRiver
{
  [DebuggerDisplay("{Id} ({Position3D})")]
  public class Vertex : IVertex
  {
    public int Xsegment { get; set; }
    public int ZSegment { get; set; }
    public int Id { get; set; }
    public int Index { get; set; }
    public Vector3 Position3D { get; set; }
    public Color Color { get; set; }

    public Vertex()
    {
      Id = -1;
    }

    //required for triangulation algorithm to work, maybe one day I will change that to Vector 2 (or even 3).
    public double[] Position
    {
      get
      {
        return new double[2] { Position3D.x, Position3D.z };
      }
      set
      {
        Position3D = new Vector3((float)value[0], Position3D.y, (float)value[1]);
      }
    }
  }
}