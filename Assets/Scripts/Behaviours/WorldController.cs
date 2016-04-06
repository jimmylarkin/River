using UnityEngine;
using System.Collections;

namespace GrumpyDev.EndlessRiver
{
  public class WorldController : MonoBehaviour
  {
    public GameObject mainCamera;
    public GameObject terrainController;
    public float speed = 30;  //world units per second

    private int cameraOverTileIndex;
    private float terrainTileSize = 500;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
      var distance = Time.deltaTime * speed;
      mainCamera.transform.Translate(0, 0, distance, Space.World);
      var newCameraOverTileIndex = Mathf.FloorToInt(mainCamera.transform.position.z / terrainTileSize);
      TerrainController controller = terrainController.GetComponent<TerrainController>();
      if (newCameraOverTileIndex > cameraOverTileIndex)
      {
        controller.AdvanceTile();
        cameraOverTileIndex = newCameraOverTileIndex;
      }
    }
  }
}