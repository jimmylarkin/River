using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ground
{
  private PerlinNoise gen = new PerlinNoise();

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
    GroundData = new Queue<float[]>(2000);
  }

  public void Init()
  {
    int length = 513 + 512;
    float[][] data = gen.Generate(0, 513, 0, length, ScaleHorizontal, Octaves, StartAmplitude, StartFrequency);
    for (int i = 0; i < length; i++)
    {
      GroundData.Enqueue(data[i]);
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
    //tileToBump.TerrainObject.transform.Translate(0, 0, tileToBump.Height * Tiles.Length);
    //tileToBump.ResetTileHeightData();
    //tileToBump.GenerateTileHeightData(gen, this);
    Tiles[Tiles.Length - 1] = tileToBump;
  }
}
