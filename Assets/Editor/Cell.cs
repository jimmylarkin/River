using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Cell
{
  public int X { get; set; }
  public int Z { get; set; }
  public Vertex VertexA { get; set; }
  public Vertex VertexB { get; set; }
  public Vertex VertexC { get; set; }
  public Vertex VertexD { get; set; }
  public Vertex VertexAB { get; set; }
  public Vertex VertexBC { get; set; }
  public Vertex VertexCD { get; set; }
  public Vertex VertexDA { get; set; }
  public Vertex VertexX { get; set; }
  public List<Triangle> Triangles { get; set; }
  public Cell TopLeft { get; set; }
  public Cell Top { get; set; }
  public Cell TopRight { get; set; }
  public Cell Right { get; set; }
  public Cell BottomRight { get; set; }
  public Cell Bottom { get; set; }
  public Cell BottomLeft { get; set; }
  public Cell Left { get; set; }

  public Cell()
  {
    Triangles = new List<Triangle>();
  }

  public void Tesselate()
  {
    if (VertexA == null || VertexB == null || VertexC == null || VertexD == null)
    {
      return;
    }
    Triangles.Add(new Triangle { Vertex1 = VertexA, Vertex2 = VertexB, Vertex3 = VertexD });
    Triangles.Add(new Triangle { Vertex1 = VertexC, Vertex2 = VertexD, Vertex3 = VertexB });
  }

  public Vertex[] Tesselate2(ref int vertexId)
  {
    List<Vertex> newVertices = new List<Vertex>();
    if (VertexA == null || VertexB == null || VertexC == null || VertexD == null)
    {
      return new Vertex[0];
    }
    float midX = VertexA.Coords.x * 0.5f + VertexB.Coords.x * 0.5f;
    float midZ = VertexB.Coords.z * 0.5f + VertexC.Coords.z * 0.5f;

    //check if side vertices are set already and create if not, populate to neighbours straight away
    if (VertexAB == null)
    {
      VertexAB = new Vertex { Id = vertexId++, Coords = new Vector3(midX, VertexA.Coords.y * 0.5f + VertexB.Coords.y * 0.5f, VertexA.Coords.z) };
      if (Top != null)
      {
        if (Top.VertexCD == null)
        {
          Top.VertexCD = VertexAB;
          newVertices.Add(VertexAB);
        }
        else {
          VertexAB = Top.VertexCD;
        }
      }
      else {
        newVertices.Add(VertexAB);
      }
    }
    if (VertexBC == null)
    {
      VertexBC = new Vertex { Id = vertexId++, Coords = new Vector3(VertexB.Coords.x, VertexB.Coords.y * 0.5f + VertexC.Coords.y * 0.5f, midZ) };
      if (Right != null)
      {
        if (Right.VertexDA == null)
        {
          Right.VertexDA = VertexBC;
          newVertices.Add(VertexBC);
        }
        else {
          VertexBC = Right.VertexDA;
        }
      }
      else {
        newVertices.Add(VertexBC);
      }
    }
    if (VertexCD == null)
    {
      VertexCD = new Vertex { Id = vertexId++, Coords = new Vector3(midX, VertexC.Coords.y * 0.5f + VertexD.Coords.y * 0.5f, VertexC.Coords.z) };
      if (Bottom != null)
      {
        if (Bottom.VertexAB == null)
        {
          Bottom.VertexAB = VertexCD;
          newVertices.Add(VertexCD);
        }
        else {
          VertexCD = Bottom.VertexAB;
        }
      }
      else {
        newVertices.Add(VertexCD);
      }
    }
    if (VertexDA == null)
    {
      VertexDA = new Vertex { Id = vertexId++, Coords = new Vector3(VertexD.Coords.x, VertexD.Coords.y * 0.5f + VertexA.Coords.y * 0.5f, midZ) };
      if (Left != null)
      {
        if (Left.VertexBC == null)
        {
          Left.VertexBC = VertexDA;
          newVertices.Add(VertexDA);
        }
        else {
          VertexDA = Left.VertexBC;
        }
      }
      else {
        newVertices.Add(VertexDA);
      }
    }

    if (Mathf.Abs(VertexA.Coords.y - VertexC.Coords.y) < Mathf.Abs(VertexB.Coords.y - VertexD.Coords.y))
    {
      VertexX = new Vertex { Id = vertexId++, Coords = new Vector3(midX, VertexA.Coords.y * 0.5f + VertexC.Coords.y * 0.5f, midZ) };
    }
    else
    {
      VertexX = new Vertex { Id = vertexId++, Coords = new Vector3(midX, VertexB.Coords.y * 0.5f + VertexD.Coords.y * 0.5f, midZ) };
    }

    Triangles.Add(new Triangle { Vertex1 = VertexA, Vertex2 = VertexAB, Vertex3 = VertexX });
    Triangles.Add(new Triangle { Vertex1 = VertexAB, Vertex2 = VertexB, Vertex3 = VertexX });
    Triangles.Add(new Triangle { Vertex1 = VertexB, Vertex2 = VertexBC, Vertex3 = VertexX });
    Triangles.Add(new Triangle { Vertex1 = VertexBC, Vertex2 = VertexC, Vertex3 = VertexX });
    Triangles.Add(new Triangle { Vertex1 = VertexC, Vertex2 = VertexCD, Vertex3 = VertexX });
    Triangles.Add(new Triangle { Vertex1 = VertexCD, Vertex2 = VertexD, Vertex3 = VertexX });
    Triangles.Add(new Triangle { Vertex1 = VertexD, Vertex2 = VertexDA, Vertex3 = VertexX });
    Triangles.Add(new Triangle { Vertex1 = VertexDA, Vertex2 = VertexA, Vertex3 = VertexX });
    newVertices.Add(VertexX);
    return newVertices.ToArray();
  }

  public void Normalize()
  {
    foreach (Triangle triangle in Triangles)
    {
      Vector3 normal = Vector3.Cross(triangle.Vertex2.Coords - triangle.Vertex1.Coords, triangle.Vertex3.Coords - triangle.Vertex1.Coords);
      normal.Normalize();
      triangle.Vertex1.Normal = normal;
      triangle.Vertex2.Normal = normal;
      triangle.Vertex3.Normal = normal;
    }
  }

  public void DropArea(float deltaY, float minY, float midPointX)
  {
    if (deltaY < 0.1f)
    {
      return;
    }
    if (VertexA == null || VertexB == null || VertexC == null || VertexD == null)
    {
      return;
    }
    if (deltaY > 0)
    {
      VertexA.Coords = new Vector3(VertexA.Coords.x, Mathf.Max(VertexA.Coords.y - deltaY, minY), VertexA.Coords.z);
      VertexB.Coords = new Vector3(VertexB.Coords.x, Mathf.Max(VertexB.Coords.y - deltaY, minY), VertexB.Coords.z);
      VertexC.Coords = new Vector3(VertexC.Coords.x, Mathf.Max(VertexC.Coords.y - deltaY, minY), VertexC.Coords.z);
      VertexD.Coords = new Vector3(VertexD.Coords.x, Mathf.Max(VertexD.Coords.y - deltaY, minY), VertexD.Coords.z);
    }
    float nextDeltaY = deltaY * 0.4f;
    DropNeigbour(Top, nextDeltaY, minY, midPointX);
    DropNeigbour(Right, nextDeltaY, minY, midPointX);
    DropNeigbour(Bottom, nextDeltaY, minY, midPointX);
    DropNeigbour(Left, nextDeltaY, minY, midPointX);

    nextDeltaY = deltaY * 0.3f;
    DropNeigbour(TopLeft, nextDeltaY, minY, midPointX);
    DropNeigbour(TopRight, nextDeltaY, minY, midPointX);
    DropNeigbour(BottomRight, nextDeltaY, minY, midPointX);
    DropNeigbour(BottomLeft, nextDeltaY, minY, midPointX);
  }

  public float DropAreaLeft(Vector3 mid)
  {
    float factorA = Shape(mid, VertexA.Coords);
    float factorB = Shape(mid, VertexB.Coords);
    float factorC = Shape(mid, VertexC.Coords);
    float factorD = Shape(mid, VertexD.Coords);

    float deltaA = (VertexA.Coords.y - -10f) * factorA;
    float deltaB = (VertexB.Coords.y - -10f) * factorB;
    float deltaC = (VertexC.Coords.y - -10f) * factorC;
    float deltaD = (VertexD.Coords.y - -10f) * factorD;
    VertexA.Coords = new Vector3(VertexA.Coords.x, VertexA.Coords.y - deltaA, VertexA.Coords.z);
    VertexB.Coords = new Vector3(VertexB.Coords.x, VertexB.Coords.y - deltaB, VertexB.Coords.z);
    VertexC.Coords = new Vector3(VertexC.Coords.x, VertexC.Coords.y - deltaC, VertexC.Coords.z);
    VertexD.Coords = new Vector3(VertexD.Coords.x, VertexD.Coords.y - deltaD, VertexD.Coords.z);

    float factor = factorA * 0.5f + factorD * 0.5f; 
    if (Left != null)
    {
      while (factor < 0.9f)
      {
        factor = Left.DropAreaLeft(mid);
      }
    }
    return factor;
  }

  public float DropAreaRight(Vector3 mid)
  {
    float factorA = Shape(mid, VertexA.Coords);
    float factorB = Shape(mid, VertexB.Coords);
    float factorC = Shape(mid, VertexC.Coords);
    float factorD = Shape(mid, VertexD.Coords);

    float deltaA = (VertexA.Coords.y - -10f) * factorA;
    float deltaB = (VertexB.Coords.y - -10f) * factorB;
    float deltaC = (VertexC.Coords.y - -10f) * factorC;
    float deltaD = (VertexD.Coords.y - -10f) * factorD;
    VertexA.Coords = new Vector3(VertexA.Coords.x, VertexA.Coords.y - deltaA, VertexA.Coords.z);
    VertexB.Coords = new Vector3(VertexB.Coords.x, VertexB.Coords.y - deltaB, VertexB.Coords.z);
    VertexC.Coords = new Vector3(VertexC.Coords.x, VertexC.Coords.y - deltaC, VertexC.Coords.z);
    VertexD.Coords = new Vector3(VertexD.Coords.x, VertexD.Coords.y - deltaD, VertexD.Coords.z);

    float factor = factorB * 0.5f + factorC * 0.5f;
    if (Right != null)
    {
      while (factor < 0.9f)
      {
        factor = Right.DropAreaRight(mid);
      }
    }
    return factor;
  }

  private float Shape(Vector3 mid, Vector3 point)
  {
    float distance = Mathf.Abs((point - mid).magnitude);
    float factor = Mathf.Exp(distance * distance / 100f * -1f) * -1f + 1f;
    return factor;
  }

  private void DropNeigbour(Cell neigbour, float deltaY, float minY, float midPointX)
  {
    if (neigbour == null)
    {
      return;
    }
    neigbour.DropArea(deltaY, minY, midPointX);
  }
}
