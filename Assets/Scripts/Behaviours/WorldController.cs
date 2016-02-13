using UnityEngine;
using System.Collections;

public class WorldController : MonoBehaviour
{
  public GameObject camera;
  public GameObject terrainController;
  public float speed = 30;  //world units per second

  private float terrainTileSize = 500;

  // Use this for initialization
  void Start()
  {
  }

  // Update is called once per frame
  void Update()
  {
    var distance = Time.deltaTime * speed;
    camera.transform.Translate(0, distance, 0);
  }
}
