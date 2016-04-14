using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using LibNoise;
using LibNoise.Modifiers;
using MIConvexHull;

namespace GrumpyDev.EndlessRiver
{
  public class Map
  {
    private Perlin biomesPerlin;
    private Perlin perlinLeft;
    private Perlin perlinLeftWidth;
    private Perlin perlinRight;
    private Perlin perlinRightWidth;
    //private float scaleX;
    //private float scaleZ;
    private ScaleOutput terrainScaledModule;
    private int currentEndZ;
    private int vertexId;
    List<Vertex> tempVertices;
    List<Triangle<Vertex>> tempTriangles;


    //general parameters
    public int Seed { get; set; }
    public List<Vertex> Vertices { get; set; }
    public List<Triangle<Vertex>> Triangles { get; set; }
    public List<RiverData> RiverData { get; set; }

    //terrain parameters
    public double TerrainPerlinFrequency { get; set; }
    public double TerrainPerlinLacunarity { get; set; }
    public double TerrainPerlinPersistence { get; set; }
    public double TerrainRMFFrequency { get; set; }
    public int TerrainRMFLacunarity { get; set; }
    public double TerrainRMFScale { get; set; }
    public double TerrainRMFBias { get; set; }
    public int WidthSegments { get; set; }
    public int HeightSegments { get; set; }
    public int WorldWidth { get; set; }
    public int WorldHeight { get; set; }
    public float WorldScale { get; set; }
    public float HeightScale { get; set; }

    //river parameters
    public double RiverShapeLacunarity { get; set; }
    public double RiverShapeFrequency { get; set; }
    public double RiverShapePersistence { get; set; }
    public double RiverWidthLacunarity { get; set; }
    public double RiverWidthFrequency { get; set; }
    public double RiverWidthPersistence { get; set; }
    public float RiverScale { get; set; }
    public float MinimumChannelWidth { get; set; }
    public float RiverWidthScale { get; set; }
    public float ChannelOffset { get; set; }

    //biomes parameters
    public double BiomesFrequency { get; set; }
    public double BiomesLacunarity { get; set; }
    public double BiomesPersistence { get; set; }
    public float CliffAngle { get; set; }
    public float HillAngle { get; set; }
    public float WaterLevel { get; set; }
    public float ShallowWaterLevel { get; set; }
    public float BottomLevel { get; set; }

    //vertex colors
    private Color WaterColor { get; set; }
    private Color ShallowWaterColor { get; set; }
    private Color GrassColor { get; set; }
    private Color DeadGrassColor { get; set; }
    private Color SandColor { get; set; }
    private Color CliffColor { get; set; }
    private Color DirtColor { get; set; }

    public Map()
    {
      currentEndZ = 0;
      Triangles = new List<Triangle<Vertex>>();
      Vertices = new List<Vertex>();
      RiverData = new List<RiverData>();
      vertexId = 0;
      //general parameters
      Seed = 1234566;
      //terrain parameters
      TerrainPerlinFrequency = 0.008f;
      TerrainPerlinLacunarity = 2.5;
      TerrainPerlinPersistence = 0.35f;
      TerrainRMFFrequency = 0.002f;
      TerrainRMFLacunarity = 5;
      TerrainRMFScale = 1.4;
      TerrainRMFBias = 0.6;
      WidthSegments = 1;
      HeightSegments = 1;
      WorldWidth = 100;
      WorldHeight = 200;
      WorldScale = 10;
      HeightScale = 15.0f;
      //river parameters
      RiverShapeLacunarity = 1.2;
      RiverShapeFrequency = 0.004f;
      RiverShapePersistence = 0.45f;
      RiverWidthLacunarity = 1.4;
      RiverWidthFrequency = 0.005f;
      RiverWidthPersistence = 0.3f;
      RiverScale = 6f;
      MinimumChannelWidth = 13f;
      RiverWidthScale = 12f;
      ChannelOffset = 9f;
      //biomes parameters
      BiomesFrequency = 0.15f;
      BiomesLacunarity = 2.5;
      BiomesPersistence = 0.35f;
      CliffAngle = 50;
      HillAngle = 20;
      WaterLevel = 0f;
      ShallowWaterLevel = -1f;
      BottomLevel = -2.5f;
      //vertex colors
      DirtColor = new Color(207f / 256f, 181f / 256f, 144f / 256f);
      CliffColor = new Color(125f / 256f, 136f / 256f, 127f / 256f);
      SandColor = new Color(205f / 256f, 179f / 256f, 128f / 256f);
      DeadGrassColor = new Color(93f / 256f, 126f / 256f, 98f / 256f);
      GrassColor = new Color(117f / 256f, 153f / 256f, 90f / 256f);
      ShallowWaterColor = new Color(78f / 256f, 144f / 256f, 135f / 256f);
      WaterColor = new Color(37f / 256f, 66f / 256f, 71f / 256f);
    }

    public void GetMeshRawData(float fromWorldZ, float toWorldZ, ref Vector3[] rawVertices, ref int[] rawTriangles)
    {
      int index = 0;
      List<Vector3> rawVerticesList = new List<Vector3>();
      List<int> rawTrianglesList = new List<int>();
      for (int i = 0; i < Vertices.Count; i++)
      {
        Vertices[i].Index = -1;
      }
      for (int i = 0; i < Triangles.Count; i++)
      {
        if ((Triangles[i].Vertices[0].Position3D.z >= fromWorldZ && Triangles[i].Vertices[0].Position3D.z <= toWorldZ)
          || (Triangles[i].Vertices[1].Position3D.z >= fromWorldZ && Triangles[i].Vertices[1].Position3D.z <= toWorldZ)
          || (Triangles[i].Vertices[2].Position3D.z >= fromWorldZ && Triangles[i].Vertices[2].Position3D.z <= toWorldZ))
        {
          if (Triangles[i].Vertices[0].Index == -1)
          {
            Triangles[i].Vertices[0].Index = index++;
            rawVerticesList.Add(Triangles[i].Vertices[0].Position3D);
          }
          if (Triangles[i].Vertices[1].Index == -1)
          {
            Triangles[i].Vertices[1].Index = index++;
            rawVerticesList.Add(Triangles[i].Vertices[1].Position3D);
          }
          if (Triangles[i].Vertices[2].Index == -1)
          {
            Triangles[i].Vertices[2].Index = index++;
            rawVerticesList.Add(Triangles[i].Vertices[2].Position3D);
          }
          rawTrianglesList.Add(Triangles[i].Vertices[0].Index);
          rawTrianglesList.Add(Triangles[i].Vertices[1].Index);
          rawTrianglesList.Add(Triangles[i].Vertices[2].Index);
        }
      }
      rawVertices = rawVerticesList.ToArray();
      rawTriangles = rawTrianglesList.ToArray();
    }
        
    public void GenerateMapData(float fromWorldX, float toWorldX, float fromWorldZ, float toWorldZ)
    {
      Vertices.Clear();
      Triangles.Clear();
      InitGenerators();
      //set scalling factors
      //scaleX = (float)WorldWidth / (float)WidthSegments;
      //scaleZ = (float)WorldHeight / (float)HeightSegments;
      currentEndZ = 0;
      GenerateMapInternal(fromWorldX, toWorldX, fromWorldZ, toWorldZ);
      Vertices.AddRange(tempVertices);
      Triangles.AddRange(tempTriangles);
      tempVertices.Clear();
      tempTriangles.Clear();
    }

    public void GenerateMapChunk()
    {
      //List<Vertex> verticesToRemove = new List<Vertex>();
      //List<Triangle<Vertex>> trianglesToRemove = new List<Triangle<Vertex>>();
      //GenerateMapChunkInternal(chunkLength);
      //int startZ = currentEndZ - HeightSegments;
      //for (int i = Vertices.Count - 1; i >= 0; i--)
      //{
      //  if (Vertices[i].ZSegment < startZ)
      //  {
      //    //verticesToRemove.Add(Vertices[i]);
      //    Vertices.Remove(Vertices[i]);
      //  }
      //}
      ////for (int i = 0; i < verticesToRemove.Count; i++)
      ////{
      ////  for (int j = 0; j < Triangles.Count; j++)
      ////  {
      ////    if (Triangles[j].HasVertex(verticesToRemove[i]))
      ////    {
      ////      trianglesToRemove.Add(Triangles[j]);
      ////    }
      ////  }
      ////}
    }

    private void GenerateMapInternal(float fromWorldX, float toWorldX, float fromWorldZ, float toWorldZ)
    {
      tempVertices = new List<Vertex>();
      tempTriangles = new List<Triangle<Vertex>>();
      for (float worldZ = fromWorldZ; worldZ <= toWorldZ; worldZ += HeightSegments)
      {
        Vector3[] dataRow = new Vector3[Mathf.CeilToInt(toWorldX - fromWorldX) / WidthSegments];

        //build basic terrain data row
        int index = 0;
        for (float worldX = fromWorldX; worldX <= toWorldX; worldX += WidthSegments)
        {
          float worldY = (float)terrainScaledModule.GetValue(worldX * WorldScale, 0, worldZ * WorldScale) / WorldScale + 1.1f;
          Vector3 vertex = new Vector3(worldX, worldY, worldZ);
          dataRow[index] = vertex;
          index++;
        }
        //carve river shape
        CarveRiverChannel(dataRow, worldZ);

        //collect modified points and add to vertices array
        for (int x = 0; x < dataRow.Length; x++)
        {
          //check for boundaries
          bool boundary = false;
          if (worldZ == fromWorldZ || worldZ == toWorldZ || dataRow[x].x == fromWorldX || dataRow[x].x == toWorldX)
          {
            boundary = true;
          }
          tempVertices.Add(new Vertex { Position3D = dataRow[x], Id = vertexId++, IsOnBoundary = boundary });
        }
        //for (int x = 0; x < widthSegments; x++)
        //{
        //  uvs.Add(new Vector2(x * scaleX / (float)widthSegments, z * scaleZ / (float)heightSegments));
        //}
      }

      var config = new TriangulationComputationConfig();
      tempTriangles = Triangulation.CreateDelaunay<Vertex, Triangle<Vertex>>(tempVertices, config).Cells.ToList();
      for (int i = tempTriangles.Count - 1; i >= 0; i--)
      {
        tempTriangles[i].ComputeNormal();
        SetBiome(tempTriangles[i], biomesPerlin);
      }
      foreach (Triangle<Vertex> triangle in tempTriangles)
      {
        SplitTriangles(triangle);
      }
    }

    private void CarveRiverChannel(Vector3[] dataRow, float worldZ)
    {
      //generate river channels for Z
      float leftWidth = Mathf.Abs((float)perlinLeftWidth.GetValue(0, 0, worldZ * WorldScale)) * RiverWidthScale + MinimumChannelWidth;
      float rightWidth = Mathf.Abs((float)perlinRightWidth.GetValue(0, 0, worldZ * WorldScale)) * RiverWidthScale + MinimumChannelWidth;

      float leftChannelMiddle = (float)perlinLeft.GetValue(0, 0, worldZ * WorldScale) * RiverScale - ChannelOffset;
      float leftChannelLeftEdge = leftChannelMiddle - leftWidth / 2f;
      float leftChannelRightEdge = leftChannelMiddle + leftWidth / 2f;

      float rightChannelMiddle = (float)perlinRight.GetValue(0, 0, worldZ * WorldScale) * RiverScale + ChannelOffset;
      float rightChannelLeftEdge = rightChannelMiddle - rightWidth / 2f;
      float rightChannelRightEdge = rightChannelMiddle + rightWidth / 2f;

      float midJointChannel = leftChannelLeftEdge * 0.5f + rightChannelRightEdge * 0.5f;

      RiverData riverDataElement = new RiverData
      {
        Z = worldZ,
        JointChannelMiddleLine = midJointChannel,
        LeftChannel = new RiverChannelData
        {
          LeftEdge = leftChannelLeftEdge,
          MiddleLine = leftChannelMiddle,
          RightEdge = leftChannelRightEdge
        },
        RightChannel = new RiverChannelData
        {
          LeftEdge = rightChannelLeftEdge,
          MiddleLine = rightChannelMiddle,
          RightEdge = rightChannelRightEdge
        }
      };
      RiverData.Add(riverDataElement);

      //approximate channel values to nearest segment
      int leftChannelLeftEdgeSegment = Mathf.FloorToInt(leftChannelLeftEdge / WidthSegments) + dataRow.Length / 2;
      int leftChannelMiddleSegment = Mathf.RoundToInt(leftChannelMiddle / WidthSegments) + dataRow.Length / 2;
      int leftChannelRightEdgeSegment = Mathf.CeilToInt(leftChannelRightEdge / WidthSegments) + dataRow.Length / 2;
      int rightChannelLeftEdgeSegment = Mathf.FloorToInt(rightChannelLeftEdge / WidthSegments) + dataRow.Length / 2;
      int rightChannelMiddleSegment = Mathf.RoundToInt(rightChannelMiddle / WidthSegments) + dataRow.Length / 2;
      int rightChannelRightEdgeSegment = Mathf.CeilToInt(rightChannelRightEdge / WidthSegments) + dataRow.Length / 2;
      int midJointChannelSegment = Mathf.RoundToInt(midJointChannel / WidthSegments) + dataRow.Length / 2;

      if (rightChannelLeftEdge < leftChannelRightEdge)
      {
        for (int i = leftChannelLeftEdgeSegment; i < rightChannelRightEdgeSegment; i++)
        {
          dataRow[i] = new Vector3(dataRow[i].x, BottomLevel, dataRow[i].z);
        }
      }
      else {
        for (int i = leftChannelLeftEdgeSegment; i < leftChannelRightEdgeSegment; i++)
        {
          dataRow[i] = new Vector3(dataRow[i].x, BottomLevel, dataRow[i].z);
        }
        for (int i = rightChannelLeftEdgeSegment; i < rightChannelRightEdgeSegment; i++)
        {
          dataRow[i] = new Vector3(dataRow[i].x, BottomLevel, dataRow[i].z);
        }
      }
      bool inWater = dataRow[0].y < WaterLevel;
      for (int i = 1; i < dataRow.Length; i++)
      {
        if (inWater && dataRow[i].y >= WaterLevel)
        {
          inWater = false;
          dataRow[i] = new Vector3(dataRow[i].x, WaterLevel, dataRow[i].z);
        }
        else if (!inWater && dataRow[i].y < WaterLevel)
        {
          inWater = true;
          dataRow[i] = new Vector3(dataRow[i].x, WaterLevel, dataRow[i].z);
        }
      }
    }


    private void SetBiome(Triangle<Vertex> triangle, IModule biomesNoise)
    {
      Vector3 center = (triangle.Vertices[0].Position3D + triangle.Vertices[1].Position3D + triangle.Vertices[2].Position3D) / 3f;
      var noiseValue = biomesNoise.GetValue(center.x, center.y, center.z);
      float normalAngle = Mathf.Abs(Vector3.Angle(triangle.Normal, Vector3.up));
      triangle.Biome = Biomes.Grass;
      float triangleMinY = triangle.MinY();
      float triangleMaxY = triangle.MaxY();
      if (triangleMaxY < WaterLevel)
      {
        if (normalAngle >= CliffAngle)
        {
          triangle.Biome = Biomes.DeepWater;
        }
        else if (triangleMinY > ShallowWaterLevel)
        {
          triangle.Biome = Biomes.ShallowWater;
        }
        else {
          triangle.Biome = Biomes.Water;
        }
      }
      else {
        if (normalAngle >= CliffAngle)
        {
          triangle.Biome = Biomes.Cliff;
        }
        else
        {
          if (normalAngle < CliffAngle && normalAngle >= HillAngle)
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
            if (triangleMinY < WaterLevel && triangleMaxY > WaterLevel)
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


    private void SetVertexColors(Triangle<Vertex> triangle)
    {
      switch (triangle.Biome)
      {
        case Biomes.Grass:
          triangle.Vertices[0].Color = GrassColor;
          triangle.Vertices[1].Color = GrassColor;
          triangle.Vertices[2].Color = GrassColor;
          break;
        case Biomes.DeadGrass:
          triangle.Vertices[0].Color = DeadGrassColor;
          triangle.Vertices[1].Color = DeadGrassColor;
          triangle.Vertices[2].Color = DeadGrassColor;
          break;
        case Biomes.Dirt:
          triangle.Vertices[0].Color = DirtColor;
          triangle.Vertices[1].Color = DirtColor;
          triangle.Vertices[2].Color = DirtColor;
          break;
        case Biomes.Cliff:
          triangle.Vertices[0].Color = CliffColor;
          triangle.Vertices[1].Color = CliffColor;
          triangle.Vertices[2].Color = CliffColor;
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

    private void DuplicateIfSharedAtIndex(Triangle<Vertex> triangle, Vertex toDuplicate)
    {
      int index = triangle.VertexIndex(toDuplicate);
      if (index >= 0)
      {
        Vertex newVertex = new Vertex
        {
          Position3D = new Vector3(toDuplicate.Position3D.x, toDuplicate.Position3D.y, toDuplicate.Position3D.z),
          Xsegment = toDuplicate.Xsegment,
          ZSegment = toDuplicate.ZSegment,
          Id = vertexId++
        };
        triangle.Vertices[index] = newVertex;
        tempVertices.Add(newVertex);
      }
    }

    private void InitGenerators()
    {
      //prepare noise generators
      perlinLeft = new Perlin() { Frequency = RiverShapeFrequency, NoiseQuality = NoiseQuality.Standard, Seed = Seed + 1, OctaveCount = 6, Lacunarity = RiverShapeLacunarity, Persistence = RiverShapePersistence };
      perlinRight = new Perlin() { Frequency = RiverShapeFrequency, NoiseQuality = NoiseQuality.Standard, Seed = Seed + 2, OctaveCount = 6, Lacunarity = RiverShapeLacunarity, Persistence = RiverShapePersistence };
      perlinLeftWidth = new Perlin() { Frequency = RiverWidthFrequency, NoiseQuality = NoiseQuality.Standard, Seed = Seed + 3, OctaveCount = 6, Lacunarity = RiverWidthLacunarity, Persistence = RiverWidthPersistence };
      perlinRightWidth = new Perlin() { Frequency = RiverWidthFrequency, NoiseQuality = NoiseQuality.Standard, Seed = Seed + 4, OctaveCount = 6, Lacunarity = RiverWidthLacunarity, Persistence = RiverWidthPersistence };

      Perlin terrainPerlin = new Perlin() { Frequency = TerrainPerlinFrequency, NoiseQuality = NoiseQuality.Standard, Seed = Seed, OctaveCount = 6, Lacunarity = TerrainPerlinLacunarity, Persistence = TerrainPerlinPersistence };
      RidgedMultifractal terrainRMF = new RidgedMultifractal() { Frequency = TerrainRMFFrequency, NoiseQuality = NoiseQuality.High, Seed = Seed - 1, OctaveCount = 6, Lacunarity = TerrainRMFLacunarity };
      ScaleOutput scaledRMF = new ScaleOutput(terrainRMF, TerrainRMFScale);
      Add terrainAdd = new Add(terrainPerlin, new BiasOutput(scaledRMF, TerrainRMFBias));
      terrainScaledModule = new ScaleOutput(terrainAdd, HeightScale);

      biomesPerlin = new Perlin() { Frequency = BiomesFrequency, NoiseQuality = NoiseQuality.Low, Seed = Seed - 10, OctaveCount = 6, Lacunarity = BiomesLacunarity, Persistence = BiomesPersistence };
    }
  }
}