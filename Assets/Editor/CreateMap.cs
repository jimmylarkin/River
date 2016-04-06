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
      mesh.colors = verticesOrdered.Select(v => v.Color).ToArray();
      //mesh.normals = verticesOrdered.Select(v => v.Normal).ToArray();
      //mesh.uv = uvs.ToArray();
      //mesh.tangents = tangents.ToArray();
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
      mesh.colors = verticesOrdered.Select(v => v.Color).ToArray();
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