using LibNoise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ChannelData
{
  public float Left { get; set; }
  public float Right { get; set; }
}

public class RiverPathData
{
  public float Z { get; set; }
  public ChannelData LeftChannel { get; set; }
  public ChannelData RightChannel { get; set; }
}

public class RiverGenerator
{
  private float scale = 40f;
  private int octaves = 6;
  private float persistence = 0.75f;
  private float frequency = 0.005f;
  private Perlin perlinLeft;
  private Perlin perlinRight;
  private Perlin perlinLeftWidth;
  private Perlin perlinRightWidth;
  private int seed = 1000;

  public RiverGenerator()
  {
    perlinLeft = new Perlin();
    perlinLeft.Frequency = frequency;
    perlinLeft.NoiseQuality = NoiseQuality.Standard;
    perlinLeft.Seed = seed;
    perlinLeft.OctaveCount = octaves;
    perlinLeft.Lacunarity = 1.3;
    perlinLeft.Persistence = persistence;

    perlinRight = new Perlin();
    perlinRight.Frequency = frequency;
    perlinRight.NoiseQuality = NoiseQuality.Standard;
    perlinRight.Seed = seed * 2;
    perlinRight.OctaveCount = octaves;
    perlinRight.Lacunarity = 1.4;
    perlinRight.Persistence = persistence;

    perlinLeftWidth = new Perlin();
    perlinLeftWidth.Frequency = frequency;
    perlinLeftWidth.NoiseQuality = NoiseQuality.Standard;
    perlinLeftWidth.Seed = seed / 5;
    perlinLeftWidth.OctaveCount = octaves;
    perlinLeftWidth.Lacunarity = 1.2;
    perlinLeftWidth.Persistence = persistence;

    perlinRightWidth = new Perlin();
    perlinRightWidth.Frequency = frequency;
    perlinRightWidth.NoiseQuality = NoiseQuality.Standard;
    perlinRightWidth.Seed = seed * 3;
    perlinRightWidth.OctaveCount = octaves;
    perlinRightWidth.Lacunarity = 1.2;
    perlinRightWidth.Persistence = persistence;
  }


  public RiverPathData Generate(float z)
  {
    RiverPathData result = new RiverPathData { Z = z };
    float leftWidth = Mathf.Abs((float)perlinLeftWidth.GetValue(0, 0, z)) * scale + scale;
    float rightWidth = Mathf.Abs((float)perlinRightWidth.GetValue(0, 0, z)) * scale + scale;
    float leftShore = (float)perlinLeft.GetValue(0, 0, z + 100) * scale - scale * 0.7f;
    float rightShore = (float)perlinRight.GetValue(0, 0, z + 200) * scale + scale * 0.7f;
    result.LeftChannel = new ChannelData { Left = leftShore, Right = leftShore + leftWidth };
    result.RightChannel = new ChannelData { Left = rightShore, Right = rightShore + rightWidth };
    return result;
  }
}

