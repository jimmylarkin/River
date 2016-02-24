using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using LibNoise;

namespace River
{
  public class Ground
  {
    private float heightAdjustment = 0;
    private IModule module = null;
    private int heightmapResolution;
    private float heightmapToWorldScale = 1;
    private IModule riverModuleLeft = null;
    private IModule riverModuleRight = null;
    private IModule riverModuleSpread = null;

    private int lastDataRowIndex;
    private int riverCenterPoint = 0;

    public BackgroundWorker Bw { get; private set; }

    public Queue<float[]> GroundData { get; set; }

    public float ScaleVertical { get; set; }

    public int Octaves { get; set; }

    public float Persistence { get; set; }

    public float RiverPersistence { get; set; }

    public float Frequency { get; set; }

    public float RiverFrequency { get; set; }

    public GroundTile[] Tiles { get; set; }

    public float[] LastDataRow { get; set; }

    public float TileWidth { get; set; }

    public float TileHeight { get; set; }

    public Ground()
    {
      Bw = new BackgroundWorker();
      GroundData = new Queue<float[]>(2000);
      Bw.DoWork += Bw_DoWork;
      Bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
      lastDataRowIndex = 0;
    }

    private void Bw_DoWork(object sender, DoWorkEventArgs e)
    {
      int startFrom = lastDataRowIndex + 1;
      int xSize = heightmapResolution;
      int ySize = startFrom + heightmapResolution - 1;
      GenerateData(0, xSize, startFrom, ySize);
    }

    private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      Debug.LogFormat("Bw_RunWorkerCompleted: Data buffer after completion is {0}", GroundData.Count);
    }


    public void Init()
    {
      module = new Perlin();
      ((Perlin)module).Frequency = Frequency;
      ((Perlin)module).NoiseQuality = NoiseQuality.Standard;
      ((Perlin)module).Seed = 0;
      ((Perlin)module).OctaveCount = Octaves;
      ((Perlin)module).Lacunarity = 2.5;
      ((Perlin)module).Persistence = Persistence;

      riverModuleLeft = new Perlin();
      ((Perlin)riverModuleLeft).Frequency = RiverFrequency;
      ((Perlin)riverModuleLeft).NoiseQuality = NoiseQuality.Standard;
      ((Perlin)riverModuleLeft).Seed = 568467;
      ((Perlin)riverModuleLeft).OctaveCount = Octaves;
      ((Perlin)riverModuleLeft).Lacunarity = 2.5;
      ((Perlin)riverModuleLeft).Persistence = RiverPersistence;

      riverModuleRight = new Perlin();
      ((Perlin)riverModuleRight).Frequency = RiverFrequency;
      ((Perlin)riverModuleRight).NoiseQuality = NoiseQuality.Standard;
      ((Perlin)riverModuleRight).Seed = 12312;
      ((Perlin)riverModuleRight).OctaveCount = Octaves;
      ((Perlin)riverModuleRight).Lacunarity = 2.5;
      ((Perlin)riverModuleRight).Persistence = RiverPersistence;

      riverModuleSpread = new Perlin();
      ((Perlin)riverModuleSpread).Frequency = RiverFrequency;
      ((Perlin)riverModuleSpread).NoiseQuality = NoiseQuality.Standard;
      ((Perlin)riverModuleSpread).Seed = 45645;
      ((Perlin)riverModuleSpread).OctaveCount = Octaves;
      ((Perlin)riverModuleSpread).Lacunarity = 2.5;
      ((Perlin)riverModuleSpread).Persistence = RiverPersistence;
    }

    private void GenerateData(int xFrom, int xSize, int yFrom, int ySize)
    {
      for (int y = yFrom; y < ySize; y++)
      {
        float[] xData = new float[xSize];
        for (int x = xFrom; x < xSize; x++)
        {
          float val = (float)((module.GetValue(x, y, 0) + 2) * ScaleVertical + heightAdjustment);
          xData[x] = val;
        }
        lastDataRowIndex++;
        AddRiverShape(xData, y);
        GroundData.Enqueue(xData);
      }
      //Debug.LogFormat("GenerateData: Data buffer after generation is {0}", GroundData.Count);
    }

    private void AddRiverShape(float[] xData, int y)
    {
      float scale = heightmapResolution * 0.25f;
      float lineScale = heightmapResolution * 0.2f;
      float minwidth = 20 / heightmapToWorldScale;
      //GetValue gives value from range roughly -2.5 - + 2.5
      float riverLine = heightmapResolution / 2 + (float)riverModuleLeft.GetValue(20, y + 30, 0) * lineScale;
      float riverWidth = minwidth + ((float)riverModuleRight.GetValue(70, y - 60, 0) + 1) * scale + (float)riverModuleSpread.GetValue(120, y + 90, 0) * scale * 0.5f;
      if (riverWidth < minwidth)
      {
        Debug.LogFormat("riverWidth = {0}, y={1}", riverWidth, y);
        riverWidth = minwidth;
      }
      int riverStart = Mathf.RoundToInt(riverLine - riverWidth / 2);
      int riverEnd = Mathf.RoundToInt(riverLine + riverWidth / 2);
      if (riverStart < 0)
      {
        Debug.LogFormat("riverStart={0}, y={1}", riverStart, y);
        riverStart = 0;
      }
      if (riverEnd > heightmapResolution -1 )
      {
        Debug.LogFormat("riverEnd={0}, y={1}", riverEnd, y);
        riverEnd = heightmapResolution - 1;
      }
      for (int x = riverStart; x <= riverEnd; x++)
      {
        xData[x] = 0f;
      }
      //if (x == riverStart)
      //{
      //  float slopeStart = xData[x - slopeStartDistance];
      //  int actualSlopeStartDistance = slopeStartDistance;
      //  for (int i = slopeStartDistance - 10; i < slopeStartDistance + 10; i++)
      //  {
      //    if (xData[x - i] < slopeStart)
      //    {
      //      slopeStart = xData[x - i];
      //      actualSlopeStartDistance = i;
      //    }
      //  }
      //  if (actualSlopeStartDistance < lastRowSlopeStartDistance)
      //  {
      //    actualSlopeStartDistance = lastRowSlopeStartDistance - 1;
      //  }
      //  if (actualSlopeStartDistance > lastRowSlopeStartDistance)
      //  {
      //    actualSlopeStartDistance = lastRowSlopeStartDistance + 1;
      //  }
      //  slopeStart = xData[x - actualSlopeStartDistance];
      //  float declineRatio = (float)slopeStart / (float)actualSlopeStartDistance;
      //  for (int i = actualSlopeStartDistance; i >= 1; i--)
      //  {
      //    float oldValue = xData[x - i];
      //    float newValue = oldValue - (actualSlopeStartDistance - i) * declineRatio;
      //    xData[x - i] = newValue;
      //  }
      //  val = val - (float)actualSlopeStartDistance * declineRatio;
      //  lastRowSlopeStartDistance = actualSlopeStartDistance;
      //}
      //if (x > riverStart && x < riverEnds)
      //{
      //  val = 0f;
      //}
    }

    public void InitTiles()
    {
      if (Tiles[0] == null)
      {
        throw new InvalidOperationException("First tile is null");
      }
      heightmapResolution = Tiles[0].GetHeightmapResolution();
      heightmapToWorldScale = 500f / (float)heightmapResolution;
      int xSize = heightmapResolution;
      int ySize = heightmapResolution * 3 + 1;
      GenerateData(0, xSize, 0, ySize);
      FindAndSetHeightAdjustment();
      foreach (GroundTile tile in Tiles)
      {
        tile.SetHeightmap(GroundData);
      }
    }

    public void AdvanceTile()
    {
      int maxIndex = 0;
      GroundTile tileToBump = Tiles[0];
      for (int i = 1; i < Tiles.Length; i++)
      {
        if (Tiles[i].Index > maxIndex)
        {
          maxIndex = Tiles[i].Index;
        }
        Tiles[i - 1] = Tiles[i];
      }
      tileToBump.Index = maxIndex + 1;
      tileToBump.TerrainObject.transform.Translate(0, 0, 500 * Tiles.Length);
      tileToBump.SetHeightmap(GroundData);
      Tiles[Tiles.Length - 1] = tileToBump;
    }

    private void FindAndSetHeightAdjustment()
    {
      heightAdjustment = float.MaxValue;
      foreach (float[] floatVar in GroundData)
      {
        for (int i = 0; i < floatVar.Length; i++)
        {
          if (floatVar[i] < heightAdjustment && floatVar[i] != 0)
          {
            heightAdjustment = floatVar[i];
          }
        }
      }
      heightAdjustment = heightAdjustment > 0 ? heightAdjustment - 0.02f : heightAdjustment + 0.02f;
      Debug.LogFormat("Height adjustment={0}", heightAdjustment);
      foreach (float[] floatVar in GroundData)
      {
        for (int i = 0; i < floatVar.Length; i++)
        {
          floatVar[i] = floatVar[i] - heightAdjustment;

        }
      }
    }
  }
}