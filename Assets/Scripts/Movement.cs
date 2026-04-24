using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class Movement : MonoBehaviour
{
    public Transform mainCamera;
    public float moveSpeed = 3f;
    public float snapAngle = 45f;

    private InputDevice leftDevice;
    private InputDevice rightDevice;
    private bool snapWasPressed = false;

    void Start()
    {
        TryGetDevices();
    }

    void TryGetDevices()
    {
        if (!leftDevice.isValid)
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, devices);
            if (devices.Count > 0) leftDevice = devices[0];
        }
        if (!rightDevice.isValid)
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
            if (devices.Count > 0) rightDevice = devices[0];
        }
    }

    void Update()
    {
        TryGetDevices();

        // MOVEMENT — left thumbstick
        leftDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 leftStick);

        if (leftStick.magnitude > 0.1f)
        {
            // Move relative to where camera is looking
            Vector3 forward = mainCamera.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = mainCamera.right;
            right.y = 0;
            right.Normalize();

            Vector3 move = (forward * leftStick.y + right * leftStick.x)
                           * moveSpeed * Time.deltaTime;

            transform.position += move;
        }

        // SNAP TURN — right thumbstick
        rightDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 rightStick);

        bool snapPressed = Mathf.Abs(rightStick.x) > 0.7f;

        if (snapPressed && !snapWasPressed)
        {
            float direction = Mathf.Sign(rightStick.x);
            // Rotate around camera position so view doesnt shift
            transform.RotateAround(
                mainCamera.position,
                Vector3.up,
                snapAngle * direction);
        }

        snapWasPressed = snapPressed;
    }
}