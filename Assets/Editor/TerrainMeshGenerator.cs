using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using LibNoise;
using LibNoise.Modifiers;
using System.Diagnostics;
using MIConvexHull;

public class TerrainMeshGenerator
{
  private List<GridCell> gridCells;
  private IEnumerable<Triangle<Vertex>> tetrahedronCells;
  private List<Vertex> vertices;
  private List<int> triangles;
  private List<Vector4> tangents;
  private List<Vector2> uvs;
  private List<Color> colors;

  public int widthSegments;
  public int heightSegments;
  public int width;
  public int height;
  public float worldScale;

  private float riverScale = 40f;
  private float scale = 10.0f;
  private int octaves = 6;
  private float persistence = 0.35f;
  private float frequency = 0.008f;
  private int vertexIndex;
  private float waterLevel = -20.0f;

  private Color water = new Color(37f / 256f, 66f / 256f, 71f / 256f);
  private Color shallowWater = new Color(78f / 256f, 144f / 256f, 135f / 256f);
  private Color grass = new Color(117f / 256f, 153f / 256f, 19f / 256f);
  private Color sand = new Color(205f / 256f, 179f / 256f, 128f / 256f);
  private Color cliff = new Color(125f / 256f, 136f / 256f, 127f / 256f);

  public TerrainMeshGenerator()
  {
    vertices = new List<Vertex>();
    tangents = new List<Vector4>();
    uvs = new List<Vector2>();
    colors = new List<Color>();
    triangles = new List<int>();
    gridCells = new List<GridCell>();
  }

  public Mesh Createmesh()
  {
    vertexIndex = 0;
    GenerateMeshData();
    Mesh mesh = new Mesh();
    Vertex[] verticesOrdered = vertices.OrderBy(v => v.Id).ToArray();
    mesh.vertices = verticesOrdered.Select(v => v.Coords).ToArray();
    foreach (var triangle in tetrahedronCells)
    {
      triangles.Add(triangle.Vertices[0].Id);
      triangles.Add(triangle.Vertices[1].Id);
      triangles.Add(triangle.Vertices[2].Id);
    }
    mesh.triangles = triangles.ToArray();
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
    Perlin perlinLeft = new Perlin() { Frequency = frequency, NoiseQuality = NoiseQuality.Standard, Seed = seed, OctaveCount = octaves, Lacunarity = 1.3, Persistence = persistence };
    Perlin perlinRight = new Perlin() { Frequency = frequency, NoiseQuality = NoiseQuality.Standard, Seed = seed * 2, OctaveCount = octaves, Lacunarity = 1.4, Persistence = persistence };
    Perlin perlinLeftWidth = new Perlin() { Frequency = frequency, NoiseQuality = NoiseQuality.Standard, Seed = seed / 5, OctaveCount = octaves, Lacunarity = 1.2, Persistence = persistence };
    Perlin perlinRightWidth = new Perlin() { Frequency = frequency, NoiseQuality = NoiseQuality.Standard, Seed = seed * 3, OctaveCount = octaves, Lacunarity = 1.2, Persistence = persistence };

    Perlin terrainPerlin = new Perlin() { Frequency = frequency, NoiseQuality = NoiseQuality.Standard, Seed = 0, OctaveCount = octaves, Lacunarity = 2.5, Persistence = persistence };
    Billow terrainBillow = new Billow() { Frequency = terrainPerlin.Frequency, NoiseQuality = NoiseQuality.High, Seed = 1, OctaveCount = octaves, Lacunarity = terrainPerlin.Lacunarity, Persistence = terrainPerlin.Persistence };
    RidgedMultifractal terrainRMF = new RidgedMultifractal() { Frequency = terrainPerlin.Frequency, NoiseQuality = NoiseQuality.High, Seed = 2, OctaveCount = octaves, Lacunarity = 5 };
    Select terrainSelect = new Select(terrainPerlin, terrainRMF, terrainBillow) { EdgeFalloff = 0 };
    terrainSelect.SetBounds(-0.75, 0.75);
    Add terrainAdd = new Add(terrainPerlin, terrainRMF);
    ScaleOutput terrainScaledModule = new ScaleOutput(terrainAdd, scale);

    //set scalling factors
    float scaleX = (float)width / ((float)widthSegments - 1);
    float scaleZ = (float)height / ((float)heightSegments - 1);
    riverScale = riverScale / worldScale;
    waterLevel = waterLevel / worldScale;

    for (int z = 0; z < heightSegments; z++)
    {
      float worldZ = z * scaleZ;
      //generate river channels for Z
      float leftWidth = Mathf.Abs((float)perlinLeftWidth.GetValue(0, 0, worldZ * worldScale)) * riverScale + riverScale;
      float rightWidth = Mathf.Abs((float)perlinRightWidth.GetValue(0, 0, worldZ * worldScale)) * riverScale + riverScale;
      float leftShore = (float)perlinLeft.GetValue(0, 0, worldZ * worldScale) * riverScale - riverScale * 0.7f;
      float rightShore = (float)perlinRight.GetValue(0, 0, worldZ * worldScale) * riverScale + riverScale * 0.7f;
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
        float worldY = (float)terrainScaledModule.GetValue(worldX * worldScale, 0, worldZ * worldScale) / worldScale;
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

        float realWaterLevel = -1.1f;
        color = grass;
        //biomes
        if (worldY <= realWaterLevel - 1f)
        {
          color = water;
        }
        else if(worldY > realWaterLevel - 1f && worldY < realWaterLevel - 0.3f)
        {
          color = shallowWater;
        }
        else if (worldY > realWaterLevel - 0.3f && worldY < realWaterLevel + 0.2f)
        {
          color = sand;
        }
        vertex = new Vector3(worldX, worldY, worldZ);
        vertices.Add(new Vertex { Coords = vertex, Id = vertexIndex++, Color = color });

        //Vector4 tangent = new Vector4(worldX, 0f, 0f, -1f);
        //tangent.Normalize();
        //tangents.Add(tangent);
        //uvs.Add(new Vector2(x * scaleX / (float)widthSegments, z * scaleZ / (float)heightSegments));
        //colors.Add(Color.Lerp(Color.red, Color.green, (float)x / (float)widthSegments));
      }
    }

    var config = new TriangulationComputationConfig();
    tetrahedronCells = Triangulation.CreateDelaunay<Vertex, Triangle<Vertex>>(vertices, config).Cells;
  }

  private float Decline(Vector3 mid, Vector3 point)
  {
    float distance = Mathf.Abs((point - mid).magnitude) * worldScale;
    float factor = Mathf.Exp(distance * distance / 200f * -1f);
    //if (factor < -0.1)
    //{
    //  return 0f;
    //}
    return factor;
  }
}
