using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;

namespace GrumpyDev.EndlessRiver
{
  public class MapController : MonoBehaviour
  {
    public Map map;
    private float secondsSinceLastMapupdate;

    void Start() {
      secondsSinceLastMapupdate = 0f;
    }

    void Update() {
    }

    void FixedUpdate()
    {
      secondsSinceLastMapupdate += Time.deltaTime;
    }
  }
}