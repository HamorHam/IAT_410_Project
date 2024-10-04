using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    private Rigidbody playerRig;
    [SerializeField] private GameInput gameInput;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7.0f;
    [SerializeField] private float distance = 5.0f;
    [SerializeField] private float dashDuration = 0.2f; // Added dash duration

    private float rotateSpeed = 10f;
    private bool isWalking;
    private bool isDashing; // To ensure dash is executed only once

    private void Awake()
    {
        playerRig = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Check for dash input, only dash if not already dashing
        if (gameInput.IsJumpButtonPressed() && !isDashing)
        {
            Debug.Log("dash");
            StartCoroutine(DashCoroutine()); // Use coroutine for smooth dash
        }

        // Get the movement vector from GameInput script
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        // Move the player with the moveSpeed value
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        // Smooth rotation towards movement direction
        if (moveDir != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
        }

        isWalking = moveDir != Vector3.zero;
    }

    // Coroutine for smooth dash
    private IEnumerator DashCoroutine()
    {
        isDashing = true;

        RaycastHit hit;
        Vector3 startPosition = transform.position;
        Vector3 destination = transform.position + transform.forward * distance;

        // Check if there's an obstacle in the path
        if (Physics.Linecast(transform.position, destination, out hit))
        {
            destination = transform.position + transform.forward * (hit.distance - 1f);
        }

        // Ensure the destination is grounded
        if (Physics.Raycast(destination, -Vector3.up, out hit))
        {
            destination = hit.point;
        }

        // Smoothly move the player over the dash duration
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            transform.position = Vector3.Lerp(startPosition, destination, elapsed / dashDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the player is at the final destination
        transform.position = destination;

        isDashing = false;
    }

    public bool IsWalking()
    {
        return isWalking;
    }
}


// using UnityEditor.Experimental.GraphView;
// using UnityEngine;
// using System.Collections; // Required for IEnumerator if coroutines are used

// public class Player : MonoBehaviour
// {
//     private Rigidbody playerRig;
//     [SerializeField] private GameInput gameInput;

//     [Header("Movement")]
//     [SerializeField] private float moveSpeed = 7.0f;
//     [SerializeField] private float distance = 2.0f;
//     [SerializeField] private float dashDuration = 0.2f; // Keeping dash duration for timing purposes

//     private float rotateSpeed = 10f;
//     private bool isWalking;
//     private bool isDashing; // To ensure dash is executed only once

//     private void Awake()
//     {
//         playerRig = GetComponent<Rigidbody>();
//     }

//     private void Update()
//     {
//         // Check for dash input, only dash if not already dashing
//         if (gameInput.IsJumpButtonPressed() && !isDashing)
//         {
//             Debug.Log("dash");
//             BlinkDash(); // Perform instant blink (teleport)
//         }

//         // Get the movement vector from GameInput script
//         Vector2 inputVector = gameInput.GetMovementVectorNormalized();

//         // Move the player with the moveSpeed value
//         Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
//         transform.position += moveDir * moveSpeed * Time.deltaTime;

//         // Smooth rotation towards movement direction
//         if (moveDir != Vector3.zero)
//         {
//             transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
//         }

//         isWalking = moveDir != Vector3.zero;
//     }

//     // Instant Blink Dash (Teleport)
//     private void BlinkDash()
//     {
//         isDashing = true;

//         RaycastHit hit;
//         Vector3 destination = transform.position + transform.forward * distance;

//         // Check if there's an obstacle in the path
//         if (Physics.Linecast(transform.position, destination, out hit))
//         {
//             // Adjust destination to stop just before the obstacle
//             destination = transform.position + transform.forward * (hit.distance - 1f);
//         }

//         // Ensure the destination is grounded
//         if (Physics.Raycast(destination, -Vector3.up, out hit))
//         {
//             destination = hit.point;
//         }

//         // Instantly move the player to the destination (blink/teleport)
//         transform.position = destination;

//         // Invoke reset function after a short delay to allow for cooldown
//         Invoke(nameof(ResetDash), dashDuration);
//     }

//     private void ResetDash()
//     {
//         isDashing = false;
//     }

//     public bool IsWalking()
//     {
//         return isWalking;
//     }
// }
