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
  public int heightSegments = 200;
  public int width = 100;
  public int height = 200;
  public float worldScale = 10;
  public float angle;

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
    MeshRenderer renderer = plane.AddComponent<MeshRenderer>();
    //renderer.materials = new Material[8];
    TerrainMeshGenerator generator = new TerrainMeshGenerator();
    generator.height = height;
    generator.width = width;
    generator.heightSegments = heightSegments + 1;
    generator.widthSegments = widthSegments + 1;
    generator.worldScale = worldScale;
    generator.angle = angle;
    Mesh mesh = generator.Createmesh();
    meshFilter.sharedMesh = mesh;
    Selection.activeObject = plane;
  }
}
