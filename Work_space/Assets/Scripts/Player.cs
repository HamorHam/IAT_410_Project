using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody playerRig;
    [SerializeField] private GameInput gameInput;

    [Header("Movement")]
    [SerializeField] private float moveSpeed=7f;
    [SerializeField] private float dashForce=5f;
    [SerializeField] private float dashDuration=0.2f;

    private float rotateSpeed=10f;
    private bool isWalking;
    private bool isDashing;

    private void Awake(){
        playerRig = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // Get the movement vector from GameInput script
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        // Move the player with the moveSpeed value
        Vector3 moveDir = new Vector3(inputVector.x, 0f , inputVector.y);
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        transform.forward =Vector3.Slerp( transform.forward,moveDir,Time.deltaTime*rotateSpeed);

        // Dash movement

        if (gameInput.IsJumpButtonPressed()){
            Dash() ;
        }

        isWalking = moveDir != Vector3.zero;
    }

    private void Dash(){
        Vector3 forceToApply = transform.forward* dashForce;

        playerRig.AddForce(forceToApply, ForceMode.Impulse);

        Invoke(nameof(ResetDash), dashDuration);
    }

    private void ResetDash(){}

    public bool IsWalking(){
        return isWalking;
    }
}
