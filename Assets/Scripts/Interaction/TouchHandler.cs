using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class TouchHandler : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 0.01f;
    [SerializeField] private float rotateSpeed = 0.3f;
    [SerializeField] private float scaleSpeed = 0.01f;
    [SerializeField] private float minScale = 0.1f;
    [SerializeField] private float maxScale = 5f;

    private float _lastPinchDistance;
    private float _lastTwoFingerAngle;

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
#if UNITY_EDITOR
        HandleMouseInput();
#else
        var touches = Touch.activeTouches;
        if (touches.Count == 1)
            HandleSingleTouch(touches[0]);
        else if (touches.Count == 2)
            HandleTwoFingerGesture(touches[0], touches[1]);
#endif
    }

    private void HandleMouseInput()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        // Click sinistro = muovi
        if (mouse.leftButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue();
            transform.position += new Vector3(delta.x, 0, delta.y) * moveSpeed;
        }

        // Click destro = ruota
        if (mouse.rightButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue();
            transform.Rotate(Vector3.up, -delta.x * rotateSpeed, Space.World);
        }

        // Scroll = scala
        float scroll = mouse.scroll.ReadValue().y;
        if (scroll != 0)
        {
            float newScale = Mathf.Clamp(
                transform.localScale.x + scroll * scaleSpeed,
                minScale,
                maxScale
            );
            transform.localScale = Vector3.one * newScale;
        }
    }

    private void HandleSingleTouch(Touch touch)
    {
        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began) return;

        Vector2 delta = touch.delta;
        transform.position += new Vector3(delta.x, 0, delta.y) * moveSpeed;
    }

    private void HandleTwoFingerGesture(Touch t0, Touch t1)
    {
        if (t1.phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            _lastPinchDistance = Vector2.Distance(t0.screenPosition, t1.screenPosition);
            _lastTwoFingerAngle = GetAngle(t0.screenPosition, t1.screenPosition);
            return;
        }

        // Pinch → scala
        float currentDistance = Vector2.Distance(t0.screenPosition, t1.screenPosition);
        float pinchDelta = currentDistance - _lastPinchDistance;
        _lastPinchDistance = currentDistance;

        float newScale = Mathf.Clamp(
            transform.localScale.x + pinchDelta * scaleSpeed,
            minScale,
            maxScale
        );
        transform.localScale = Vector3.one * newScale;

        // Twist → ruota
        float currentAngle = GetAngle(t0.screenPosition, t1.screenPosition);
        float angleDelta = currentAngle - _lastTwoFingerAngle;
        _lastTwoFingerAngle = currentAngle;

        transform.Rotate(Vector3.up, -angleDelta * rotateSpeed, Space.World);
    }

    private float GetAngle(Vector2 a, Vector2 b)
    {
        Vector2 dir = b - a;
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }
}