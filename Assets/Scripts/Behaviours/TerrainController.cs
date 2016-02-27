using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
      //ground = new Ground()
      //{
      //  ScaleVertical = scaleVertical,
      //  Octaves = octaves,
      //  Persistence = persistence,
      //  Frequency = frequency,
      //  RiverFrequency = riverFrequency,
      //  RiverPersistence = riverPersistence
      //};

      //ground.Tiles = new GroundTile[transform.childCount];
      //for (int i = 0; i < transform.childCount; i++)
      //{
      //  GameObject childgameObject = transform.GetChild(i).gameObject;
      //  Terrain terrain = childgameObject.GetComponent<Terrain>();
      //  if (terrain != null)
      //  {
      //    ground.Tiles[i] = new GroundTile() { TerrainObject = terrain };
      //  }
      //}
      //ground.Init();
      //ground.InitTiles();
    }

    public void AdvanceTile()
    {
      //ground.AdvanceTile();
      //ground.Bw.RunWorkerAsync();
    }
  }
}