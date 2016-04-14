using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace GrumpyDev.EndlessRiver
{
  public class PolygonReducer
  {
    private void Reduce(ref List<Vertex> vertices, ref List<Triangle<Vertex>> triangles)
    {
      int beforeCount = vertices.Count;
      int targetCount = (int)(beforeCount * 0.6);
      InitCollapse(ref vertices, ref triangles);
      Vertex mn = MinimumCostEdge(ref vertices, ref triangles);
      float currentCost = mn.CachedEdgeCollapsingCost;
      while (vertices.Count > targetCount)
      {
        Collapse(mn, mn.CollapseCandidate, ref vertices, ref triangles);
        mn = MinimumCostEdge(ref vertices, ref triangles);
        currentCost = mn.CachedEdgeCollapsingCost;
      }
      Debug.LogFormat("Reduced from {0} to {1}", beforeCount, vertices.Count);
    }

    private void InitCollapse(ref List<Vertex> vertices, ref List<Triangle<Vertex>> triangles)
    {
      for (int i = 0; i < vertices.Count; i++)
      {
        ComputeEdgeCostAtVertex(vertices[i]);
      }
    }

    private Vertex MinimumCostEdge(ref List<Vertex> vertices, ref List<Triangle<Vertex>> triangles)
    {
      float cost = 1000000;
      Vertex candidate = null;
      for (int i = 0; i < vertices.Count; i++)
      {
        if (!vertices[i].IsReduced && vertices[i].CachedEdgeCollapsingCost < cost)
        {
          candidate = vertices[i];
          cost = vertices[i].CachedEdgeCollapsingCost;
        }
      }
      return candidate;
    }

    private float ComputeEdgeCollapseCost(Vertex u, Vertex v)
    {
      // if we collapse edge uv by moving u to v then how
      // much different will the model change, i.e. the “error”.
      float edgeLength = (v.Position3D - u.Position3D).magnitude;
      float curvature = 0f;

      // find the “sides” triangles that are on the edge uv
      List<Triangle<Vertex>> sides = new List<Triangle<Vertex>>();
      for (int i = 0; i < u.Triangles.Count; i++)
      {
        if (u.Triangles[i].HasVertex(v))
        {
          sides.Add(u.Triangles[i]);
        }
      }

      // use the triangle facing most away from the sides
      // to determine our curvature term
      for (int i = 0; i < u.Triangles.Count; i++)
      {
        float mincurv = 1;
        for (int j = 0; j < sides.Count; j++)
        {
          // use dot product of face normals.
          float dotprod = Vector3.Dot(u.Triangles[i].Normal, sides[j].Normal);
          mincurv = Min(mincurv, (1f - dotprod) / 2.0f);
        }
        curvature = Max(curvature, mincurv);
      }
      return edgeLength * curvature;
    }

    private void ComputeEdgeCostAtVertex(Vertex v)
    {
      if (v.Neighbor.Count == 0)
      {
        v.CollapseCandidate = null;
        v.CachedEdgeCollapsingCost = -0.01f;
        return;
      }
      v.CachedEdgeCollapsingCost = 1000000;
      v.CollapseCandidate = null;
      // search all neighboring edges for “least cost” edge
      for (int i = 0; i < v.Neighbor.Count; i++)
      {
        float c = ComputeEdgeCollapseCost(v, v.Neighbor[i]);
        if (c < v.CachedEdgeCollapsingCost)
        {
          v.CollapseCandidate = v.Neighbor[i]; // v - neighbor[i];
          v.CachedEdgeCollapsingCost = c;
        }
      }
      if (v.IsOnBoundary)
      {
        v.CachedEdgeCollapsingCost = 1000000;
      }
    }

    private void Collapse(Vertex u, Vertex v, ref List<Vertex> vertices, ref List<Triangle<Vertex>> triangles)
    {
      // Collapse the edge uv by moving vertex u onto v
      if (v == null)
      {
        // u is a vertex all by itself so just delete it
        u.IsReduced = true;
        return;
      }
      // make tmp a list of all the neighbors of u
      List<Vertex> tmp = new List<Vertex>(u.Neighbor);
      // delete triangles on edge uv:
      for (int i = u.Triangles.Count - 1; i >= 0; i--)
      {
        if (u.Triangles[i].HasVertex(v))
        {
          triangles.Remove(u.Triangles[i]);  //remove from global list
          u.Triangles.Remove(u.Triangles[i]);  //remove from triangles in vertex
        }
      }
      for (int i = v.Triangles.Count - 1; i >= 0; i--)
      {
        if (v.Triangles[i].HasVertex(u))
        {
          triangles.Remove(v.Triangles[i]);  //remove from global list
          v.Triangles.Remove(v.Triangles[i]);  //remove from triangles in vertex
        }
      }
      // update remaining triangles to have v instead of u
      for (int i = u.Triangles.Count - 1; i >= 0; i--)
      {
        u.Triangles[i].ReplaceVertex(u, v);
      }
      for (int i = v.Triangles.Count - 1; i >= 0; i--)
      {
        v.Triangles[i].ReplaceVertex(u, v);
      }
      vertices.Remove(u);
      for (int i = 0; i < u.Neighbor.Count; i++)
      {
        u.Neighbor[i].Neighbor.Remove(u);
        u.Neighbor[i].AddNeighbour(v);
        v.AddNeighbour(u.Neighbor[i]);
      }
      // recompute the edge collapse costs in neighborhood
      for (int i = 0; i < tmp.Count; i++)
      {
        ComputeEdgeCostAtVertex(tmp[i]);
      }
    }

    private float Min(float a, float b)
    {
      if (a < b)
      {
        return a;
      }
      return b;
    }

    private float Max(float a, float b)
    {
      if (a > b)
      {
        return a;
      }
      return b;
    }
  }
}