using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag the Player (parent) GameObject here")]
    [SerializeField] Transform playerBody;

    [Header("Tuning (start here)")]
    [Tooltip("Global sensitivity multiplier")]
    [SerializeField] float sensitivity = 1.0f;

    [Tooltip("How fast input smooths (higher = snappier, lower = smoother)")]
    [SerializeField] float smoothing = 8f;

    [Tooltip("Multiplier for raw mouse delta (editor/PC)")]
    [SerializeField] float mouseMultiplier = 0.02f;

    [Tooltip("Multiplier applied to normalized touch delta (mobile)")]
    [SerializeField] float touchMultiplier = 1.0f;

    [Tooltip("Clamp vertical look (degrees)")]
    [SerializeField] float maxVerticalAngle = 80f;

    // internal state
    float rotationX = 0f;                    // pitch
    Vector2 smoothDelta = Vector2.zero;     // smoothed delta used each frame

    void Start()
    {
        if (playerBody == null)
            Debug.LogWarning("PlayerLook: playerBody not assigned.");
    }

    void Update()
    {
        Vector2 rawDelta = Vector2.zero;

        // ----- Mouse (Editor / PC) -----
        if (Mouse.current != null)
        {
            // Use delta only while a button is pressed OR always accept mouse movement in editor
            // (keeps editor testing easy)
            if (Application.isEditor || Mouse.current.leftButton.isPressed)
            {
                Vector2 m = Mouse.current.delta.ReadValue();
                rawDelta += m * mouseMultiplier; // convert pixels to a smaller scale
            }
        }

        // ----- Touch (Mobile) -----
        if (Touchscreen.current != null)
        {
            // Primary touch is more reliable for single-finger look/drag
            var touch = Touchscreen.current.primaryTouch;

            // If a touch is ongoing, read its delta (in pixels)
            if (touch.press.isPressed)
            {
                Vector2 touchDelta = touch.delta.ReadValue(); // raw pixels since last frame

                // Normalize by screen size so behavior scales across devices / resolutions
                float denom = Mathf.Max(Screen.width, Screen.height);
                if (denom <= 0) denom = 1;

                Vector2 normalized = touchDelta / denom; // now roughly -1..1 scale per full-screen swipe
                rawDelta += normalized * touchMultiplier * 100f; // amplify normalized value to useful range
            }
        }

        // ----- Smooth the input -----
        // Lerp smoothDelta toward rawDelta using smoothing factor (frame-rate independent)
        float t = 1f - Mathf.Exp(-smoothing * Time.deltaTime); // critically damped lerp factor
        smoothDelta = Vector2.Lerp(smoothDelta, rawDelta, t);

        // ----- Apply rotations -----
        // Horizontal (yaw) applied to player body
        float yaw = smoothDelta.x * sensitivity;
        playerBody.Rotate(Vector3.up * yaw);

        // Vertical (pitch) applied to camera (this)
        rotationX -= smoothDelta.y * sensitivity;
        rotationX = Mathf.Clamp(rotationX, -maxVerticalAngle, maxVerticalAngle);
        transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }
}
