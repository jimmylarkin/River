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
    public int Id { get; set; }
    public int Index { get; set; }
    public Vector3 Position3D { get; set; }
    public Color Color { get; set; }
    public List<Vertex> Neighbor { get; set; }
    public List<Triangle<Vertex>> Triangles { get; set; }
    public float CachedEdgeCollapsingCost { get; set; }
    public Vertex CollapseCandidate { get; set; }
    public bool IsOnBoundary { get; set; }

    public bool IsReduced { get; set; }

    public Vertex()
    {
      Triangles = new List<Triangle<Vertex>>();
      Neighbor = new List<Vertex>();
    }

    public void AddNeighbour(Vertex vertex)
    {
      if (vertex.Id == Id)
      {
        return;
      }
      for (int i = 0; i < Neighbor.Count; i++)
      {
        if (Neighbor[i].Id == vertex.Id)
        {
          return;
        }
      }
      Neighbor.Add(vertex);
    }

    public void AddTriangle(Triangle<Vertex> triangle)
    {
      for (int i = 0; i < Triangles.Count; i++)
      {
        if (Triangles[i].Id == triangle.Id)
        {
          return;
        }
      }
      Triangles.Add(triangle);
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