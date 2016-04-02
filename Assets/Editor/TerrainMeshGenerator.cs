using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using LibNoise;
using LibNoise.Modifiers;
using MIConvexHull;
using System;

public class TerrainMeshGenerator
{
  private List<Triangle<Vertex>> Triangles;
  private List<Vertex> vertices;

  public int widthSegments;
  public int heightSegments;
  public int width;
  public int height;
  public float worldScale;

  private float riverScale = 40f;
  private float scale = 15.0f;
  private int octaves = 6;
  private float persistence = 0.35f;
  private float frequency = 0.008f;
  private int vertexIndex;
  private float waterLevel = -25.0f;

  private Color water = new Color(37f / 256f, 66f / 256f, 71f / 256f);
  private Color shallowWater = new Color(78f / 256f, 144f / 256f, 135f / 256f);
  private Color grass = new Color(117f / 256f, 153f / 256f, 19f / 256f);
  private Color sand = new Color(205f / 256f, 179f / 256f, 128f / 256f);
  private Color cliff = new Color(125f / 256f, 136f / 256f, 127f / 256f);

  public TerrainMeshGenerator()
  {
    vertices = new List<Vertex>();
  }

  public Mesh Createmesh()
  {
    vertexIndex = 0;
    GenerateMeshData();
    Mesh mesh = new Mesh();
    var verticesOrdered = vertices.OrderBy(v => v.Position3D.z).OrderBy(v => v.Position3D.x).ToArray();
    int maxId = verticesOrdered.Length;
    for (int i = 0; i < maxId; i++)
    {
      verticesOrdered[i].Id = i;
    }
    foreach (Triangle<Vertex> item in Triangles)
    {
      if (item.Vertices[0].Id >= maxId || item.Vertices[1].Id >= maxId || item.Vertices[2].Id >= maxId)
      {
        Debug.Log("gotha!");
      }
    }
    mesh.vertices = verticesOrdered.Select(v => v.Position3D).ToArray();
    List<int> triangleIndexes = new List<int>();
    foreach (var triangle in Triangles)
    {
      triangleIndexes.Add(triangle.Vertices[0].Id);
      triangleIndexes.Add(triangle.Vertices[1].Id);
      triangleIndexes.Add(triangle.Vertices[2].Id);
    }
    mesh.triangles = triangleIndexes.ToArray();
    //mesh.normals = verticesOrdered.Select(v => v.Normal).ToArray();
    mesh.colors = verticesOrdered.Select(v => v.Color).ToArray();
    //mesh.uv = uvs.ToArray();
    //mesh.tangents = tangents.ToArray();
    mesh.RecalculateBounds();
    mesh.Optimize();
    return mesh;
  }

  public void GenerateMeshData()
  {
    //prepare noise generators
    int seed = 1000;
    Perlin perlinLeft = new Perlin() { Frequency = 0.007f, NoiseQuality = NoiseQuality.Standard, Seed = seed, OctaveCount = 6, Lacunarity = 1.3, Persistence = 0.45f };
    Perlin perlinRight = new Perlin() { Frequency = 0.008f, NoiseQuality = NoiseQuality.Standard, Seed = seed * 2, OctaveCount = 6, Lacunarity = 1.2, Persistence = 0.45f };
    Perlin perlinLeftWidth = new Perlin() { Frequency = 0.007f, NoiseQuality = NoiseQuality.Standard, Seed = seed / 5, OctaveCount = 6, Lacunarity = 1.4, Persistence = 0.6f };
    Perlin perlinRightWidth = new Perlin() { Frequency = 0.008f, NoiseQuality = NoiseQuality.Standard, Seed = seed * 3, OctaveCount = 6, Lacunarity = 1.4, Persistence = 0.6f };

    Perlin terrainPerlin = new Perlin() { Frequency = frequency, NoiseQuality = NoiseQuality.Standard, Seed = 0, OctaveCount = octaves, Lacunarity = 2.5, Persistence = persistence };
    RidgedMultifractal terrainRMF = new RidgedMultifractal() { Frequency = terrainPerlin.Frequency / 4, NoiseQuality = NoiseQuality.High, Seed = 2, OctaveCount = octaves, Lacunarity = 5 };
    ScaleOutput scaledRMF = new ScaleOutput(terrainRMF, 1.2);
    Add terrainAdd = new Add(terrainPerlin, new BiasOutput(scaledRMF, 0.6));
    ScaleOutput terrainScaledModule = new ScaleOutput(terrainAdd, scale);

    //set scalling factors
    float scaleX = (float)width / ((float)widthSegments - 1);
    float scaleZ = (float)height / ((float)heightSegments - 1);
    riverScale = riverScale / worldScale;
    waterLevel = waterLevel / worldScale;
    float realWaterLevel = 0f;

    for (int z = 0; z < heightSegments; z++)
    {
      float worldZ = z * scaleZ;
      //generate river channels for Z
      float leftWidth = Mathf.Abs((float)perlinLeftWidth.GetValue(0, 0, worldZ * worldScale)) * riverScale + riverScale * 1.2f;
      float rightWidth = Mathf.Abs((float)perlinRightWidth.GetValue(0, 0, worldZ * worldScale)) * riverScale + riverScale * 1.2f;
      float leftShore = (float)perlinLeft.GetValue(0, 0, worldZ * worldScale) * riverScale * 1.5f - riverScale;
      float rightShore = (float)perlinRight.GetValue(0, 0, worldZ * worldScale) * riverScale * 1.5f + riverScale;
      Vector3 leftChannelLeftShore = new Vector3(leftShore, 0f, worldZ);
      Vector3 leftChannelRightShore = new Vector3(leftShore + leftWidth, 0f, worldZ);
      Vector3 leftChannelMiddle = new Vector3(leftShore + leftWidth / 2f, waterLevel, worldZ);
      Vector3 rightChannelLeftShore = new Vector3(rightShore, 0f, worldZ);
      Vector3 rightChannelRightShore = new Vector3(rightShore + rightWidth, 0f, worldZ);
      Vector3 rightChannelMiddle = new Vector3(rightShore + rightWidth / 2f, waterLevel, worldZ);
      Vector3 midChannel = new Vector3(leftChannelLeftShore.x * 0.5f + rightChannelRightShore.x * 0.5f, waterLevel, worldZ);

      for (int x = 0; x < widthSegments; x++)
      {
        float worldX = (x - widthSegments / 2) * scaleX;
        float worldY = (float)terrainScaledModule.GetValue(worldX * worldScale, 0, worldZ * worldScale) / worldScale + 1.1f;
        Vector3 vertex = new Vector3(worldX, worldY, worldZ);
        float factor = 0f;
        float delta = 0f;
        Color color = grass;
        //river shape calculations
        //A land shaping
        //1. points left of the left channel
        if (worldX <= leftChannelLeftShore.x)
        {
          factor = Decline(leftChannelLeftShore, vertex);
          delta = (worldY - waterLevel) * factor;
        }
        //2. points between channels
        if (worldX >= leftChannelRightShore.x && worldX <= rightChannelLeftShore.x)
        {
          factor = Decline(leftChannelRightShore, vertex) * 0.5f + Decline(rightChannelLeftShore, vertex) * 0.5f;
          delta = (worldY - waterLevel) * factor;
        }
        //3. points right of the right channel
        if (worldX >= rightChannelRightShore.x)
        {
          factor = Decline(rightChannelRightShore, vertex);
          delta = (worldY - waterLevel) * factor;
        }
        //B bottom shapping
        //1. separated channels
        if (leftChannelRightShore.x < rightChannelLeftShore.x)
        {
          //left channel
          if (worldX > leftChannelLeftShore.x && worldX <= leftChannelRightShore.x)
          {
            factor = 1f;
            float factorBetween = Decline(leftChannelMiddle, vertex);
            delta = (worldY - waterLevel) * (factor + factorBetween);
            color = water;
          }
          //right channel
          if (worldX > rightChannelLeftShore.x && worldX <= rightChannelRightShore.x)
          {
            factor = 1f;
            float factorBetween = Decline(rightChannelMiddle, vertex);
            delta = (worldY - waterLevel) * (factor + factorBetween);
            color = water;
          }
        }
        float lastFactorBetween = 0f;
        //2. overlapping channels
        if (leftChannelRightShore.x >= rightChannelLeftShore.x)
        {
          //left channel part
          if (worldX > leftChannelLeftShore.x && worldX <= leftChannelMiddle.x)
          {
            factor = 1f;
            float factorBetween = Decline(leftChannelMiddle, vertex);
            lastFactorBetween = factorBetween;
            delta = (worldY - waterLevel) * (factor + factorBetween);
            color = water;
          }
          //right channel part
          if (worldX > rightChannelMiddle.x && worldX <= rightChannelRightShore.x)
          {
            factor = 1f;
            float factorBetween = Decline(rightChannelMiddle, vertex);
            delta = (worldY - waterLevel) * (factor + factorBetween);
            color = water;
          }
          //part in the middle
          if (worldX >= leftChannelMiddle.x && worldX <= rightChannelMiddle.x)
          {
            factor = 1f;
            float factorBetween = Decline(midChannel, vertex);
            delta = (worldY - waterLevel) * (factor + factorBetween + lastFactorBetween);
            color = water;
          }
        }
        worldY = worldY - delta;

        color = grass;
        //biomes
        if (worldY <= realWaterLevel - 1f)
        {
          color = water;
        }
        else if (worldY > realWaterLevel - 1f && worldY < realWaterLevel - 0.3f)
        {
          color = shallowWater;
        }
        else if (worldY > realWaterLevel - 0.3f && worldY < realWaterLevel + 0.2f)
        {
          color = sand;
        }
        vertex = new Vector3(worldX, worldY, worldZ);
        //determine if vertex is on the boundary
        bool boundary = false;
        if (x == 0 || x == widthSegments - 1 || z == 0 || z == heightSegments - 1)
        {
          boundary = true;
        }
        vertices.Add(new Vertex { Position3D = vertex, Id = vertexIndex++, Color = color, IsOnBoundary = boundary });

        //Vector4 tangent = new Vector4(worldX, 0f, 0f, -1f);
        //tangent.Normalize();
        //tangents.Add(tangent);
        //uvs.Add(new Vector2(x * scaleX / (float)widthSegments, z * scaleZ / (float)heightSegments));
        //colors.Add(Color.Lerp(Color.red, Color.green, (float)x / (float)widthSegments));
      }
    }

    var config = new TriangulationComputationConfig();
    Triangles = Triangulation.CreateDelaunay<Vertex, Triangle<Vertex>>(vertices, config).Cells.ToList();
    foreach (Triangle<Vertex> triangle in Triangles)
    {
      triangle.Vertices[0].AddNeighbour(triangle.Vertices[1]);
      triangle.Vertices[0].AddNeighbour(triangle.Vertices[2]);
      triangle.Vertices[0].AddTriangle(triangle);

      triangle.Vertices[1].AddNeighbour(triangle.Vertices[0]);
      triangle.Vertices[1].AddNeighbour(triangle.Vertices[2]);
      triangle.Vertices[1].AddTriangle(triangle);

      triangle.Vertices[2].AddNeighbour(triangle.Vertices[0]);
      triangle.Vertices[2].AddNeighbour(triangle.Vertices[1]);
      triangle.Vertices[2].AddTriangle(triangle);
      triangle.ComputeNormal();
    }
    //int beforeCount = vertices.Count;
    //int targetCount = (int)(beforeCount * 0.6);
    //InitCollapse();
    //Vertex mn = MinimumCostEdge();
    //float currentCost = mn.CachedEdgeCollapsingCost;
    //while (vertices.Count > targetCount)
    //{
    //  Collapse(mn, mn.CollapseCandidate);
    //  mn = MinimumCostEdge();
    //  currentCost = mn.CachedEdgeCollapsingCost;
    //}
    //Debug.LogFormat("Reduced from {0} to {1}", beforeCount, vertices.Count);
  }

  private void InitCollapse()
  {
    for (int i = 0; i < vertices.Count; i++)
    {
      ComputeEdgeCostAtVertex(vertices[i]);
    }
  }

  private Vertex MinimumCostEdge()
  {
    float cost = 1000000;
    Vertex candidate = null;
    for (int i = 0; i < vertices.Count; i++)
    {
      if (vertices[i].CachedEdgeCollapsingCost < cost)
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

  private void Collapse(Vertex u, Vertex v)
  {
    // Collapse the edge uv by moving vertex u onto v
    if (v == null)
    {
      // u is a vertex all by itself so just delete it
      vertices.Remove(u);
      return;
    }
    // make tmp a list of all the neighbors of u
    List<Vertex> tmp = new List<Vertex>(u.Neighbor);
    // delete triangles on edge uv:
    for (int i = u.Triangles.Count - 1; i >= 0; i--)
    {
      if (u.Triangles[i].HasVertex(v))
      {
        Triangles.Remove(u.Triangles[i]);
        u.Triangles.Remove(u.Triangles[i]);
      }
    }
    // update remaining triangles to have v instead of u
    for (int i = u.Triangles.Count - 1; i >= 0; i--)
    {
      u.Triangles[i].ReplaceVertex(u, v);
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

  private float Decline(Vector3 mid, Vector3 point)
  {
    float distance = Mathf.Abs((point - mid).magnitude) * worldScale;
    //don't calculate for some distance - check what distance
    float factor = Mathf.Exp(distance * distance / 250f * -1f);
    return factor;
  }
}
