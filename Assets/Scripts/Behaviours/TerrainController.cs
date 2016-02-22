using UnityEngine;
using System.Collections;

namespace River
{
  public class TerrainController : MonoBehaviour
  {
    public float scaleHorizontal = 3000f;
    public float scaleVertical = 500f;
    public int octaves = 8;
    public float startAmplitude = 128;
    public float startFrequency = 4;
    Ground ground;

    private

    // Use this for initialization
    void Start()
    {
      ground = new Ground() { ScaleHorizontal = scaleHorizontal, ScaleVertical = scaleVertical, Octaves = octaves, StartAmplitude = startAmplitude, StartFrequency = startFrequency };
      ground.Init2();
      Debug.LogFormat("Start: Data buffer length is now {0}", ground.GroundData.Count);
      ground.Tiles = new GroundTile[transform.childCount];
      for (int i = 0; i < transform.childCount; i++)
      {
        GameObject childgameObject = transform.GetChild(i).gameObject;
        Terrain terrain = childgameObject.GetComponent<Terrain>();
        if (terrain != null)
        {
          ground.Tiles[i] = new GroundTile() { TerrainObject = terrain };
          ground.Tiles[i].SetHeightmap(ground.GroundData, scaleVertical);
        }
      }
      Debug.LogFormat("Start: Data buffer length is now {0}", ground.GroundData.Count);
    }

    public void AdvanceTile()
    {
      Debug.LogFormat("AdvanceTile: Data buffer length is now {0}", ground.GroundData.Count);
      ground.AdvanceTile();
      ground.Tiles[ground.Tiles.Length - 1].SetHeightmap(ground.GroundData, scaleVertical);
      Debug.LogFormat("AdvanceTile: Data buffer length is now {0}", ground.GroundData.Count);
    }

    public void HeightmapSweep()
    {
      //ground.GenerateTerrainStripe(10);
    }
  }
}