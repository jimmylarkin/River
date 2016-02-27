using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LibNoise;
using System.Drawing;
using System.Drawing.Imaging;
using LibNoise.Modifiers;

namespace River.CSharpTests.Assets.Scripts.Models
{
  [TestClass]
  public class LibNoiseTests
  {
    [TestMethod]
    public void NoiseLineTest()
    {
      for (int o = 1; o < 6; o++)
      {
        for (double f = 0.001; f < 0.01; f += 0.001)
        {
          for (double p = 0.05; p < 0.5; p += 0.05)
          {
            Run(o, 0, f, 0.5);
          }
        }
      }
    }

    private static void Run(int octaves, double persistence, double frequency, double lacunarity)
    {
      string fileName = string.Format("O{3}_P{0:N3}_F{1:N3}.png", persistence, frequency, lacunarity, octaves);
      Billow billow = new Billow();
      billow.Frequency = frequency;
      billow.NoiseQuality = NoiseQuality.High;
      billow.Seed = 0;
      billow.OctaveCount = octaves;
      billow.Lacunarity = lacunarity;
      billow.Persistence = persistence;

      ScaleBiasOutput scaledBillow = new ScaleBiasOutput(billow);
      scaledBillow.Bias = -0.75;
      scaledBillow.Scale = 0.125;

      RidgedMultifractal ridged = new RidgedMultifractal();
      ridged.Frequency = frequency;
      ridged.NoiseQuality = NoiseQuality.High;
      ridged.Seed = 0;
      ridged.OctaveCount = octaves;
      ridged.Lacunarity = lacunarity;

      Perlin perlin = new Perlin();
      perlin.Frequency = frequency;
      perlin.NoiseQuality = NoiseQuality.High;
      perlin.Seed = 0;
      perlin.OctaveCount = octaves;
      perlin.Lacunarity = lacunarity;
      perlin.Persistence = persistence;

      Select selector = new Select(perlin, ridged, scaledBillow);
      selector.SetBounds(0, 1000);
      selector.EdgeFalloff = 0.5;
      IModule module = selector;
      //RidgedMultifractal module = new RidgedMultifractal();
      //((RidgedMultifractal)module).Frequency = frequency;
      //((RidgedMultifractal)module).NoiseQuality = NoiseQuality.High;
      //((RidgedMultifractal)module).Seed = 0;
      //((RidgedMultifractal)module).OctaveCount = octaves;
      //((RidgedMultifractal)module).Lacunarity = lacunarity;
      //((RidgedMultifractal)module).Persistence = persistence;
      Pen blackPen = new Pen(Color.Black, 1);
      Font arialFont = new Font("Arial", 8);

      double min = double.MaxValue;
      double max = double.MinValue;
      double avg = 0;

      using (Bitmap b = new Bitmap(300, 2500))
      {
        using (Graphics g = Graphics.FromImage(b))
        {
          g.Clear(Color.White);
          for (int i = 0; i < 5000; i++)
          {
            var value = module.GetValue(i, 0, 0);
            avg += value;
            if (value < min)
            {
              min = value;
            }
            if (value > max)
            {
              max = value;
            }
            g.DrawRectangle(blackPen, 150 + (int)System.Math.Round(value * 100), i / 2, 1, 1);
          }
          avg /= 5000;
          g.DrawString(string.Format("Samples: {0:N3}\r\nMin: {1:N3}\r\nMax: {2:N3}\r\nAverage: {3:N3}", 5000, min, max, avg), arialFont, Brushes.DarkBlue, 5f, 60f);
          g.DrawString(string.Format("Frequency: {0:N3}\r\nPersistence: {1:N3}\r\nLacunarity: {2:N3}\r\nOctaves: {3:N3}", frequency, persistence, lacunarity, octaves), arialFont, Brushes.DarkGreen, 5f, 5f);
        }
        b.Save(@"Combined\" + fileName, ImageFormat.Png);
      }
    }
  }
}
