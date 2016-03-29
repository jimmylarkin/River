using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using LibNoise;
using LibNoise.Modifiers;
using System.Diagnostics;

[DebuggerDisplay("{Vertex1} | {Vertex2} | {Vertex3}")]
public class Triangle
{
  public Vertex Vertex1 { get; set; }
  public Vertex Vertex2 { get; set; }
  public Vertex Vertex3 { get; set; }
}

[DebuggerDisplay("{Id} ({Coords})")]
public class Vertex
{
  public int Id { get; set; }
  public Vector3 Coords { get; set; }
  public Vector3 Normal { get; set; }
}

public class TerrainMeshGenerator
{
  private List<Cell> cells;
  private List<Vertex> vertices;
  private List<int> triangles;
  private List<Vector4> tangents;
  private List<Vector2> uvs;
  private List<Color> colors;

  public int widthSegments;
  public int heightSegments;
  public int width;
  public int height;


  private float scale = 10.0f;
  private int octaves = 6;
  private float persistence = 0.35f;
  private float frequency = 0.008f;
  private int vertexIndex;
  private float waterlevel = -10.0f;

  public TerrainMeshGenerator()
  {
    vertices = new List<Vertex>();
    tangents = new List<Vector4>();
    uvs = new List<Vector2>();
    colors = new List<Color>();
    triangles = new List<int>();
    cells = new List<Cell>();
  }

  public Mesh Createmesh()
  {
    vertexIndex = 0;
    GenerateMeshData();
    Mesh mesh = new Mesh();
    Vertex[] verticesOrdered = vertices.OrderBy(v => v.Id).ToArray();
    mesh.vertices = verticesOrdered.Select(v => v.Coords).ToArray();
    foreach (Triangle triangle in cells.SelectMany(c => c.Triangles))
    {
      triangles.Add(triangle.Vertex1.Id);
      triangles.Add(triangle.Vertex2.Id);
      triangles.Add(triangle.Vertex3.Id);
    }
    mesh.triangles = triangles.ToArray();
    mesh.normals = verticesOrdered.Select(v => v.Normal).ToArray();
    //mesh.colors = colors.ToArray();
    //mesh.uv = uvs.ToArray();
    //mesh.tangents = tangents.ToArray();
    mesh.RecalculateBounds();
    mesh.Optimize();
    return mesh;
  }

  public void GenerateMeshData()
  {
    RiverGenerator riverGen = new RiverGenerator();
    Perlin perlin = new Perlin()
    {
      Frequency = frequency,
      NoiseQuality = NoiseQuality.Standard,
      Seed = 0,
      OctaveCount = 6,
      Lacunarity = 2.5,
      Persistence = persistence
    };
    //perlin.Frequency = frequency;
    //perlin.NoiseQuality = NoiseQuality.Standard;
    //perlin.Seed = 0;
    //perlin.OctaveCount = octaves;
    //perlin.Lacunarity = 2.5;
    //perlin.Persistence = persistence;

    Billow billow = new Billow();
    billow.Frequency = perlin.Frequency;
    billow.NoiseQuality = NoiseQuality.High;
    billow.Seed = 1;
    billow.OctaveCount = 6;
    billow.Lacunarity = perlin.Lacunarity;
    billow.Persistence = perlin.Persistence;

    RidgedMultifractal rmf = new RidgedMultifractal();
    rmf.Frequency = perlin.Frequency;
    rmf.NoiseQuality = NoiseQuality.High;
    rmf.Seed = 2;
    rmf.OctaveCount = 6;
    rmf.Lacunarity = 5;

    Select module = new Select(perlin, rmf, billow) { EdgeFalloff = 0 };
    module.SetBounds(-0.75, 0.75);

    Add addModule = new Add(perlin, rmf);

    ScaleOutput scaledModule = new ScaleOutput(addModule, scale);

    float scaleX = (float)width / ((float)widthSegments - 1);
    float scaleZ = (float)height / ((float)heightSegments - 1);

    //GENERATE CELLS
    for (int z = 0; z < heightSegments; z++)
    {
      float worldZ = z * scaleZ;
      for (int x = 0; x < widthSegments; x++)
      {
        float worldX = (x - widthSegments / 2) * scaleX;
        float worldY = (float)scaledModule.GetValue(worldX, 0, worldZ);
        CreateCell(x, z, new Vector3(worldX, worldY, worldZ), x == widthSegments - 1 || z == heightSegments - 1);

        //if (scaledX > riverData.Shore.Left && scaledX < riverData.Shore.Right)
        //{
        //  scaledY = -1 * scale;
        //}
        //TesselateCell(x, z, scaledX, scaledY, scaledZ, scaleX, scaleZ);
        //        vertices.Add(new Vector4(scaledX, scaledY, scaledZ));
        //Vector4 tangent = new Vector4(worldX, 0f, 0f, -1f);
        //tangent.Normalize();
        //tangents.Add(tangent);
        //uvs.Add(new Vector2(x * scaleX / (float)widthSegments, z * scaleZ / (float)heightSegments));
        //colors.Add(Color.Lerp(Color.red, Color.green, (float)x / (float)widthSegments));
      }


      //RiverPathData rowRiverData = riverGen.Generate(worldZ);
    }

    //CREATE RIVER SHAPE
    for (int z = 0; z < heightSegments; z++)
    {
      float worldZ = z * scaleZ;
      RiverPathData riverData = riverGen.Generate(worldZ);
      Cell leftCell = FindCell(riverData.Shore.Left, z);
      if (leftCell != null && leftCell.VertexA != null && leftCell.VertexB != null && leftCell.VertexC != null && leftCell.VertexD != null)
      {
        float midY = leftCell.VertexA.Coords.y * 0.25f + leftCell.VertexB.Coords.y * 0.25f + leftCell.VertexC.Coords.y * 0.25f + leftCell.VertexD.Coords.y * 0.25f;
        float midZ = leftCell.VertexA.Coords.z * 0.5f + leftCell.VertexD.Coords.z * 0.5f;
        leftCell.DropAreaLeft(new Vector3(riverData.Shore.Left, midY, midZ));
        leftCell.DropAreaRight(new Vector3(riverData.Shore.Left, midY, midZ));
        //  leftCell = leftCell.Right;
        //  float midY = leftCell.VertexA.Coords.y * 0.25f + leftCell.VertexB.Coords.y * 0.25f + leftCell.VertexC.Coords.y * 0.25f + leftCell.VertexD.Coords.y * 0.25f;
        //  float deltaY = midY - waterlevel;
        //  leftCell.DropArea(deltaY, waterlevel + 2f, riverData.Shore.Left);
        //  leftCell = leftCell.Left;
      }
      Cell rightCell = FindCell(riverData.Shore.Right, z);
      if (rightCell != null && rightCell.VertexA != null && rightCell.VertexB != null && rightCell.VertexC != null && rightCell.VertexD != null)
      {
        float midY = rightCell.VertexA.Coords.y * 0.25f + rightCell.VertexB.Coords.y * 0.25f + rightCell.VertexC.Coords.y * 0.25f + rightCell.VertexD.Coords.y * 0.25f;
        float midZ = rightCell.VertexA.Coords.z * 0.5f + rightCell.VertexD.Coords.z * 0.5f;
        leftCell.DropAreaLeft(new Vector3(riverData.Shore.Right, midY, midZ));
        rightCell.DropAreaRight(new Vector3(riverData.Shore.Right, midY, midZ));
        //  rightCell = rightCell.Left;
        //  float midY = rightCell.VertexA.Coords.y * 0.25f + rightCell.VertexB.Coords.y * 0.25f + rightCell.VertexC.Coords.y * 0.25f + rightCell.VertexD.Coords.y * 0.25f;
        //  float deltaY = midY - waterlevel;
        //  rightCell.DropArea(deltaY, waterlevel + 2f, riverData.Shore.Right);
        //  rightCell = rightCell.Left;
      }
    }

    //CREATE TRIANGLES FOR EACH CELL
    foreach (Cell cell in cells)
    {
      var newVertices = cell.Tesselate2(ref vertexIndex);
      vertices.AddRange(newVertices);
    }
    //NORMALS
    foreach (Cell cell in cells)
    {
      cell.Normalize();
    }
  }



  private void CreateCell(int x, int z, Vector3 vertex, bool createReferencesOnly)
  {
    Cell leftCell = null;
    Cell bottomCell = null;
    Cell bottomLeftCell = null;
    Cell newCell = new Cell { X = x, Z = z };
    Vertex newVertex = new Vertex { Id = vertexIndex++, Coords = vertex };
    newCell.VertexD = newVertex;
    if (x > 0)
    {
      leftCell = FindCell(x - 1, z);
      if (leftCell != null)
      {
        leftCell.VertexC = newVertex;
      }
    }
    if (z > 0)
    {
      bottomCell = FindCell(x, z - 1);
      if (bottomCell != null)
      {
        bottomCell.VertexA = newVertex;
      }
      if (x > 0)
      {
        bottomLeftCell = FindCell(x - 1, z - 1);
        if (bottomLeftCell != null)
        {
          bottomLeftCell.VertexB = newVertex;
        }
      }
    }
    if (!createReferencesOnly)
    {
      if (leftCell != null)
      {
        newCell.Left = leftCell;
        leftCell.Right = newCell;
      }
      if (bottomCell != null)
      {
        newCell.Bottom = bottomCell;
        bottomCell.Top = newCell;
      }
      if (bottomLeftCell != null)
      {
        newCell.BottomLeft = bottomLeftCell;
        bottomLeftCell.TopRight = newCell;
      }
      Cell bottomRightCell = FindCell(x + 1, z - 1);
      if (bottomRightCell != null)
      {
        newCell.BottomRight = bottomRightCell;
        bottomRightCell.TopLeft = newCell;
      }
      cells.Add(newCell);
    }
    vertices.Add(newVertex);
  }

  //private void TesselateCell(int x, int z, float scaledX, float scaledY, float scaledZ, float scaleX, float scaleZ)
  //{
  //  int wt = widthSegments * 2 - 1;

  //  int indA = z * 2 * wt + 2 * x;   //to create
  //  int indAB = indA - wt;     //to create
  //  int indB = indAB - wt;

  //  int indDA = indA - 1;     //DA
  //  int indX = indAB - 1;     //X
  //  int indBC = indB - 1;

  //  int indD = indDA - 1;
  //  int indCD = indX - 1;
  //  int indC = indBC - 1;

  //  float yA = scaledY;
  //  float yC = FindVertex(indC).y;
  //  float yB = FindVertex(indB).y;
  //  float yD = FindVertex(indD).y;
  //  float yDA = 0.5f * yD + 0.5f * yA;
  //  float yAB = 0.5f * yB + 0.5f * yA;
  //  float yX = 0.25f * yA + 0.25f * yB + 0.25f * yC + 0.25f * yD;

  //  if (z == 0)
  //  {
  //    //  for z == 0 and x = 0:
  //    //   (A )
  //    vertices.Add(new Vector4(scaledX, yA, scaledZ, indA));  //A               
  //    if (x > 0)
  //    {
  //      //  for z == 0 and x > 0:
  //      //   D   (DA)    (A )        
  //      vertices.Add(new Vector4(scaledX - scaleX / 2, yDA, scaledZ, indDA));  //DA
  //    }
  //  }
  //  else {
  //    //for z > 0
  //    //for x == 0:
  //    //   (A )
  //    //   (AB)
  //    //    B   
  //    vertices.Add(new Vector4(scaledX, yA, scaledZ, indA));
  //    vertices.Add(new Vector4(scaledX, yAB, scaledZ - scaleZ / 2, indAB));
  //    if (x > 0)
  //    {
  //      //  for x > 0:
  //      //   D---(DA) -(A )
  //      //   |1\2 |3 /4 |        
  //      //   CD--(X )--(AB)
  //      //   |8/ 7| 6\5 |        
  //      //   C----BC -- B

  //      vertices.Add(new Vector4(scaledX - scaleX / 2, yDA, scaledZ, indDA));
  //      vertices.Add(new Vector4(scaledX - scaleX / 2, yX, scaledZ - scaleZ / 2, indX));

  //      AddTriangle(indCD, indD, indX); //1
  //      AddTriangle(indD, indDA, indX); //2
  //      AddTriangle(indDA, indA, indX); //3
  //      AddTriangle(indA, indAB, indX); //4
  //      AddTriangle(indAB, indB, indX); //5
  //      AddTriangle(indB, indBC, indX); //6
  //      AddTriangle(indBC, indC, indX); //7
  //      AddTriangle(indC, indCD, indX); //8
  //    }
  //  }
  //}

  private Vertex FindVertex(int id)
  {
    for (int i = vertices.Count - 1; i >= 0; i--)
    {
      if (vertices[i].Id == id)
      {
        return vertices[i];
      }
    }
    return null;
  }

  private Cell FindCell(int x, int z)
  {
    for (int i = cells.Count - 1; i >= 0; i--)
    {
      if (cells[i].X == x && cells[i].Z == z)
      {
        return cells[i];
      }
    }
    return null;
  }

  private Cell FindCell(float worldX, int z)
  {
    for (int i = cells.Count - 1; i >= 0; i--)
    {
      if (cells[i].Z == z)
      {
        if (cells[i].VertexD != null && cells[i].VertexC != null && cells[i].VertexD.Coords.x <= worldX && cells[i].VertexC.Coords.x >= worldX)
        {
          return cells[i];
        }
      }
    }
    return null;
  }
  private void AddTriangle(int vertexIndex1, int vertexIndex2, int vertexIndex3)
  {
    triangles.Add(vertexIndex1);
    triangles.Add(vertexIndex2);
    triangles.Add(vertexIndex3);
  }
}
