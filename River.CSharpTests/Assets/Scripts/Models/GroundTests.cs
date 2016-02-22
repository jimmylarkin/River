using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using River;

namespace Tests
{
  [TestClass()]
  public class GroundTests
  {
    [TestMethod()]
    public void InitTest()
    {
      Ground ground = new Ground() { ScaleHorizontal = 3000f, ScaleVertical = 500f, Octaves = 8, StartAmplitude = 128, StartFrequency = 4 };
      ground.Init();
    }
  }
}