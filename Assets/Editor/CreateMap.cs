using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using LibNoise;
using GrumpyDev.EndlessRiver;


namespace GrumpyDev.EndlessRiver.Editor
{
  public class CreateMap : ScriptableWizard
  {
    public int seed = 12299474;

    private static Camera cam;
    private static Camera lastUsedCam;

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
      ScriptableWizard.DisplayWizard("Create Map", typeof(CreateMap));
    }

    void OnWizardCreate()
    {
      GameObject plane = new GameObject();
      plane.name = "Map";
      plane.transform.position = Vector3.zero;
      MeshFilter meshFilter = plane.AddComponent<MeshFilter>();
      MeshRenderer renderer = plane.AddComponent<MeshRenderer>();
      //renderer.materials = new Material[8];
      Map map = new Map();
      map.Seed = seed;
      //map.GenerateMapData();
      Mesh mesh = new Mesh();
      map.GenerateMapData(-50, 50, 0, 200);
      Vector3[] vertices = null;
      int[] triangles = null;
      map.GetMeshRawData(0, 200, ref vertices, ref triangles);
      mesh.vertices = vertices;
      mesh.triangles = triangles;
      mesh.RecalculateBounds();
      mesh.Optimize();
      meshFilter.sharedMesh = mesh;
      var mapController = plane.AddComponent<MapController>();
      mapController.map = map;
      Selection.activeObject = plane;
    }

    public Mesh Createmesh()
    {
      Map map = new Map();
      map.Seed = seed;
      map.GenerateMapData(-50, 50, 0, 200);
      Mesh mesh = new Mesh();
      Vector3[] vertices = null;
      int[] triangles = null;
      map.GetMeshRawData(0, 200, ref vertices, ref triangles);
      mesh.vertices = vertices;
      mesh.triangles = triangles;
      //mesh.subMeshCount = 8;
      //GetTrianglesByBiome(triangleIndexes, Biomes.Grass);
      //mesh.SetTriangles(triangleIndexes.ToArray(), 0);
      //GetTrianglesByBiome(triangleIndexes, Biomes.Dirt);
      //mesh.SetTriangles(triangleIndexes.ToArray(), 1);
      //GetTrianglesByBiome(triangleIndexes, Biomes.DeadGrass);
      //mesh.SetTriangles(triangleIndexes.ToArray(), 2);
      //GetTrianglesByBiome(triangleIndexes, Biomes.Cliff);
      //mesh.SetTriangles(triangleIndexes.ToArray(), 3);
      //GetTrianglesByBiome(triangleIndexes, Biomes.Sand);
      //mesh.SetTriangles(triangleIndexes.ToArray(), 4);
      //GetTrianglesByBiome(triangleIndexes, Biomes.Water);
      //mesh.SetTriangles(triangleIndexes.ToArray(), 5);
      //GetTrianglesByBiome(triangleIndexes, Biomes.ShallowWater);
      //mesh.SetTriangles(triangleIndexes.ToArray(), 6);
      //GetTrianglesByBiome(triangleIndexes, Biomes.DeepWater);
      //mesh.SetTriangles(triangleIndexes.ToArray(), 7);
      //mesh.triangles = triangleIndexes.ToArray();
      //mesh.normals = verticesOrdered.Select(v => v.Normal).ToArray();
      //mesh.uv = uvs.ToArray();
      //mesh.tangents = tangents.ToArray();
      mesh.RecalculateBounds();
      mesh.Optimize();
      return mesh;
    }

    //private void GetTrianglesByBiome(List<int> triangleIndexes, Biomes biomes)
    //{
    //  triangleIndexes.Clear();
    //  foreach (var triangle in Triangles.Where(t => t.Biome == biomes))
    //  {
    //    triangleIndexes.Add(triangle.Vertices[0].Id);
    //    triangleIndexes.Add(triangle.Vertices[1].Id);
    //    triangleIndexes.Add(triangle.Vertices[2].Id);
    //  }
    //}
  }
}