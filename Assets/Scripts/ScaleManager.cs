using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.InputSystem;

public class ScaleManager : MonoBehaviour
{
    public Transform rightController;
    public Transform leftController;
    public InputActionReference leftTriggerAction;

    private GameObject scalingObject;
    private XRGrabInteractable grabbedInteractable;
    private float initialHandDistance;
    private Vector3 initialWorldScale;
    private bool isScaling = false;

    void OnEnable() { leftTriggerAction.action.Enable(); }
    void OnDisable() { leftTriggerAction.action.Disable(); }

    void Update()
    {
        bool leftTrigger = leftTriggerAction.action.ReadValue<float>() > 0.5f;

        // Find currently grabbed object by ray interactor
        GameObject currentlyGrabbed = null;
        XRGrabInteractable foundInteractable = null;

        var allInteractables = FindObjectsByType<XRGrabInteractable>(FindObjectsInactive.Exclude);
        foreach (var interactable in allInteractables)
        {
            if (interactable.isSelected)
            {
                currentlyGrabbed = interactable.gameObject;
                foundInteractable = interactable;
                break;
            }
        }

        // Also check proximity selector
        var proximitySelector = FindAnyObjectByType<ProximitySelector>();
        GameObject proximityHeld = proximitySelector != null ?
            proximitySelector.GetHeldObject() : null;

        // Prefer proximity held object
        if (proximityHeld != null)
            currentlyGrabbed = proximityHeld;

        // Clear if nothing grabbed
        if (currentlyGrabbed == null)
        {
            StopScaling();
            scalingObject = null;
            grabbedInteractable = null;
            return;
        }

        scalingObject = currentlyGrabbed;
        grabbedInteractable = foundInteractable;

        // SCALE when left trigger held while anything is grabbed
        if (leftTrigger && !isScaling)
        {
            isScaling = true;

            if (grabbedInteractable != null)
                grabbedInteractable.trackScale = false;

            initialHandDistance = Vector3.Distance(
                rightController.position, leftController.position);
            if (initialHandDistance < 0.05f) initialHandDistance = 0.05f;

            initialWorldScale = scalingObject.transform.lossyScale;

            var hl = scalingObject.GetComponent<HighlightOnHover>();
            if (hl != null) hl.SetScaling(true);

            Debug.Log("Scaling started! dist=" + initialHandDistance);
        }

        // Stop scaling when left trigger released
        if (!leftTrigger && isScaling)
        {
            StopScaling();
        }

        // Apply scale every frame while scaling
        if (isScaling && scalingObject != null)
        {
            float currentDistance = Vector3.Distance(
                rightController.position, leftController.position);

            float rawFactor = currentDistance / initialHandDistance;
            float scaleFactor = Mathf.Clamp(1 + (rawFactor - 1) * 5f, 0.1f, 10f);

            Vector3 targetWorldScale = initialWorldScale * scaleFactor;
            Transform t = scalingObject.transform;

            if (t.parent != null)
            {
                Vector3 parentScale = t.parent.lossyScale;
                t.localScale = new Vector3(
                    parentScale.x != 0 ? targetWorldScale.x / parentScale.x : targetWorldScale.x,
                    parentScale.y != 0 ? targetWorldScale.y / parentScale.y : targetWorldScale.y,
                    parentScale.z != 0 ? targetWorldScale.z / parentScale.z : targetWorldScale.z
                );
            }
            else
            {
                t.localScale = targetWorldScale;
            }

            Debug.Log("Scaling: factor=" + scaleFactor + " dist=" + currentDistance);
        }
    }

    void StopScaling()
    {
        if (isScaling)
        {
            if (scalingObject != null)
            {
                var hl = scalingObject.GetComponent<HighlightOnHover>();
                if (hl != null) hl.SetScaling(false);
            }
            if (grabbedInteractable != null)
                grabbedInteractable.trackScale = true;
        }
        isScaling = false;
    }
}