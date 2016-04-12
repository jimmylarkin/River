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
    private int ownerId;
    private MeshFilter meshFilter;
    private Mesh mesh;
    public Map map;
    private float secondsSinceLastMapupdate;
    private bool isDone = false;
    private bool isReady = false;
    private float z = 0;

    public MeshFilter MeshFilter {
      get
      {
        meshFilter = meshFilter ?? GetComponent<MeshFilter>();
        meshFilter = meshFilter ?? gameObject.AddComponent<MeshFilter>();
        return meshFilter;
      }
    }

    public Mesh Mesh
    {
      get {
        bool isOwner = ownerId == gameObject.GetInstanceID();
        if (MeshFilter.sharedMesh == null || !isOwner)
        {
          MeshFilter.sharedMesh = mesh = new Mesh();
          ownerId = gameObject.GetInstanceID();
          mesh.name = "Mesh [" + ownerId + "]";
        }
        return mesh;
      }
    }
    

    void Start()
    {
      Debug.Log("Starting");
      secondsSinceLastMapupdate = 0f;
      //MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
      //MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
      map = new Map();
      map.Seed = 12299474;
      map.GenerateMapData(-50, 50, z, z + 100);
      //Mesh mesh = new Mesh();
      Mesh.Clear();
      Vector3[] vertices = null;
      int[] triangles = null;
      map.GetMeshRawData(z, z + 100, ref vertices, ref triangles);
      Mesh.vertices = vertices;
      Mesh.triangles = triangles;
      Mesh.RecalculateBounds();
      Mesh.Optimize();
      //meshFilter.mesh = mesh;
      Debug.Log("Generated starting point");
      isReady = true;
    }

    void Update()
    {
    }

    void FixedUpdate()
    {
      secondsSinceLastMapupdate += Time.deltaTime;
      if (isReady)
      {
        if (secondsSinceLastMapupdate > 1)
        {
          secondsSinceLastMapupdate = 0;
          z += 5;
          GenerateMapFragment();
          isDone = true;
        }
      }
    }

    public void GenerateMapFragment()
    {
      ////generate next chunk
      map.GenerateMapData(-50, 50, z, z + 100);
      //Mesh mesh = new Mesh();
      Mesh.Clear();
      Vector3[] vertices = null;
      int[] triangles = null;
      map.GetMeshRawData(z, z + 100, ref vertices, ref triangles);
      Mesh.vertices = vertices;
      Mesh.triangles = triangles;
      Mesh.RecalculateBounds();
      Mesh.Optimize();
      Debug.Log("--> chunk done");
    }
  }
}