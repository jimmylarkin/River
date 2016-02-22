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
    private IModule module = new Perlin();

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
      ((Perlin)module).Frequency = Frequency;
      ((Perlin)module).NoiseQuality = NoiseQuality.High;
      ((Perlin)module).Seed = 0;
      ((Perlin)module).OctaveCount = Octaves;
      ((Perlin)module).Lacunarity = 2.5;
      ((Perlin)module).Persistence = Persistence;
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
      int xSize = 513;
      int ySize = 512 * 3 + 1;
      GenerateData(0, xSize, 0, ySize);
    }

    private void GenerateData(int xFrom, int xSize, int yFrom, int ySize)
    {
      Debug.LogFormat("GenerateData: Generating data for tile {0},{1} {2},{3}", xFrom, xSize, yFrom, ySize);
      for (int y = yFrom; y < ySize; y++)
      {
        float[] xData = new float[xSize];
        for (int x = xFrom; x < xSize; x++)
        {
          float val = (float)(module.GetValue(x, y, 0) + 2) * ScaleVertical;
          if (x > 240 && x < 260)
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