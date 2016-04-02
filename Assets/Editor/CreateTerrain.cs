using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using LibNoise;

public class CreateTerrain : ScriptableWizard
{
  private static Camera cam;
  private static Camera lastUsedCam;

  public int widthSegments = 100;
  public int heightSegments = 100;
  public int width = 100;
  public int height = 100;
  public float worldScale = 10;

  public string optionalName = "Terrain";

  [MenuItem("GameObject/Create Other/Terrain mesh...")]
  static void CreateWizard()
  {
    cam = Camera.current;
    // Hack because camera.current doesn't return editor camera if scene view doesn't have focus
    if (!cam)
    {
      cam = lastUsedCam;
    }
    else
    {
      lastUsedCam = cam;
    }
    ScriptableWizard.DisplayWizard("Create Terrain", typeof(CreateTerrain));
  }

  void OnWizardCreate()
  {
    GameObject plane = new GameObject();
    if (!string.IsNullOrEmpty(optionalName))
    {
      plane.name = optionalName;
    }
    else
    {
      plane.name = "Terrain";
    }
    plane.transform.position = Vector3.zero;
    MeshFilter meshFilter = plane.AddComponent<MeshFilter>();
    plane.AddComponent<MeshRenderer>();
    TerrainMeshGenerator generator = new TerrainMeshGenerator();
    generator.height = height;
    generator.width = width;
    generator.heightSegments = heightSegments + 1;
    generator.widthSegments = widthSegments + 1;
    generator.worldScale = worldScale;
    Mesh mesh = generator.Createmesh();
    meshFilter.sharedMesh = mesh;
    Selection.activeObject = plane;
  }
}
