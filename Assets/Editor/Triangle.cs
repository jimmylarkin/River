using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using MIConvexHull;

[DebuggerDisplay("{Vertex1} | {Vertex2} | {Vertex3}")]
public class Triangle<TVertex> : TriangulationCell<TVertex, Triangle<TVertex>>
      where TVertex : Vertex
{
}
