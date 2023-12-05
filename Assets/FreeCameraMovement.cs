using UnityEngine;

public class FreeCameraMovement : MonoBehaviour
{
    public float movementSpeed = 5f;
    public float rotationSpeed = 2f;
    public float verticalMovementSpeed = 3f;

    private bool isRotating = false;
    private Vector3 rotationAmount = Vector3.zero;

    void Update()
    {
        // Check for movement input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float upDown = Input.GetKey(KeyCode.E) ? 1f : (Input.GetKey(KeyCode.Q) ? -1f : 0f);

        // Calculate movement direction
        Vector3 movement = new Vector3(horizontal, upDown, vertical).normalized;

        // Check if right mouse button is pressed
        if (Input.GetMouseButtonDown(1)) // 1 corresponds to the right mouse button
        {
            isRotating = true;
        }

        // Check if right mouse button is released
        if (Input.GetMouseButtonUp(1))
        {
            isRotating = false;
        }

        // Move the camera based on input
        MoveCamera(movement);

        // Rotate the camera based on mouse input
        if (isRotating)
        {
            RotateCamera();
        }
    }

    void MoveCamera(Vector3 movement)
    {
        // Translate the camera based on input
        transform.Translate(movement * movementSpeed * Time.deltaTime);
    }

    void RotateCamera()
    {
        // Get mouse input for rotation
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Accumulate mouse movement
        rotationAmount += new Vector3(-mouseY, mouseX, 0f) * rotationSpeed;

        // Clamp vertical rotation to avoid flipping
        rotationAmount.x = Mathf.Clamp(rotationAmount.x, -90f, 90f);

        // Apply rotation to the camera
        transform.rotation = Quaternion.Euler(rotationAmount);
    }
}
