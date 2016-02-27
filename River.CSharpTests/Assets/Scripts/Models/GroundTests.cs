using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using River;
using UnityEngine;

namespace River.Tests
{
  [TestClass()]
  public class GroundTests
  {
    //  [TestMethod()]
    //  public void GenerateMeshDataTest()
    //  {
    //    int width = 4;
    //    int height = 3;
    //    List<Vector3> vertices = new List<Vector3>(width * height);
    //    List<Vector3> normals = new List<Vector3>(width * height);
    //    List<int> triangles = new List<int>();
    //    List<Vector4> tangents = new List<Vector4>(width * height);
    //    Ground ground = new Ground();
    //    ground.GenerateMeshData(width, height, vertices, normals, triangles, tangents);
    //    Assert.AreEqual(12, vertices.Count());
    //    Assert.AreEqual(12 * 3, triangles.Count());
    //    //1 bottom right
    //    Assert.AreEqual(1, triangles[0]);
    //    Assert.AreEqual(2, triangles[1]);
    //    Assert.AreEqual(5, triangles[2]);
    //    //1 bottom left
    //    Assert.AreEqual(1, triangles[3]);
    //    Assert.AreEqual(5, triangles[4]);
    //    Assert.AreEqual(0, triangles[5]);
    //    //3 bottom left
    //    Assert.AreEqual(3, triangles[6]);
    //    Assert.AreEqual(7, triangles[7]);
    //    Assert.AreEqual(2, triangles[8]);
    //    //4 top right
    //    Assert.AreEqual(4, triangles[9]);
    //    Assert.AreEqual(0, triangles[10]);
    //    Assert.AreEqual(5, triangles[11]);
    //    //4 bottom right
    //    Assert.AreEqual(4, triangles[12]);
    //    Assert.AreEqual(5, triangles[13]);
    //    Assert.AreEqual(8, triangles[14]);
    //    //6 top left
    //    Assert.AreEqual(6, triangles[15]);
    //    Assert.AreEqual(5, triangles[16]);
    //    Assert.AreEqual(2, triangles[17]);
    //    //6 top right
    //    Assert.AreEqual(6, triangles[18]);
    //    Assert.AreEqual(2, triangles[19]);
    //    Assert.AreEqual(7, triangles[20]);
    //    //6 bottom right
    //    Assert.AreEqual(6, triangles[21]);
    //    Assert.AreEqual(7, triangles[22]);
    //    Assert.AreEqual(10, triangles[23]);
    //    //6 bottom left
    //    Assert.AreEqual(6, triangles[24]);
    //    Assert.AreEqual(10, triangles[25]);
    //    Assert.AreEqual(5, triangles[26]);
    //    //9 top left
    //    Assert.AreEqual(9, triangles[27]);
    //    Assert.AreEqual(8, triangles[28]);
    //    Assert.AreEqual(5, triangles[29]);
    //    //9 top right
    //    Assert.AreEqual(9, triangles[30]);
    //    Assert.AreEqual(5, triangles[31]);
    //    Assert.AreEqual(10, triangles[32]);
    //    //11 top left
    //    Assert.AreEqual(11, triangles[33]);
    //    Assert.AreEqual(10, triangles[34]);
    //    Assert.AreEqual(7, triangles[35]);
    //  }

    [TestMethod()]
    public void InitTest()
    {
      Ground ground = new Ground() { ScaleVertical = 500f, Octaves = 8, Persistence = 128, Frequency = 4 };
      ground.Init();
    }
  }
}