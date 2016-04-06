using System;
using System.Linq;
using System.Collections.Generic;

namespace GrumpyDev.EndlessRiver
{
  public class RiverData
  {
    public float Z { get; set; }
    public float JointChannelMiddleLine { get; set; }
    public RiverChannelData LeftChannel { get; set; }
    public RiverChannelData RightChannel { get; set; }
  }
}
