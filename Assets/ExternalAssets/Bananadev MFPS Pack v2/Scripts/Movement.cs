// using UnityEngine;
//
// public class Movement : MonoBehaviour
// {
//     [Header("Movement")]
//     public float walkSpeed = 4f;
//     public float maxVelocityChange = 10f;
//
//     [Header("Jumping")]
//     public float jumpForce = 5f;
//
//     private Vector2 input;
//     private Rigidbody rb;
//     private bool isGrounded;
//
//     void Start()
//     {
//         rb = GetComponent<Rigidbody>();
//     }
//
//     void Update()
//     {
//         input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
//
//         if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
//         {
//             rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
//         }
//     }
//
//     void FixedUpdate()
//     {
//         rb.AddForce(CalculateMovement(), ForceMode.VelocityChange);
//
//         isGrounded = false;
//     }
//
//     Vector3 CalculateMovement()
//     {
//         Vector3 targetVelocity = transform.TransformDirection(new Vector3(input.x, 0f, input.y)) * walkSpeed;
//         Vector3 velocityChange = targetVelocity - rb.linearVelocity;
//
//         velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
//         velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
//         velocityChange.y = 0f;
//
//         return input.magnitude > 0.5f ? velocityChange : Vector3.zero;
//     }
//
//     void OnCollisionStay(Collision collision)
//     {
//         isGrounded = true;
//
//     }
// }
