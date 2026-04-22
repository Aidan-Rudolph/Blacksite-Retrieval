using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    private Camera cam;
    private Rigidbody rb;
    private TerrainTool tt;
    private float camRot = 0;
    public float speed = 5;
    public float sens = 500;

    void Start() {
        cam = transform.GetChild(0).GetComponent<Camera>();
        if (cam == null) {
            Debug.LogError("Camera was not main child of player...");
        }
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        tt = GetComponent<TerrainTool>();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        rb.velocity = new Vector3(0, rb.velocity.y, 0);
        if (Cursor.lockState == CursorLockMode.None) {
            if (Input.GetMouseButtonDown(0)) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else return;
        }
        if (tt) {
            float dir = ((Input.GetKey(KeyCode.Q) ? -1 : 0) + (Input.GetKey(KeyCode.E) ? 1 : 0));
            if (dir != 0)
                tt.Use(new Ray(Camera.main.transform.position, Camera.main.transform.forward), dir*Time.deltaTime, 10);
        }
        rb.velocity += (Input.GetAxis("Horizontal")*transform.right + Input.GetAxis("Vertical")*transform.forward).normalized*speed;
        transform.Rotate(Input.GetAxis("Mouse X")*sens*Time.deltaTime * Vector3.up);
        camRot -= Input.GetAxis("Mouse Y")*sens*Time.deltaTime;
        camRot = Mathf.Clamp(camRot, -90, 90);
        cam.transform.localRotation = Quaternion.Euler(camRot, 0, 0);
    }
}
