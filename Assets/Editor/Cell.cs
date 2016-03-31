using MIConvexHull;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

  public class GridCell
{
  public int X { get; set; }
  public int Z { get; set; }
  public Vertex VertexA { get; set; }
  public Vertex VertexB { get; set; }
  public Vertex VertexC { get; set; }
  public Vertex VertexD { get; set; }
  public GridCell TopLeft { get; set; }
  public GridCell Top { get; set; }
  public GridCell TopRight { get; set; }
  public GridCell Right { get; set; }
  public GridCell BottomRight { get; set; }
  public GridCell Bottom { get; set; }
  public GridCell BottomLeft { get; set; }
  public GridCell Left { get; set; }

  public GridCell()
  {
  }

  public float DropAreaLeft(Vector3 mid, float waterLevel)
  {
    float factorA = Shape(mid, VertexA.Coords);
    float factorB = Shape(mid, VertexB.Coords);
    float factorC = Shape(mid, VertexC.Coords);
    float factorD = Shape(mid, VertexD.Coords);

    float deltaA = (VertexA.Coords.y - waterLevel) * factorA;
    float deltaB = (VertexB.Coords.y - waterLevel) * factorB;
    float deltaC = (VertexC.Coords.y - waterLevel) * factorC;
    float deltaD = (VertexD.Coords.y - waterLevel) * factorD;
    VertexA.Coords = new Vector3(VertexA.Coords.x, VertexA.Coords.y - deltaA, VertexA.Coords.z);
    VertexB.Coords = new Vector3(VertexB.Coords.x, VertexB.Coords.y - deltaB, VertexB.Coords.z);
    VertexC.Coords = new Vector3(VertexC.Coords.x, VertexC.Coords.y - deltaC, VertexC.Coords.z);
    VertexD.Coords = new Vector3(VertexD.Coords.x, VertexD.Coords.y - deltaD, VertexD.Coords.z);

    float factor = factorA * 0.5f + factorD * 0.5f; 
    if (Left != null)
    {
      while (factor < 0.9f)
      {
        factor = Left.DropAreaLeft(mid, waterLevel);
      }
    }
    return factor;
  }

  public float DropAreaRight(Vector3 mid, float waterLevel)
  {
    float factorA = Shape(mid, VertexA.Coords);
    float factorB = Shape(mid, VertexB.Coords);
    float factorC = Shape(mid, VertexC.Coords);
    float factorD = Shape(mid, VertexD.Coords);

    float deltaA = (VertexA.Coords.y - waterLevel) * factorA;
    float deltaB = (VertexB.Coords.y - waterLevel) * factorB;
    float deltaC = (VertexC.Coords.y - waterLevel) * factorC;
    float deltaD = (VertexD.Coords.y - waterLevel) * factorD;
    VertexA.Coords = new Vector3(VertexA.Coords.x, VertexA.Coords.y - deltaA, VertexA.Coords.z);
    VertexB.Coords = new Vector3(VertexB.Coords.x, VertexB.Coords.y - deltaB, VertexB.Coords.z);
    VertexC.Coords = new Vector3(VertexC.Coords.x, VertexC.Coords.y - deltaC, VertexC.Coords.z);
    VertexD.Coords = new Vector3(VertexD.Coords.x, VertexD.Coords.y - deltaD, VertexD.Coords.z);

    float factor = factorB * 0.5f + factorC * 0.5f;
    if (Right != null)
    {
      while (factor < 0.9f)
      {
        factor = Right.DropAreaRight(mid, waterLevel);
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
}
