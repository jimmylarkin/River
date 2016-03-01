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
    [TestMethod()]
    public void InitTest()
    {
      Ground ground = new Ground() { ScaleVertical = 500f, Octaves = 8, Persistence = 128, Frequency = 4 };
      ground.Init();
    }
  }
}