using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LibNoise;

namespace River.CSharpTests.Assets.Scripts.Models
{
  [TestClass]
  public class LibNoiseTests
  {
    [TestMethod]
    public void TestMethod1()
    {
      float min = float.MaxValue;
      float max = float.MinValue;
      IModule module = new FastNoise();
      ((FastNoise)module).Frequency = 0.05;
      ((FastNoise)module).NoiseQuality = NoiseQuality.High;
      ((FastNoise)module).Seed = 0; 
      ((FastNoise)module).OctaveCount = 6;
      ((FastNoise)module).Lacunarity = 2.0;
      ((FastNoise)module).Persistence = 0.5;
      LibNoise.Models.Sphere sphere = new LibNoise.Models.Sphere(module);

      int xSize = 513;
      int ySize = 513 + 512 + 512;
      float[][] yData = new float[ySize][];
      for (int y = 0; y < ySize; y++)
      {
        float[] xData = new float[xSize];
        for (int x = 0; x < xSize; x++)
        {
          float val = (float)module.GetValue(x, y, 0);
          if (val > max)
          {
            max = val;
          }
          if (val < min)
          {
            min = val;
          }
          xData[x] = val;
        }
        yData[y] = xData;
      }
    }
  }
}
