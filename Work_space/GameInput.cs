using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private PlayerInputActions inputActions; // Reference to the generated input actions

    private void Awake()
    {
        // Initialize the input actions
        inputActions = new PlayerInputActions();

        // Enable the input action map for the Player 
        inputActions.Player.Enable();
    }

    public Vector2 GetMovementVectorNormalized()
    {
        // Get the movement vector from the input actions
        Vector2 inputVector = inputActions.Player.Move.ReadValue<Vector2>();
        // Normalize the input vector for smooth movement
        return inputVector.normalized;
    }

     public bool IsJumpButtonPressed()
    {
        // Check if the Jump button is currently being pressed
        return inputActions.Player.Dash.WasPressedThisFrame();
    }

    private void OnEnable()
    {
        // Enable the input action map when the object is enabled
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        // Disable the input action map when the object is disabled
        inputActions.Player.Disable();
    }
}