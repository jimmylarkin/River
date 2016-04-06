using UnityEngine;
using System.Collections;
using UnityEditor;
using GrumpyDev.EndlessRiver;


public class MapEditor : Editor
{
  [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
  static void DrawRiverLines(MapController mapController, GizmoType gizmoType)
  {
    if (mapController == null)
    {
      return;
    }
    Map map = mapController.map;
    if (map == null)
    {
      return;
    }
    Gizmos.color = Color.red;
    for (int i = 1; i < map.RiverData.Count; i++)
    {
      Gizmos.DrawLine(new Vector3(map.RiverData[i - 1].JointChannelMiddleLine, map.WaterLevel, map.RiverData[i - 1].Z), new Vector3(map.RiverData[i].JointChannelMiddleLine, map.WaterLevel, map.RiverData[i].Z));
    }
    Gizmos.color = Color.yellow;
    for (int i = 1; i < map.RiverData.Count; i++)
    {
      Gizmos.DrawLine(new Vector3(map.RiverData[i - 1].LeftChannel.LeftEdge, map.WaterLevel, map.RiverData[i - 1].Z), new Vector3(map.RiverData[i].LeftChannel.LeftEdge, map.WaterLevel, map.RiverData[i].Z));
      Gizmos.DrawLine(new Vector3(map.RiverData[i - 1].LeftChannel.MiddleLine, map.WaterLevel, map.RiverData[i - 1].Z), new Vector3(map.RiverData[i].LeftChannel.MiddleLine, map.WaterLevel, map.RiverData[i].Z));
      Gizmos.DrawLine(new Vector3(map.RiverData[i - 1].LeftChannel.RightEdge, map.WaterLevel, map.RiverData[i - 1].Z), new Vector3(map.RiverData[i].LeftChannel.RightEdge, map.WaterLevel, map.RiverData[i].Z));
    }
    Gizmos.color = Color.cyan;
    for (int i = 1; i < map.RiverData.Count; i++)
    {
      Gizmos.DrawLine(new Vector3(map.RiverData[i - 1].RightChannel.LeftEdge, map.WaterLevel, map.RiverData[i - 1].Z), new Vector3(map.RiverData[i].RightChannel.LeftEdge, map.WaterLevel, map.RiverData[i].Z));
      Gizmos.DrawLine(new Vector3(map.RiverData[i - 1].RightChannel.MiddleLine, map.WaterLevel, map.RiverData[i - 1].Z), new Vector3(map.RiverData[i].RightChannel.MiddleLine, map.WaterLevel, map.RiverData[i].Z));
      Gizmos.DrawLine(new Vector3(map.RiverData[i - 1].RightChannel.RightEdge, map.WaterLevel, map.RiverData[i - 1].Z), new Vector3(map.RiverData[i].RightChannel.RightEdge, map.WaterLevel, map.RiverData[i].Z));
    }
  }
}