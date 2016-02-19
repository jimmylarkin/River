using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GroundTile
{
  public int Index { get; set; }

  public Terrain TerrainObject { get; set; }

  public void SetHeightmap(Queue<float[]> groundData, float scaleVertical)
  {
    float[,] heights = new float[TerrainObject.terrainData.heightmapWidth, TerrainObject.terrainData.heightmapHeight];
    for (int i = 0; i < TerrainObject.terrainData.heightmapHeight - 1; i++)
    {
      float[] row = groundData.Dequeue();
      for (int k = 0; k < row.Length; k++)
      {
        heights[i, k] = row[k] / scaleVertical;
      }
    }
    float[] sharedRow = groundData.Peek();
    for (int k = 0; k < sharedRow.Length; k++)
    {
      heights[TerrainObject.terrainData.heightmapHeight - 1, k] = sharedRow[k] / scaleVertical;
    }
    TerrainObject.terrainData.SetHeights(0, 0, heights);
  }
}
