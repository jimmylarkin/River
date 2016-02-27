using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class CreateTerrain : ScriptableWizard
{
  private static Camera cam;
  private static Camera lastUsedCam;

  public int widthSegments = 100;
  public int heightSegments = 100;
  public int width = 100;
  public int height = 100;

  public float scaleVertical = 10f;
  public int octaves = 6;
  public float persistence = 0.35f;
  public float frequency = 0.008f;
  public float riverPersistence = 0.35f;
  public float riverFrequency = 0.008f;

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
    List<Vector3> vertices = new List<Vector3>(widthSegments * heightSegments);
    List<Vector3> normals = new List<Vector3>(widthSegments * heightSegments);
    List<Vector4> tangents = new List<Vector4>(widthSegments * heightSegments);
    List<Vector2> uvs = new List<Vector2>(widthSegments * heightSegments);
    List<Color> colors = new List<Color>(widthSegments * heightSegments);
    List<int> triangles = new List<int>();
    River.Ground ground = new River.Ground()
    {
      ScaleVertical = scaleVertical,
      Octaves = octaves,
      Persistence = persistence,
      Frequency = frequency,
      RiverFrequency = riverFrequency,
      RiverPersistence = riverPersistence
    };
    ground.Init();
    ground.widthSegments = widthSegments;
    ground.heightSegments = heightSegments;
    ground.width = width;
    ground.height = height;
    ground.vertices = vertices;
    ground.normals = normals;
    ground.triangles = triangles;
    ground.tangents = tangents;
    ground.uvs = uvs;
    ground.colors = colors;
    ground.GenerateMeshData();
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
    MeshRenderer meshRenderer = plane.AddComponent<MeshRenderer>();
    Mesh mesh = new Mesh();
    mesh.vertices = vertices.ToArray();
    mesh.triangles = triangles.ToArray();
    mesh.normals = normals.ToArray();
    mesh.tangents = tangents.ToArray();
    mesh.colors = colors.ToArray();
    mesh.uv = uvs.ToArray();
    mesh.RecalculateNormals();
    meshFilter.sharedMesh = mesh;
    mesh.RecalculateBounds();
    mesh.Optimize();
    Selection.activeObject = plane;
  }
}
