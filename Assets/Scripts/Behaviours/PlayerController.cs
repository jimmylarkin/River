using UnityEngine;
using System.Collections;

namespace GrumpyDev.EndlessRiver
{
  public class PlayerController : MonoBehaviour
  {
    Rigidbody rb;

    public float horizontalSpeed = 1000;
    public float verticalSpeed = 250;

    void Start()
    {
      rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
      float moveHorizontal = 0;
      float moveVertical = verticalSpeed * Time.deltaTime;
      Quaternion tiltRotation = Quaternion.Euler(0, 0, 0);
      if (Input.GetKey(KeyCode.RightArrow))
      {
        moveHorizontal = horizontalSpeed * Time.deltaTime;
        tiltRotation = Quaternion.Euler(0, 0, -30);
      }
      if (Input.GetKey(KeyCode.LeftArrow))
      {
        moveHorizontal = horizontalSpeed * Time.deltaTime * -1f;
        tiltRotation = Quaternion.Euler(0, 0, 30);
      }
      if (Input.GetKey(KeyCode.UpArrow))
      {
      }
      if (Input.GetKey(KeyCode.DownArrow))
      {
      }
      Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
      rb.velocity = movement;
      rb.rotation = Quaternion.Slerp(rb.rotation, tiltRotation, 0.1f);
    }
  }
}