using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using LibNoise;
using LibNoise.Modifiers;

namespace River
{
  public class Ground
  {
    private int lastDataRowIndex;

    public Perlin Perlin1 { get; set; }

    public BackgroundWorker Bw { get; private set; }

    public List<Vector4> GroundData { get; set; }

    public float ScaleVertical { get; set; }

    public GroundTile[] Tiles { get; set; }

    public float[] LastDataRow { get; set; }

    public Ground()
    {
      Perlin1 = new Perlin()
      {
        Frequency = 0.015f,
        NoiseQuality = NoiseQuality.Standard,
        Seed = 0,
        OctaveCount = 6,
        Lacunarity = 2.5,
        Persistence = 0.35f
      };
      GroundData = new List<Vector4>();
      //Bw = new BackgroundWorker();
      //Bw.DoWork += Bw_DoWork;
      //Bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
      lastDataRowIndex = 0;
    }

    private void Bw_DoWork(object sender, DoWorkEventArgs e)
    {
      //int startFrom = lastDataRowIndex + 1;
      //int xSize = heightmapResolution;
      //int ySize = startFrom + heightmapResolution - 1;
      //GenerateData(0, xSize, startFrom, ySize);
    }

    private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {

    }

    public void GenerateData(int xFrom, int xSize, int zFrom, int zSize)
    {
      Billow billow = new Billow();
      billow.Frequency = Perlin1.Frequency;
      billow.NoiseQuality = NoiseQuality.High;
      billow.Seed = 1;
      billow.OctaveCount = 6;
      billow.Lacunarity = Perlin1.Lacunarity;
      billow.Persistence = Perlin1.Persistence;

      RidgedMultifractal rmf = new RidgedMultifractal();
      rmf.Frequency = Perlin1.Frequency;
      rmf.NoiseQuality = NoiseQuality.High;
      rmf.Seed = 2;
      rmf.OctaveCount = 6;
      rmf.Lacunarity = 5;

      Select module = new Select(Perlin1, rmf, billow) { EdgeFalloff = 0 };
      module.SetBounds(-0.75, 0.75);

      Add addModule = new Add(Perlin1, rmf);

      ScaleOutput scaledModule = new ScaleOutput(addModule, ScaleVertical);

      //this is to set zero level and green color only, to be removed later
      for (int z = zFrom; z < zSize; z++)
      {
        for (int x = xFrom; x < xSize; x++)
        {
          float val = (float)(scaledModule.GetValue(x, z, 0));
          GroundData.Add(new Vector4(val, 0f, 104f / 256f, 10f / 256f));
        }
        lastDataRowIndex++;
      }
    }

    private void AddRiverShape(float[] xData, int y)
    {
      //float scale = heightmapResolution * 0.25f;
      //float lineScale = heightmapResolution * 0.2f;
      //float minwidth = 20 / heightmapToWorldScale;
      ////GetValue gives value from range roughly -2.5 - + 2.5
      //float riverLine = heightmapResolution / 2 + (float)riverModuleLeft.GetValue(20, y + 30, 0) * lineScale;
      //float riverWidth = minwidth + ((float)riverModuleRight.GetValue(70, y - 60, 0) + 1) * scale + (float)riverModuleSpread.GetValue(120, y + 90, 0) * scale * 0.5f;
      //if (riverWidth < minwidth)
      //{
      //  Debug.LogFormat("riverWidth = {0}, y={1}", riverWidth, y);
      //  riverWidth = minwidth;
      //}
      //int riverStart = Mathf.RoundToInt(riverLine - riverWidth / 2);
      //int riverEnd = Mathf.RoundToInt(riverLine + riverWidth / 2);
      //if (riverStart < 0)
      //{
      //  Debug.LogFormat("riverStart={0}, y={1}", riverStart, y);
      //  riverStart = 0;
      //}
      //if (riverEnd > heightmapResolution - 1)
      //{
      //  Debug.LogFormat("riverEnd={0}, y={1}", riverEnd, y);
      //  riverEnd = heightmapResolution - 1;
      //}
      //for (int x = riverStart; x <= riverEnd; x++)
      //{
      //  xData[x] = 0f;
      //}
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
      //if (Tiles[0] == null)
      //{
      //  throw new InvalidOperationException("First tile is null");
      //}
      //heightmapResolution = Tiles[0].GetHeightmapResolution();
      //heightmapToWorldScale = 500f / (float)heightmapResolution;
      //int xSize = heightmapResolution;
      //int ySize = heightmapResolution * 3 + 1;
      //GenerateData(0, xSize, 0, ySize);
      //FindAndSetHeightAdjustment();
      //foreach (GroundTile tile in Tiles)
      //{
      //  tile.SetHeightmap(GroundData);
      //}
    }

    public void AdvanceTile()
    {
      //int maxIndex = 0;
      //GroundTile tileToBump = Tiles[0];
      //for (int i = 1; i < Tiles.Length; i++)
      //{
      //  if (Tiles[i].Index > maxIndex)
      //  {
      //    maxIndex = Tiles[i].Index;
      //  }
      //  Tiles[i - 1] = Tiles[i];
      //}
      //tileToBump.Index = maxIndex + 1;
      //tileToBump.TerrainObject.transform.Translate(0, 0, 500 * Tiles.Length);
      //tileToBump.SetHeightmap(GroundData);
      //Tiles[Tiles.Length - 1] = tileToBump;
    }
  }
}