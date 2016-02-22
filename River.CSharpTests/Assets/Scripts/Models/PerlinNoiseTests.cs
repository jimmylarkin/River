using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using River;

namespace Tests
{
  [TestClass()]
  public class PerlinNoiseTests
  {
    [TestMethod()]
    public void PerlinReturnsTheSameValueForTheSameCoordinates()
    {
      PerlinNoise tm = new PerlinNoise();
      var test = tm.perlin(0.5f, 0.5f, 0);
      var test2 = tm.perlin(0.5f, 0.5f, 0);
      Assert.AreEqual(test, test2);
    }

    [TestMethod()]
    public void PerlinReturnsDifferentValuesForDifferentCoordinates()
    {
      PerlinNoise tm = new PerlinNoise();
      var test = tm.perlin(0f / 513, 0f / 513, 0);
      var test2 = tm.perlin(0f / 513, 1f / 513, 0);
      Assert.AreNotEqual(test, test2);
    }

    [TestMethod()]
    public void OctavePerlinReturnsTheSameValueForTheSameCoordinates()
    {
      PerlinNoise tm = new PerlinNoise();
      var test = tm.OctavePerlin(0.5f, 0.5f, 0, 1, 128, 4);
      var test2 = tm.OctavePerlin(0.5f, 0.5f, 0, 1, 128, 4);
      Assert.AreEqual(test, test2);
    }

    [TestMethod()]
    public void OctavePerlinReturnsDifferentValuesForDifferentCoordinates()
    {
      PerlinNoise tm = new PerlinNoise();
      var test = tm.OctavePerlin(0.5f, 0.5f, 0, 1, 128, 4);
      var test2 = tm.OctavePerlin(0.6f, 0.5f, 0, 1, 128, 4);
      Assert.AreNotEqual(test, test2);
    }

    [TestMethod]
    public void GenerateReturnsArrayOfData()
    {
      PerlinNoise tm = new PerlinNoise();
      float[][] data = tm.Generate(0, 10, 0, 20, 10, 8, 128, 4);
    }

    [TestMethod]
    public void MinMaxTest()
    {
      float min = float.MaxValue;
      float max = float.MinValue;
      PerlinNoise tm = new PerlinNoise();
      for (int y = 0; y < 20000; y++)
      {
        for (int x = 0; x < 512; x++)
        {
          float val = (float)tm.OctavePerlin((float)x / 3000f, (float)y / 3000f, 0, 8, 128, 4);
          if (val > max)
          {
            max = val;
          }
          if (val < min)
          {
            min = val;
          }
        }
      }
    }
  }
}