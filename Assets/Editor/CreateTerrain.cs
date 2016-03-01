using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class CreateTerrain : ScriptableWizard
{
  private static Camera cam;
  private static Camera lastUsedCam;

  private List<Vector3> vertices;
  private List<Vector3> normals;
  private List<int> triangles;
  private List<Vector4> tangents;
  private List<Vector2> uvs;
  private List<Color> colors;

  public int widthSegments = 10;
  public int heightSegments = 10;
  public int width = 1;
  public int height = 1;
  public bool varyVertices = true;

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
    vertices = new List<Vector3>();
    normals = new List<Vector3>();
    tangents = new List<Vector4>();
    uvs = new List<Vector2>();
    colors = new List<Color>();
    triangles = new List<int>();
    GenerateMeshData();
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
    mesh.colors = colors.ToArray();
    mesh.uv = uvs.ToArray();
    mesh.tangents = tangents.ToArray();
    meshFilter.sharedMesh = mesh;
    mesh.RecalculateBounds();
    mesh.Optimize();
    Selection.activeObject = plane;
  }


  public void GenerateMeshData()
  {
    float scaleX = (float)width / (float)widthSegments;
    float scaleZ = (float)height / (float)heightSegments;
    float variationX = scaleX / 10f;
    float variationZ = scaleZ / 10f;
    for (int z = 0; z < heightSegments; z++)
    {
      for (int x = 0; x < widthSegments; x++)
      {
        float modifiedX = x * scaleX;
        float modifiedZ = z * scaleZ;
        if (varyVertices)
        {
          modifiedX += Random.Range(variationX * -1f, variationX);
          modifiedZ += Random.Range(variationZ * -1f, variationZ);
        }
        vertices.Add(new Vector3(modifiedX, 0f, modifiedZ));
        Vector4 tangent = new Vector4(modifiedX, 0f, 0f, -1f);
        tangent.Normalize();
        tangents.Add(tangent);
        uvs.Add(new Vector2(x * scaleX / (float)widthSegments, z * scaleZ / (float)heightSegments));
        //colors.Add(Color.Lerp(Color.red, Color.green, (float)x / (float)widthSegments));
        if ((z % 2 == 0 && x % 2 != 0) || (z % 2 != 0 && x % 2 == 0))
        {
          CreateTrianglesAroundVertex(z * widthSegments + x);
        }
      }
    }
    for (int z = 0; z < heightSegments; z++)
    {
      for (int x = 0; x < widthSegments; x++)
      {
        int actualZ = z <= 1 ? 1 : z;
        int actualX = x <= 1 ? 1 : x;
        Vector3 thisVertex = vertices[actualZ * widthSegments + actualX];
        Vector3 leftvertex = vertices[(actualZ - 1) * widthSegments + actualX];
        Vector3 rightvertex = vertices[actualZ * widthSegments + actualX - 1];
        Vector3 normal = Vector3.Cross(leftvertex - thisVertex, rightvertex - thisVertex);
        normal.Normalize();
        normals.Add(normal);
      }
    }
  }

  private void CreateTrianglesAroundVertex(int vertexIndex)
  {
    bool firstRow = vertexIndex < widthSegments;
    bool lastRow = vertexIndex > (widthSegments * heightSegments - widthSegments - 1);
    bool leftcolumn = vertexIndex % widthSegments == 0;
    bool rightColumn = (vertexIndex + 1) % widthSegments == 0;

    if (firstRow)
    {
      //first row and left column are invalid for this algorithm so not checkign it
      if (!rightColumn)
      {
        //bottom right
        TestVerticesAndAddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + widthSegments);
      }
      //bottom left
      TestVerticesAndAddTriangle(vertexIndex, vertexIndex + widthSegments, vertexIndex - 1);
    }
    else if (lastRow)
    {
      if (leftcolumn)
      {
        //top right
        TestVerticesAndAddTriangle(vertexIndex, vertexIndex - widthSegments, vertexIndex + 1);
      }
      else
      if (rightColumn)
      {
        //top left
        TestVerticesAndAddTriangle(vertexIndex, vertexIndex - 1, vertexIndex - widthSegments);
      }
      else {
        //top left
        TestVerticesAndAddTriangle(vertexIndex, vertexIndex - 1, vertexIndex - widthSegments);
        //top right
        TestVerticesAndAddTriangle(vertexIndex, vertexIndex - widthSegments, vertexIndex + 1);
      }
    }
    else
    {
      if (leftcolumn)
      {
        //top right
        TestVerticesAndAddTriangle(vertexIndex, vertexIndex - widthSegments, vertexIndex + 1);
        //bottom right
        TestVerticesAndAddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + widthSegments);
      }
      else
      if (rightColumn)
      {
        //top left
        TestVerticesAndAddTriangle(vertexIndex, vertexIndex - 1, vertexIndex - widthSegments);
        //bottom left
        TestVerticesAndAddTriangle(vertexIndex, vertexIndex + widthSegments, vertexIndex - 1);
      }
      else
      {
        //top left
        TestVerticesAndAddTriangle(vertexIndex, vertexIndex - 1, vertexIndex - widthSegments);
        //top right
        TestVerticesAndAddTriangle(vertexIndex, vertexIndex - widthSegments, vertexIndex + 1);
        //bottom right
        TestVerticesAndAddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + widthSegments);
        //bottom left
        TestVerticesAndAddTriangle(vertexIndex, vertexIndex + widthSegments, vertexIndex - 1);
      }
    }
  }
  private void TestVerticesAndAddTriangle(int index1, int index2, int index3)
  {
    triangles.Add(index3);
    triangles.Add(index2);
    triangles.Add(index1);
  }
}
