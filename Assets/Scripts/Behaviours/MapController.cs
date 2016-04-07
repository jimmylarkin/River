using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditor;

namespace GrumpyDev.EndlessRiver
{
  public class MapController : MonoBehaviour
  {
    public Map map;
    private float secondsSinceLastMapupdate;

    void Start() {
      secondsSinceLastMapupdate = 0f;
      map = new Map();
      map.Seed = 12299474;
      map.GenerateMapData();
      Mesh mesh = new Mesh();
      var verticesOrdered = map.Vertices.OrderBy(v => v.Position3D.z).OrderBy(v => v.Position3D.x).ToArray();
      int maxId = verticesOrdered.Length;
      for (int i = 0; i < maxId; i++)
      {
        verticesOrdered[i].Id = i;
      }
      mesh.vertices = verticesOrdered.Select(v => v.Position3D).ToArray();
      List<int> triangleIndexes = new List<int>(map.Triangles.Count * 3);
      foreach (var triangle in map.Triangles)
      {
        triangleIndexes.Add(triangle.Vertices[0].Id);
        triangleIndexes.Add(triangle.Vertices[1].Id);
        triangleIndexes.Add(triangle.Vertices[2].Id);
      }
      mesh.triangles = triangleIndexes.ToArray();
      mesh.colors = verticesOrdered.Select(v => v.Color).ToArray();
      mesh.RecalculateBounds();
      mesh.Optimize();
      MeshFilter meshFilter = GetComponent<MeshFilter>();
      meshFilter.sharedMesh = mesh;
    }

    void Update() {
    }

    void FixedUpdate()
    {
      secondsSinceLastMapupdate += Time.deltaTime;
      GenerateMapFragment();
    }

    public void GenerateMapFragment()
    {
      //generate next chunk
      //first row of vertices (first segmentWidth number of them) will have the same positions as the last row of the current map
      map.GenerateMapChunk(2);
      MeshFilter meshFilter = GetComponent<MeshFilter>();
      Mesh mesh = meshFilter.sharedMesh;
      Vector3[] oldVertices = mesh.vertices;
      Vector3[] vertices = new Vector3[oldVertices.Length];
      int[] oldTriangles = mesh.triangles;
      int[] triangles = new int[mesh.triangles.Length];
      int newVerticesCount = map.Vertices.Count - map.WidthSegments;
      int newTrianglesCount = map.Triangles.Count;
      for (int i = 0; i < oldVertices.Length - newVerticesCount; i++)
      {
        vertices[i] = oldVertices[i + newVerticesCount];
      }
      for (int i = 0; i < oldTriangles.Length - newTrianglesCount; i++)
      {
        triangles[i] = oldTriangles[i + newTrianglesCount] - newTrianglesCount;
      }
      mesh.vertices = vertices;
      mesh.triangles = triangles;
    }
  }
}