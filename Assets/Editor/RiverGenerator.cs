using LibNoise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Shores
{
  public float Left { get; set; }
  public float Right { get; set; }
}

public class RiverPathData
{
  public float Z { get; set; }
  public Shores Shore { get; set; }
}

public class RiverGenerator
{
  private float scale = 40f;
  private int octaves = 6;
  private float persistence = 0.75f;
  private float frequency = 0.005f;
  private Perlin perlinLeft;
  private Perlin perlinRight;
  private Perlin perlinDistance;

  public RiverGenerator()
  {
    perlinLeft = new Perlin();
    perlinLeft.Frequency = frequency;
    perlinLeft.NoiseQuality = NoiseQuality.Standard;
    perlinLeft.Seed = DateTime.Now.Millisecond;
    perlinLeft.OctaveCount = octaves;
    perlinLeft.Lacunarity = 1.3;
    perlinLeft.Persistence = persistence;

    perlinRight = new Perlin();
    perlinRight.Frequency = frequency;
    perlinRight.NoiseQuality = NoiseQuality.Standard;
    perlinRight.Seed = DateTime.Now.Millisecond * 2;
    perlinRight.OctaveCount = octaves;
    perlinRight.Lacunarity = 1.4;
    perlinRight.Persistence = persistence;

    perlinDistance = new Perlin();
    perlinDistance.Frequency = frequency;
    perlinDistance.NoiseQuality = NoiseQuality.Standard;
    perlinDistance.Seed = DateTime.Now.Millisecond / 2;
    perlinDistance.OctaveCount = octaves;
    perlinDistance.Lacunarity = 1.2;
    perlinDistance.Persistence = persistence;
  }


  public RiverPathData Generate(float z)
  {
    RiverPathData result = new RiverPathData { Z = z };
    float distance = (float)perlinDistance.GetValue(0, 0, z) * scale;
    float leftShore = (float)perlinLeft.GetValue(0, 0, z + 100) * scale;
    float rightShore = (float)perlinRight.GetValue(0, 0, z + 200) * scale;
    result.Shore = new Shores { Left = leftShore - distance, Right = leftShore + distance };
    return result;
  }
}

