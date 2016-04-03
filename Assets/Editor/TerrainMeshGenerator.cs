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
  private float bottomLevel = 25.0f;

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

  private void CarveChannel(float scaleX, float waterLevel, float worldZ, float leftChannelLeftShore, float rightChannelRightShore, float midJointChannel, IModule unscalledTerrainModule, float dropDistanceFactor)
  {
    //vertices.Add(new Vertex { Position3D = new Vector3(leftChannelLeftShore, waterLevel, worldZ), Id = vertexIndex++, Color = water });
    int stepsInChannel = Mathf.FloorToInt((rightChannelRightShore - leftChannelLeftShore) / scaleX);
    float localScaleX = (rightChannelRightShore - leftChannelLeftShore) / (float)stepsInChannel;
    float worldX = leftChannelLeftShore;
    float dropDistance = (midJointChannel - worldX) / dropDistanceFactor;
    for (int i = 0; i <= stepsInChannel; i++)
    {
      float worldY = waterLevel - bottomLevel;
      //left decline
      if (worldX < leftChannelLeftShore + dropDistance)
      {
        float x = (worldX - leftChannelLeftShore) / dropDistance * 3.141592654f;
        worldY = waterLevel - bottomLevel * ((Mathf.Cos(x) * -1f) / 2f + 0.5f);
      }
      if (worldX > rightChannelRightShore - dropDistance)
      {
        float x = (rightChannelRightShore - worldX) / dropDistance * -3.141592654f;
        worldY = waterLevel - bottomLevel * ((Mathf.Cos(x) * -1f) / 2f + 0.5f);
      }
      float yDeviation = (float)unscalledTerrainModule.GetValue(worldX * worldScale, 0, worldZ * worldScale);
      worldY = worldY + yDeviation;
      if (worldY > waterLevel)
      {
        worldY = waterLevel;
      }
      Vector3 vertex = new Vector3(worldX, worldY, worldZ);
      vertices.Add(new Vertex { Position3D = vertex, Id = vertexIndex++, Color = water });
      worldX = worldX + localScaleX;
    }
    //vertices.Add(new Vertex { Position3D = new Vector3(rightChannelRightShore, waterLevel, worldZ), Id = vertexIndex++, Color = water });
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
    bottomLevel = bottomLevel / worldScale;
    float waterLevel = 0f;
    float leftBoundary = (widthSegments / 2) * scaleX * -1f;
    float rightBoundary = (widthSegments / 2) * scaleX;

    for (int z = 0; z < heightSegments; z++)
    {
      float worldZ = z * scaleZ;
      //generate river channels for Z
      float leftWidth = Mathf.Abs((float)perlinLeftWidth.GetValue(0, 0, worldZ * worldScale)) * riverScale + riverScale * 1.5f;
      float rightWidth = Mathf.Abs((float)perlinRightWidth.GetValue(0, 0, worldZ * worldScale)) * riverScale + riverScale * 1.5f;

      float leftChannelLeftShore = (float)perlinLeft.GetValue(0, 0, worldZ * worldScale) * riverScale * 1.5f - riverScale;
      float leftChannelMiddle = leftChannelLeftShore + leftWidth / 2f;
      float leftChannelRightShore = leftChannelLeftShore + leftWidth;

      float rightChannelLeftShore = (float)perlinRight.GetValue(0, 0, worldZ * worldScale) * riverScale * 1.5f + riverScale;
      float rightChannelMiddle = rightChannelLeftShore + rightWidth / 2f;
      float rightChannelRightShore = rightChannelLeftShore + rightWidth;

      float midJointChannel = leftChannelLeftShore * 0.5f + rightChannelRightShore * 0.5f;

      if (rightChannelLeftShore < leftChannelRightShore)
      {
        //joint channel
        CarveChannel(scaleX, waterLevel, worldZ, leftChannelLeftShore, rightChannelRightShore, midJointChannel, terrainAdd, 2f);
      }
      else {
        CarveChannel(scaleX, waterLevel, worldZ, leftChannelLeftShore, leftChannelRightShore, leftChannelMiddle, terrainAdd, 1f);
        CarveChannel(scaleX, waterLevel, worldZ, rightChannelLeftShore, rightChannelRightShore, rightChannelMiddle, terrainAdd, 1f);
      }
      //fill terrain left from the river and add boundary vertex
      float worldY = 0f;
      for (float worldX = leftChannelLeftShore; worldX > leftBoundary; worldX = worldX - scaleX)
      {
        worldY = (float)terrainScaledModule.GetValue(worldX * worldScale, 0, worldZ * worldScale) / worldScale + 1.1f;
        float factor = Decline(leftChannelLeftShore - worldX, 10f);
        float delta = (worldY - waterLevel) * factor;
        worldY = worldY - delta;
        Vector3 vertex = new Vector3(worldX, worldY, worldZ);
        vertices.Add(new Vertex { Position3D = vertex, Id = vertexIndex++, Color = grass });
      }
      Vector3 leftEdgeVertex = new Vector3(leftBoundary, worldY, worldZ);
      vertices.Add(new Vertex { Position3D = leftEdgeVertex, Id = vertexIndex++, Color = grass });


      //vertices.Add(new Vertex { Position3D = new Vector3(widthSegments / 2 * scaleX * -1, realWaterLevel, worldZ), Id = vertexIndex++, Color = sand });
      //vertices.Add(new Vertex { Position3D = leftChannelLeftShore, Id = vertexIndex++, Color = water });
      //vertices.Add(new Vertex { Position3D = leftChannelRightShore, Id = vertexIndex++, Color = water });
      //vertices.Add(new Vertex { Position3D = rightChannelLeftShore, Id = vertexIndex++, Color = water });
      //vertices.Add(new Vertex { Position3D = rightChannelRightShore, Id = vertexIndex++, Color = water });
      //vertices.Add(new Vertex { Position3D = new Vector3(widthSegments / 2 * scaleX, realWaterLevel, worldZ), Id = vertexIndex++, Color = sand });

      for (int x = 0; x < widthSegments; x++)
      {

        //float worldX = (x - widthSegments / 2) * scaleX;
        //float worldY = (float)terrainScaledModule.GetValue(worldX * worldScale, 0, worldZ * worldScale) / worldScale + 1.1f;
        //Vector3 vertex = new Vector3(worldX, worldY, worldZ);
        //float factor = 0f;
        //float delta = 0f;
        //Color color = grass;
        ////river shape calculations
        ////A land shaping
        ////1. points left of the left channel
        //if (worldX <= leftChannelLeftShore.x)
        //{
        //  factor = Decline(leftChannelLeftShore, vertex);
        //  delta = (worldY - waterLevel) * factor;
        //}
        ////2. points between channels
        //if (worldX >= leftChannelRightShore.x && worldX <= rightChannelLeftShore.x)
        //{
        //  factor = Decline(leftChannelRightShore, vertex) * 0.5f + Decline(rightChannelLeftShore, vertex) * 0.5f;
        //  delta = (worldY - waterLevel) * factor;
        //}
        ////3. points right of the right channel
        //if (worldX >= rightChannelRightShore.x)
        //{
        //  factor = Decline(rightChannelRightShore, vertex);
        //  delta = (worldY - waterLevel) * factor;
        //}
        ////B bottom shapping
        ////1. separated channels
        //if (leftChannelRightShore.x < rightChannelLeftShore.x)
        //{
        //  //left channel
        //  if (worldX > leftChannelLeftShore.x && worldX <= leftChannelRightShore.x)
        //  {
        //    factor = 1f;
        //    float factorBetween = Decline(leftChannelMiddle, vertex);
        //    delta = (worldY - waterLevel) * (factor + factorBetween);
        //    color = water;
        //  }
        //  //right channel
        //  if (worldX > rightChannelLeftShore.x && worldX <= rightChannelRightShore.x)
        //  {
        //    factor = 1f;
        //    float factorBetween = Decline(rightChannelMiddle, vertex);
        //    delta = (worldY - waterLevel) * (factor + factorBetween);
        //    color = water;
        //  }
        //}
        //float lastFactorBetween = 0f;
        ////2. overlapping channels
        //if (leftChannelRightShore.x >= rightChannelLeftShore.x)
        //{
        //  //left channel part
        //  if (worldX > leftChannelLeftShore.x && worldX <= leftChannelMiddle.x)
        //  {
        //    factor = 1f;
        //    float factorBetween = Decline(leftChannelMiddle, vertex);
        //    lastFactorBetween = factorBetween;
        //    delta = (worldY - waterLevel) * (factor + factorBetween);
        //    color = water;
        //  }
        //  //right channel part
        //  if (worldX > rightChannelMiddle.x && worldX <= rightChannelRightShore.x)
        //  {
        //    factor = 1f;
        //    float factorBetween = Decline(rightChannelMiddle, vertex);
        //    delta = (worldY - waterLevel) * (factor + factorBetween);
        //    color = water;
        //  }
        //  //part in the middle
        //  if (worldX >= leftChannelMiddle.x && worldX <= rightChannelMiddle.x)
        //  {
        //    factor = 1f;
        //    float factorBetween = Decline(midChannel, vertex);
        //    delta = (worldY - waterLevel) * (factor + factorBetween + lastFactorBetween);
        //    color = water;
        //  }
        //}
        //worldY = worldY - delta;

        //color = grass;
        ////biomes
        //if (worldY <= realWaterLevel - 1f)
        //{
        //  color = water;
        //}
        //else if (worldY > realWaterLevel - 1f && worldY < realWaterLevel - 0.3f)
        //{
        //  color = shallowWater;
        //}
        //else if (worldY > realWaterLevel - 0.3f && worldY < realWaterLevel + 0.2f)
        //{
        //  color = sand;
        //}
        //vertex = new Vector3(worldX, worldY, worldZ);
        //vertices.Add(new Vertex { Position3D = vertex, Id = vertexIndex++, Color = color });

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
      triangle.ComputeNormal();
    }
  }

  private float Decline(float distance, float zeroPointDistance)
  {
    //don't calculate for some distance - check what distance
    float factor = Mathf.Exp(distance * distance / (zeroPointDistance / 2 * 10) * -1f);
    return factor;
  }
}
