using UnityEngine;
using System.Collections;

public class TerrainController : MonoBehaviour
{
  public float scaleHorizontal = 3000f;
  public float scaleVertical = 500f;
  public int octaves = 8;
  public float startAmplitude = 128;
  public float startFrequency = 4;
  PerlinNoise gen = new PerlinNoise();

  private 

  // Use this for initialization
  void Start()
  {
    for (int i = 0; i < transform.childCount; i++)
    {
      var terrainGameObject = transform.GetChild(i).gameObject;
      SetTerrainHeights(terrainGameObject, i);
    }
  }

  public void PlacenewTile(float cameraPositionZ)
  {
    
  }

  private void SetTerrainHeights(GameObject terrainGameObject, int offsetMultiplier)
  {
    Terrain terrain = terrainGameObject.GetComponent<Terrain>();
    float[,] heights = new float[terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight];
    int offset = offsetMultiplier * terrain.terrainData.heightmapHeight;
    float startWidth = 0f + (float)offset;
    float startHeight = 0f;
    for (int i = 0; i < terrain.terrainData.heightmapWidth; i++)
    {
      for (int k = 0; k < terrain.terrainData.heightmapHeight; k++)
      {
        float val = (float)gen.OctavePerlin(((float)i + startWidth) / scaleHorizontal, ((float)k + startHeight) / scaleHorizontal, 0, octaves, startAmplitude, startFrequency) / scaleVertical;
        heights[i, k] = val;
      }
    }
    terrain.terrainData.SetHeights(0, 0, heights);
  }
}
