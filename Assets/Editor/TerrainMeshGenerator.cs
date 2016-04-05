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
  private Perlin biomesPerlin;
  private Perlin perlinLeft;
  private Perlin perlinLeftWidth;
  private Perlin perlinRight;
  private Perlin perlinRightWidth;
  private ScaleOutput terrainScaledModule;
  private List<Triangle<Vertex>> Triangles;
  private List<Vertex> vertices;

  public float angle;
  public int widthSegments;
  public int heightSegments;
  public int width;
  public int height;
  public float worldScale;

  private float riverScale = 40f;
  private float scale = 15.0f;
  private int vertexIndex;
  private float bottomLevel = -25.0f;
  private int seed = DateTime.Now.Millisecond * DateTime.Now.Second * DateTime.Now.Minute;

  public TerrainMeshGenerator()
  {
    vertices = new List<Vertex>();
  }

  private void InitGenerators()
  {
    //prepare noise generators
    perlinLeft = new Perlin() { Frequency = 0.004f, NoiseQuality = NoiseQuality.Standard, Seed = seed, OctaveCount = 6, Lacunarity = 1.2, Persistence = 0.45f };
    perlinRight = new Perlin() { Frequency = 0.004f, NoiseQuality = NoiseQuality.Standard, Seed = seed * 2, OctaveCount = 6, Lacunarity = 1.2, Persistence = 0.45f };
    perlinLeftWidth = new Perlin() { Frequency = 0.005f, NoiseQuality = NoiseQuality.Standard, Seed = seed / 5, OctaveCount = 6, Lacunarity = 1.4, Persistence = 0.3f };
    perlinRightWidth = new Perlin() { Frequency = 0.005f, NoiseQuality = NoiseQuality.Standard, Seed = seed * 3, OctaveCount = 6, Lacunarity = 1.4, Persistence = 0.3f };

    Perlin terrainPerlin = new Perlin() { Frequency = 0.008f, NoiseQuality = NoiseQuality.Standard, Seed = 0, OctaveCount = 6, Lacunarity = 2.5, Persistence = 0.35f };
    RidgedMultifractal terrainRMF = new RidgedMultifractal() { Frequency = 0.002f, NoiseQuality = NoiseQuality.High, Seed = 2, OctaveCount = 6, Lacunarity = 5 };
    ScaleOutput scaledRMF = new ScaleOutput(terrainRMF, 1.2);
    Add terrainAdd = new Add(terrainPerlin, new BiasOutput(scaledRMF, 0.6));
    terrainScaledModule = new ScaleOutput(terrainAdd, scale);

    biomesPerlin = new Perlin() { Frequency = 0.15f, NoiseQuality = NoiseQuality.Low, Seed = 0, OctaveCount = 6, Lacunarity = 2.5, Persistence = 0.35f };
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
    mesh.vertices = verticesOrdered.Select(v => v.Position3D).ToArray();
    List<int> triangleIndexes = new List<int>(Triangles.Count * 3);
    foreach (var triangle in Triangles)
    {
      triangleIndexes.Add(triangle.Vertices[0].Id);
      triangleIndexes.Add(triangle.Vertices[1].Id);
      triangleIndexes.Add(triangle.Vertices[2].Id);
    }
    mesh.triangles = triangleIndexes.ToArray();
    //mesh.subMeshCount = 8;
    //GetTrianglesByBiome(triangleIndexes, Biomes.Grass);
    //mesh.SetTriangles(triangleIndexes.ToArray(), 0);
    //GetTrianglesByBiome(triangleIndexes, Biomes.Dirt);
    //mesh.SetTriangles(triangleIndexes.ToArray(), 1);
    //GetTrianglesByBiome(triangleIndexes, Biomes.DeadGrass);
    //mesh.SetTriangles(triangleIndexes.ToArray(), 2);
    //GetTrianglesByBiome(triangleIndexes, Biomes.Cliff);
    //mesh.SetTriangles(triangleIndexes.ToArray(), 3);
    //GetTrianglesByBiome(triangleIndexes, Biomes.Sand);
    //mesh.SetTriangles(triangleIndexes.ToArray(), 4);
    //GetTrianglesByBiome(triangleIndexes, Biomes.Water);
    //mesh.SetTriangles(triangleIndexes.ToArray(), 5);
    //GetTrianglesByBiome(triangleIndexes, Biomes.ShallowWater);
    //mesh.SetTriangles(triangleIndexes.ToArray(), 6);
    //GetTrianglesByBiome(triangleIndexes, Biomes.DeepWater);
    //mesh.SetTriangles(triangleIndexes.ToArray(), 7);
    //mesh.triangles = triangleIndexes.ToArray();
    mesh.colors = verticesOrdered.Select(v => v.Color).ToArray();
    //mesh.normals = verticesOrdered.Select(v => v.Normal).ToArray();
    //mesh.uv = uvs.ToArray();
    //mesh.tangents = tangents.ToArray();
    mesh.RecalculateBounds();
    mesh.Optimize();
    return mesh;
  }

  private void GetTrianglesByBiome(List<int> triangleIndexes, Biomes biomes)
  {
    triangleIndexes.Clear();
    foreach (var triangle in Triangles.Where(t => t.Biome == biomes))
    {
      triangleIndexes.Add(triangle.Vertices[0].Id);
      triangleIndexes.Add(triangle.Vertices[1].Id);
      triangleIndexes.Add(triangle.Vertices[2].Id);
    }
  }

  public void GenerateMeshData()
  {
    InitGenerators();
    //set scalling factors
    float scaleX = (float)width / ((float)widthSegments - 1);
    float scaleZ = (float)height / ((float)heightSegments - 1);
    riverScale = riverScale / worldScale;
    bottomLevel = bottomLevel / worldScale;
    float waterLevel = 0f;
    float leftBoundary = (widthSegments / 2) * scaleX * -1f;
    float rightBoundary = (widthSegments / 2) * scaleX;

    for (int z = 0; z < heightSegments; z++)
    {
      Vector3[] dataRow = new Vector3[widthSegments];
      float worldZ = z * scaleZ;

      //fill terrain left from the river and add boundary vertex
      for (int x = 0; x < widthSegments; x++)
      {
        float worldX = (x - widthSegments / 2) * scaleX;
        float worldY = (float)terrainScaledModule.GetValue(worldX * worldScale, 0, worldZ * worldScale) / worldScale + 1.1f;
        //ensure river is down at the bottom
        Vector3 vertex = new Vector3(worldX, worldY, worldZ);
        dataRow[x] = vertex;
      }
      CarveRiverChannel(dataRow, scaleX, worldZ);
      for (int x = 0; x < widthSegments; x++)
      {
        vertices.Add(new Vertex { Position3D = dataRow[x], Id = vertexIndex++ });
      }
      //for (int x = 0; x < widthSegments; x++)
      //{
      //  uvs.Add(new Vector2(x * scaleX / (float)widthSegments, z * scaleZ / (float)heightSegments));
      //}
    }

    var config = new TriangulationComputationConfig();
    Triangles = Triangulation.CreateDelaunay<Vertex, Triangle<Vertex>>(vertices, config).Cells.ToList();
    for (int i = Triangles.Count - 1; i >= 0; i--)
    {
      Triangles[i].ComputeNormal();
      SetBiome(Triangles[i], biomesPerlin);
    }
    foreach (Triangle<Vertex> triangle in Triangles)
    {
      SplitTriangles(triangle);
    }
  }

  private void CarveRiverChannel(Vector3[] dataRow, float scaleX, float worldZ)
  {
    float constantWidthPart = riverScale * 3.1f;
    //generate river channels for Z
    float leftWidth = Mathf.Abs((float)perlinLeftWidth.GetValue(0, 0, worldZ * worldScale)) * riverScale * 3f + constantWidthPart;
    float rightWidth = Mathf.Abs((float)perlinRightWidth.GetValue(0, 0, worldZ * worldScale)) * riverScale * 3f + constantWidthPart;

    float leftChannelMiddle = (float)perlinLeft.GetValue(0, 0, worldZ * worldScale) * riverScale * 1.5f - constantWidthPart * 0.7f;
    float leftChannelLeftShore = leftChannelMiddle - leftWidth / 2f;
    float leftChannelRightShore = leftChannelMiddle + leftWidth / 2f;

    float rightChannelMiddle = (float)perlinRight.GetValue(0, 0, worldZ * worldScale) * riverScale * 1.5f + constantWidthPart * 0.7f;
    float rightChannelLeftShore = rightChannelMiddle - rightWidth / 2f;
    float rightChannelRightShore = rightChannelMiddle + rightWidth / 2f;

    float midJointChannel = leftChannelLeftShore * 0.5f + rightChannelRightShore * 0.5f;

    //approximate channel values to nearest segment
    int leftChannelLeftShoreSegment = Mathf.FloorToInt(leftChannelLeftShore / scaleX) + widthSegments / 2;
    int leftChannelMiddleSegment = Mathf.RoundToInt(leftChannelMiddle / scaleX) + widthSegments / 2;
    int leftChannelRightShoreSegment = Mathf.CeilToInt(leftChannelRightShore / scaleX) + widthSegments / 2;
    int rightChannelLeftShoreSegment = Mathf.FloorToInt(rightChannelLeftShore / scaleX) + widthSegments / 2;
    int rightChannelMiddleSegment = Mathf.RoundToInt(rightChannelMiddle / scaleX) + widthSegments / 2;
    int rightChannelRightShoreSegment = Mathf.CeilToInt(rightChannelRightShore / scaleX) + widthSegments / 2;
    int midJointChannelSegment = Mathf.RoundToInt(midJointChannel / scaleX) + widthSegments / 2;

    if (rightChannelLeftShore < leftChannelRightShore)
    {
      dataRow[leftChannelLeftShoreSegment] = new Vector3(dataRow[leftChannelLeftShoreSegment].x, waterLevel, dataRow[leftChannelLeftShoreSegment].z);
      for (int i = leftChannelLeftShoreSegment + 1; i < rightChannelRightShoreSegment; i++)
      {
        dataRow[i] = new Vector3(dataRow[i].x, bottomLevel, dataRow[i].z);
      }
      dataRow[rightChannelRightShoreSegment] = new Vector3(dataRow[rightChannelRightShoreSegment].x, waterLevel, dataRow[rightChannelRightShoreSegment].z);
    }
    else {
      dataRow[leftChannelLeftShoreSegment] = new Vector3(dataRow[leftChannelLeftShoreSegment].x, waterLevel, dataRow[leftChannelLeftShoreSegment].z);
      for (int i = leftChannelLeftShoreSegment + 1; i < leftChannelRightShoreSegment; i++)
      {
        dataRow[i] = new Vector3(dataRow[i].x, bottomLevel, dataRow[i].z);
      }
      dataRow[leftChannelRightShoreSegment] = new Vector3(dataRow[leftChannelRightShoreSegment].x, waterLevel, dataRow[leftChannelRightShoreSegment].z);

      dataRow[rightChannelLeftShoreSegment] = new Vector3(dataRow[rightChannelLeftShoreSegment].x, waterLevel, dataRow[rightChannelLeftShoreSegment].z);
      for (int i = rightChannelLeftShoreSegment + 1; i < rightChannelRightShoreSegment; i++)
      {
        dataRow[i] = new Vector3(dataRow[i].x, bottomLevel, dataRow[i].z);
      }
      dataRow[rightChannelRightShoreSegment] = new Vector3(dataRow[rightChannelRightShoreSegment].x, waterLevel, dataRow[rightChannelRightShoreSegment].z);
    }
  }

  float cliffAngle = 50;
  float hillAngle = 20;
  float waterLevel = 0f;
  float shallowWaterLevel = -1f;

  private void SetBiome(Triangle<Vertex> triangle, IModule biomesNoise)
  {
    Vector3 center = (triangle.Vertices[0].Position3D + triangle.Vertices[1].Position3D + triangle.Vertices[2].Position3D) / 3f;
    var noiseValue = biomesNoise.GetValue(center.x, center.y, center.z);
    float normalAngle = Mathf.Abs(Vector3.Angle(triangle.Normal, Vector3.up));
    triangle.Biome = Biomes.Grass;
    float triangleMinY = triangle.MinY();
    float triangleMaxY = triangle.MaxY();
    if (triangleMaxY < waterLevel)
    {
      if (normalAngle >= cliffAngle)
      {
        triangle.Biome = Biomes.DeepWater;
      }
      else if (triangleMinY > shallowWaterLevel)
      {
        triangle.Biome = Biomes.ShallowWater;
      }
      else {
        triangle.Biome = Biomes.Water;
      }
    }
    else {
      if (normalAngle >= cliffAngle)
      {
        triangle.Biome = Biomes.Cliff;
      }
      else
      {
        if (normalAngle < cliffAngle && normalAngle >= hillAngle)
        {
          if (noiseValue > 0.2)
          {
            triangle.Biome = Biomes.Dirt;
          }
          else {
            triangle.Biome = Biomes.DeadGrass;
          }
        }
        else {
          if (triangleMinY < waterLevel && triangleMaxY > waterLevel)
          {
            triangle.Biome = Biomes.Sand;
          }
          else {
            triangle.Biome = Biomes.Grass;
          }
        }
      }
    }
  }

  private void SplitTriangles(Triangle<Vertex> triangle)
  {
    if (triangle.Adjacency[0] != null)
    {
      if (triangle.Adjacency[0].Biome != triangle.Biome)
      {
        DuplicateIfSharedAtIndex(triangle, triangle.Adjacency[0].Vertices[0]);
        DuplicateIfSharedAtIndex(triangle, triangle.Adjacency[0].Vertices[1]);
        DuplicateIfSharedAtIndex(triangle, triangle.Adjacency[0].Vertices[2]);
      }
    }
    if (triangle.Adjacency[1] != null)
    {
      if (triangle.Adjacency[1].Biome != triangle.Biome)
      {
        DuplicateIfSharedAtIndex(triangle, triangle.Adjacency[1].Vertices[0]);
        DuplicateIfSharedAtIndex(triangle, triangle.Adjacency[1].Vertices[1]);
        DuplicateIfSharedAtIndex(triangle, triangle.Adjacency[1].Vertices[2]);
      }
    }
    if (triangle.Adjacency[2] != null)
    {
      if (triangle.Adjacency[2].Biome != triangle.Biome)
      {
        DuplicateIfSharedAtIndex(triangle, triangle.Adjacency[2].Vertices[0]);
        DuplicateIfSharedAtIndex(triangle, triangle.Adjacency[2].Vertices[1]);
        DuplicateIfSharedAtIndex(triangle, triangle.Adjacency[2].Vertices[2]);
      }
    }
  }

  private void DuplicateIfSharedAtIndex(Triangle<Vertex> triangle, Vertex toDuplicate)
  {
    int index = triangle.VertexIndex(toDuplicate);
    if (index >= 0)
    {
      Vertex newVertex = new Vertex { Id = vertexIndex++, Position3D = new Vector3(toDuplicate.Position3D.x, toDuplicate.Position3D.y, toDuplicate.Position3D.z) };
      triangle.Vertices[index] = newVertex;
      vertices.Add(newVertex);
    }
  }

  private Color water = new Color(37f / 256f, 66f / 256f, 71f / 256f);
  private Color shallowWater = new Color(78f / 256f, 144f / 256f, 135f / 256f);
  private Color grass = new Color(117f / 256f, 153f / 256f, 90f / 256f);
  private Color deadGrass = new Color(93f / 256f, 126f / 256f, 98f / 256f);
  private Color sand = new Color(205f / 256f, 179f / 256f, 128f / 256f);
  private Color cliff = new Color(125f / 256f, 136f / 256f, 127f / 256f);
  private Color dirt = new Color(207f / 256f, 181f / 256f, 144f / 256f);

  private void SetVertexColors(Triangle<Vertex> triangle)
  {
    switch (triangle.Biome)
    {
      case Biomes.Grass:
        triangle.Vertices[0].Color = grass;
        triangle.Vertices[1].Color = grass;
        triangle.Vertices[2].Color = grass;
        break;
      case Biomes.DeadGrass:
        triangle.Vertices[0].Color = deadGrass;
        triangle.Vertices[1].Color = deadGrass;
        triangle.Vertices[2].Color = deadGrass;
        break;
      case Biomes.Dirt:
        triangle.Vertices[0].Color = dirt;
        triangle.Vertices[1].Color = dirt;
        triangle.Vertices[2].Color = dirt;
        break;
      case Biomes.Cliff:
        triangle.Vertices[0].Color = cliff;
        triangle.Vertices[1].Color = cliff;
        triangle.Vertices[2].Color = cliff;
        break;
      case Biomes.Sand:
        break;
      case Biomes.ShallowWater:
        break;
      case Biomes.Water:
        break;
      case Biomes.DeepWater:
        break;
      default:
        break;
    }
  }
  private Vector3 CarveRiver(float worldZ, Vector3 leftChannelLeftShore, Vector3 leftChannelRightShore, Vector3 leftChannelMiddle, Vector3 rightChannelLeftShore, Vector3 rightChannelRightShore, Vector3 rightChannelMiddle, Vector3 midChannel, float worldX, float worldY)
  {
    Vector3 vertex = new Vector3(worldX, worldY, worldZ); ;
    float factor = 0f;
    float delta = 0f;
    Color color = grass;
    //river shape calculations
    //A land shaping
    //1. points left of the left channel
    if (worldX <= leftChannelLeftShore.x)
    {
      factor = Decline(leftChannelLeftShore, vertex);
      delta = (worldY - bottomLevel) * factor;
    }
    //2. points between channels
    if (worldX >= leftChannelRightShore.x && worldX <= rightChannelLeftShore.x)
    {
      factor = Decline(leftChannelRightShore, vertex) * 0.5f + Decline(rightChannelLeftShore, vertex) * 0.5f;
      delta = (worldY - bottomLevel) * factor;
    }
    //3. points right of the right channel
    if (worldX >= rightChannelRightShore.x)
    {
      factor = Decline(rightChannelRightShore, vertex);
      delta = (worldY - bottomLevel) * factor;
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
        delta = (worldY - bottomLevel) * (factor + factorBetween);
      }
      //right channel
      if (worldX > rightChannelLeftShore.x && worldX <= rightChannelRightShore.x)
      {
        factor = 1f;
        float factorBetween = Decline(rightChannelMiddle, vertex);
        delta = (worldY - bottomLevel) * (factor + factorBetween);
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
        delta = (worldY - bottomLevel) * (factor + factorBetween);
      }
      //right channel part
      if (worldX > rightChannelMiddle.x && worldX <= rightChannelRightShore.x)
      {
        factor = 1f;
        float factorBetween = Decline(rightChannelMiddle, vertex);
        delta = (worldY - bottomLevel) * (factor + factorBetween);
      }
      //part in the middle
      if (worldX >= leftChannelMiddle.x && worldX <= rightChannelMiddle.x)
      {
        factor = 1f;
        float factorBetween = Decline(midChannel, vertex);
        delta = (worldY - bottomLevel) * (factor + factorBetween + lastFactorBetween);
      }
    }
    worldY = worldY - delta;
    vertex = new Vector3(worldX, worldY, worldZ);
    return vertex;
  }
  private float Decline(Vector3 mid, Vector3 point)
  {
    float distance = Mathf.Abs((point - mid).magnitude) * worldScale;
    //don't calculate for some distance - check what distance
    float factor = Mathf.Exp(distance * distance / 250f * -1f);
    return factor;
  }
}
