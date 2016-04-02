using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using MIConvexHull;
using UnityEngine;

public class Triangle<TVertex> : TriangulationCell<TVertex, Triangle<TVertex>>
      where TVertex : Vertex
{
  public Vector3 Normal { get; set; }

  public void ReplaceVertex(TVertex oldVertex, TVertex newVertex)
  {
    if (Vertices.Length < 3)
    {
      return;
    }
    if (Vertices[0].Id == oldVertex.Id)
    {
      Vertices[0] = newVertex;
      return;
    }
    if (Vertices[1].Id == oldVertex.Id)
    {
      Vertices[1] = newVertex;
      return;
    }
    if (Vertices[2].Id == oldVertex.Id)
    {
      Vertices[2] = newVertex;
      return;
    }
  }

  public bool HasVertex(TVertex vertex)
  {
    if (Vertices.Length == 3)
    {
      if (Vertices[0].Id == vertex.Id)
      {
        return true;
      }
      if (Vertices[1].Id == vertex.Id)
      {
        return true;
      }
      if (Vertices[2].Id == vertex.Id)
      {
        return true;
      }
    }
    return false;
  }

  public void ComputeNormal()
  {
    if (Vertices.Length == 3)
    {
      var normal = Vector3.Cross(Vertices[1].Position3D - Vertices[0].Position3D, Vertices[2].Position3D - Vertices[0].Position3D);
      normal.Normalize();
      Normal = normal;
    }
    else {
      Normal = Vector3.up;
    }
  }

  public override string ToString()
  {
    if (Vertices.Length == 3)
    {
      return string.Format("Id: {6}; v0: {0} ({1}); v1: {2} ({3}), v2: {4} ({5})",
        Vertices[0].Id, Vertices[0].Position3D, Vertices[1].Id, Vertices[1].Position3D, Vertices[2].Id, Vertices[2].Position3D, Id);
    }
    return "Triangle, no vertices set";
  }
}
