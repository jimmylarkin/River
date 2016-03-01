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
    public int width = 10;
    public int height = 10;
    Ground ground;

    private

    // Use this for initialization
    void Awake()
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
      ground.Init();
      ground.GenerateData(0, width, 0, height);
      MeshFilter meshFilter = transform.gameObject.GetComponent<MeshFilter>();
      if (meshFilter == null)
      {
        Debug.LogError("The TerrainController can be applied to mesh only");
      }
      Mesh mesh = meshFilter.sharedMesh;
      Debug.Log(mesh.bounds);
      List<Vector3> newVertices = new List<Vector3>(mesh.vertices.Length);
      List<Vector3> newNormals = new List<Vector3>(mesh.normals.Length);
      int vertexIndex = 0;
      while (ground.GroundData.Count > 0)
      {
        float data = ground.GroundData.Dequeue();
        newVertices.Add(new Vector3(mesh.vertices[vertexIndex].x, data, mesh.vertices[vertexIndex].z));
        vertexIndex++;
      }

      for (int z = 0; z < width; z++)
      {
        for (int x = 0; x < height; x++)
        {
          int actualZ = z <= 1 ? 1 : z;
          int actualX = x <= 1 ? 1 : x;
          Vector3 thisVertex = newVertices[actualZ * width + actualX];
          Vector3 leftvertex = newVertices[(actualZ - 1) * width + actualX];
          Vector3 rightvertex = newVertices[actualZ * width + actualX - 1];
          Vector3 normal = Vector3.Cross(leftvertex - thisVertex, rightvertex - thisVertex);
          normal.Normalize();
          newNormals.Add(normal);
        }
      }
      mesh.vertices = newVertices.ToArray();
      mesh.normals = newNormals.ToArray();
      mesh.RecalculateBounds();
      mesh.Optimize();
      Debug.Log(mesh.bounds);
    }

    void Start() {
    }

    public void AdvanceTile()
    {
      //ground.AdvanceTile();
      //ground.Bw.RunWorkerAsync();
    }
  }
}