using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using LibNoise;

namespace River
{
  public class Ground
  {
    private IModule module = null;

    private IModule riverModuleLeft = null;
    private IModule riverModuleRight = null;
    private IModule riverModuleSpread = null;

    private int lastDataRowIndex;

    public BackgroundWorker Bw { get; private set; }

    public Queue<float[]> GroundData { get; set; }

    public float ScaleVertical { get; set; }

    public int Octaves { get; set; }

    public float Persistence { get; set; }

    public float Frequency { get; set; }

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
      int xSize = 513;
      int ySize = startFrom + 512;
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
      ((Perlin)module).NoiseQuality = NoiseQuality.High;
      ((Perlin)module).Seed = 0;
      ((Perlin)module).OctaveCount = Octaves;
      ((Perlin)module).Lacunarity = 2.5;
      ((Perlin)module).Persistence = Persistence;

      riverModuleLeft = new Perlin();
      ((Perlin)riverModuleLeft).Frequency = Frequency;
      ((Perlin)riverModuleLeft).NoiseQuality = NoiseQuality.Standard;
      ((Perlin)riverModuleLeft).Seed = 568467;
      ((Perlin)riverModuleLeft).OctaveCount = Octaves;
      ((Perlin)riverModuleLeft).Lacunarity = 2.5;
      ((Perlin)riverModuleLeft).Persistence = Persistence;

      riverModuleRight = new Perlin();
      ((Perlin)riverModuleRight).Frequency = Frequency;
      ((Perlin)riverModuleRight).NoiseQuality = NoiseQuality.Standard;
      ((Perlin)riverModuleRight).Seed = 12312;
      ((Perlin)riverModuleRight).OctaveCount = Octaves;
      ((Perlin)riverModuleRight).Lacunarity = 2.5;
      ((Perlin)riverModuleRight).Persistence = Persistence;

      riverModuleSpread = new Perlin();
      ((Perlin)riverModuleSpread).Frequency = Frequency;
      ((Perlin)riverModuleSpread).NoiseQuality = NoiseQuality.Standard;
      ((Perlin)riverModuleSpread).Seed = 45645;
      ((Perlin)riverModuleSpread).OctaveCount = Octaves;
      ((Perlin)riverModuleSpread).Lacunarity = 2.5;
      ((Perlin)riverModuleSpread).Persistence = Persistence;

      int xSize = 513;
      int ySize = 512 * 3 + 1;
      GenerateData(0, xSize, 0, ySize);
    }

    private void GenerateData(int xFrom, int xSize, int yFrom, int ySize)
    {
      int slopeStartDistance = 150;
      //int lastRowSlopeStartDistance = slopeStartDistance;
      Debug.LogFormat("GenerateData: Generating data for tile {0},{1} {2},{3}", xFrom, xSize, yFrom, ySize);
      for (int y = yFrom; y < ySize; y++)
      {
        int riverStart = Mathf.FloorToInt((float)riverModuleLeft.GetValue(0, y, 0) * 30f) + 200;
        int riverEnds = riverStart + Mathf.FloorToInt((float)(riverModuleSpread.GetValue(0, y + 50, 0) + 5) * 30f + (float)riverModuleRight.GetValue(0, y - 30, 0) * 30f);
        float[] xData = new float[xSize];
        for (int x = xFrom; x < xSize; x++)
        {
          float val = (float)(module.GetValue(x, y, 0) + 2);
          if (x == riverStart)
          {
            float slopeStart = xData[x - slopeStartDistance];
            int actualSlopeStartDistance = slopeStartDistance;
            //for (int i = slopeStartDistance - 10; i < slopeStartDistance + 10; i++)
            //{
            //  if (xData[x - i] < slopeStart)
            //  {
            //    slopeStart = xData[x - i];
            //    actualSlopeStartDistance = i;
            //  }
            //}
            //if (actualSlopeStartDistance < lastRowSlopeStartDistance)
            //{
            //  actualSlopeStartDistance = lastRowSlopeStartDistance - 1;
            //}
            //if (actualSlopeStartDistance > lastRowSlopeStartDistance)
            //{
            //  actualSlopeStartDistance = lastRowSlopeStartDistance + 1;
            //}
            //slopeStart = xData[x - actualSlopeStartDistance];
            float declineRatio = (float)slopeStart / (float)actualSlopeStartDistance;
            for (int i = actualSlopeStartDistance; i >= 1; i--)
            {
              float oldValue = xData[x - i];
              float newValue = oldValue - (actualSlopeStartDistance - i) * declineRatio;
              xData[x - i] = newValue;
            }
            val = val - (float)actualSlopeStartDistance * declineRatio;
            //lastRowSlopeStartDistance = actualSlopeStartDistance;
          }
          if (x > riverStart && x < riverEnds)
          {
            val = 0f;
          }
          xData[x] = val;
        }
        lastDataRowIndex++;
        GroundData.Enqueue(xData);
      }
      Debug.LogFormat("GenerateData: Data buffer after generation is {0}", GroundData.Count);
    }

    private void GenerateRiver()
    {
      
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
      Tiles[Tiles.Length - 1] = tileToBump;
    }
  }
}