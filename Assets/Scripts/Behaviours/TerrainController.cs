using UnityEngine;
using System.Collections;

namespace River
{
  public class TerrainController : MonoBehaviour
  {
    public float scaleVertical = 0.02f;
    public int octaves = 6;
    public float persistence = 0.35f;
    public float frequency = 0.008f;
    public float riverPersistence = 0.35f;
    public float riverFrequency = 0.008f;
    Ground ground;

    private

    // Use this for initialization
    void Start()
    {
      ground = new Ground()
      {
        ScaleVertical = scaleVertical,
        Octaves = octaves,
        Persistence = persistence,
        Frequency = frequency,
        RiverFrequency = riverFrequency,
        RiverPersistence = riverPersistence
      };
      //Debug.LogFormat("Start: Starting buffer length is {0}", ground.GroundData.Count);
      ground.Tiles = new GroundTile[transform.childCount];
      for (int i = 0; i < transform.childCount; i++)
      {
        GameObject childgameObject = transform.GetChild(i).gameObject;
        Terrain terrain = childgameObject.GetComponent<Terrain>();
        if (terrain != null)
        {
          ground.Tiles[i] = new GroundTile() { TerrainObject = terrain };
        }
      }
      ground.Init();
      ground.InitTiles();
      //Debug.LogFormat("Start: Data buffer after start is {0}", ground.GroundData.Count);
    }

    public void AdvanceTile()
    {
      //Debug.LogFormat("AdvanceTile: Data buffer before settign heightmaps is {0}", ground.GroundData.Count);
      ground.AdvanceTile();
      //Debug.LogFormat("AdvanceTile: Data buffer after setting heightmaps is {0}", ground.GroundData.Count);
      ground.Bw.RunWorkerAsync();
    }
  }
}