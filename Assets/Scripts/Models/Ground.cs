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
    private PerlinNoise gen = new PerlinNoise();
    public BackgroundWorker Bw { get; private set; }

    private int lastDataRowIndex;

    public Queue<float[]> GroundData { get; set; }

    public float ScaleHorizontal { get; set; }

    public float ScaleVertical { get; set; }

    public int Octaves { get; set; }

    public float StartAmplitude { get; set; }

    public float StartFrequency { get; set; }

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
      int length = startFrom + 512;
      try
      {
        float[][] data = gen.Generate(0, 513, startFrom, length, ScaleHorizontal, Octaves, StartAmplitude, StartFrequency);
        e.Result = data;
      }
      catch (Exception ex)
      {
        Debug.Log(ex.StackTrace);
      }
    }

    private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      float[][] data = (float[][])e.Result;
      Debug.LogFormat("Bw_RunWorkerCompleted: Result containms {0} data rows", data.Length);
      for (int i = 0; i < data.Length; i++)
      {
        lastDataRowIndex++;
        GroundData.Enqueue(data[i]);
      }
      Debug.LogFormat("Bw_RunWorkerCompleted: Data buffer length is now {0}", GroundData.Count);
    }


    public void Init()
    {
      int length = 513 + 512 + 512;
      float[][] data = gen.Generate(0, 513, 0, length, ScaleHorizontal, Octaves, StartAmplitude, StartFrequency);
      for (int i = 0; i < length; i++)
      {
        lastDataRowIndex++;
        GroundData.Enqueue(data[i]);
      }
    }

    public void Init2()
    {
      IModule module = new Perlin();
      ((Perlin)module).Frequency = 0.008;
      ((Perlin)module).NoiseQuality = NoiseQuality.High;
      ((Perlin)module).Seed = 0;
      ((Perlin)module).OctaveCount = 6;
      ((Perlin)module).Lacunarity = 2.5;
      ((Perlin)module).Persistence = 0.35;
      LibNoise.Models.Sphere sphere = new LibNoise.Models.Sphere(module);

      int xSize = 513;
      int ySize = 513 + 512 + 512;
      for (int y = 0; y < ySize; y++)
      {
        float[] xData = new float[xSize];
        for (int x = 0; x < xSize; x++)
        {
          float val = (float)(module.GetValue(x , y, 0) + 5) * 10f;
          xData[x] = val;
        }
        lastDataRowIndex++;
        GroundData.Enqueue(xData);
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
      //tileToBump.ResetTileHeightData();
      //tileToBump.GenerateTileHeightData(gen, this);
      Tiles[Tiles.Length - 1] = tileToBump;
      Bw.RunWorkerAsync(lastDataRowIndex);
    }
  }
}