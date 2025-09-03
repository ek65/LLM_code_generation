using UnityEngine;

public class CameraMovementController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;

    private bool isMouseLocked = false;

    void Start()
    {
#if UNITY_STANDALONE_WIN || UNITY_WEBGL || UNITY_STANDALONE_OSX
        LockMouse(false);
#endif
    }

    void Update()
    {
#if UNITY_STANDALONE_WIN || UNITY_WEBGL || UNITY_STANDALONE_OSX
        HandleMovement();
        HandleMouseRotation();
        HandleMouseLock();
#endif
    }

    void HandleMovement()
    {
        float moveHorizontal = 0f;
        float moveVertical = 0f;
        float moveUpDown = 0f;

        if (Input.GetKey(KeyCode.UpArrow)) moveVertical += 1f;
        if (Input.GetKey(KeyCode.DownArrow)) moveVertical -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) moveHorizontal += 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) moveHorizontal -= 1f;
        if (Input.GetKey(KeyCode.LeftBracket)) moveUpDown -= 1f;
        if (Input.GetKey(KeyCode.RightBracket)) moveUpDown += 1f;

        Vector3 movement = new Vector3(moveHorizontal, moveUpDown, moveVertical);
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.Self);
    }

    void HandleMouseRotation()
    {
        if (isMouseLocked)
        {
            // Get mouse movement input
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Yaw (rotate around Y-axis) and pitch (rotate around X-axis)
            transform.Rotate(Vector3.up, mouseX, Space.World);
            transform.Rotate(Vector3.right, -mouseY, Space.Self);
        }
    }

    void HandleMouseLock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LockMouse(false);
        }

        if (Input.GetMouseButtonDown(0))
        {
            LockMouse(true);
        }
    }

    void LockMouse(bool lockState)
    {
        isMouseLocked = lockState;
        Cursor.lockState = lockState ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockState;
    }
}
