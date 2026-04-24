using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.InputSystem;

public class ProximitySelector : MonoBehaviour
{
    public Transform leftController;

    // Grip = grab, Trigger = scale (handled by ScaleManager)
    public InputActionReference leftGripAction;
    public InputActionReference leftTriggerAction; // for rotate
    public float grabRadius = 0.2f;

    [Header("Sphere Visual")]
    public Color sphereIdleColor = new Color(0, 1, 1, 0.15f);
    public Color sphereHoverColor = new Color(1, 1, 0, 0.3f);
    public Color sphereGrabColor = new Color(0, 1, 0, 0.3f);

    private GameObject visualSphere;
    private MeshRenderer sphereRenderer;

    private GameObject hoveredObject;
    private GameObject selectedObject;
    private HighlightOnHover hoveredHighlight;

    private Vector3 grabOffset;
    private Quaternion grabRotOffset;
    private Rigidbody selectedRigidbody;
    private bool isHolding = false;
    private bool gripWasPressed = false;

    void OnEnable()
    {
        leftGripAction.action.Enable();
        leftTriggerAction.action.Enable();
    }

    void OnDisable()
    {
        leftGripAction.action.Disable();
        leftTriggerAction.action.Disable();
    }

    // Called by ScaleManager to know what left hand is holding
    public GameObject GetHeldObject() => selectedObject;

    void Start()
    {
        CreateVisualSphere();
    }

    void CreateVisualSphere()
    {
        visualSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visualSphere.name = "ProximityVisual";
        Destroy(visualSphere.GetComponent<Collider>());
        visualSphere.transform.SetParent(leftController);
        visualSphere.transform.localPosition = Vector3.zero;
        visualSphere.transform.localScale = Vector3.one * grabRadius * 2f;

        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetFloat("_Surface", 1);
        mat.renderQueue = 3000;
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.color = sphereIdleColor;

        sphereRenderer = visualSphere.GetComponent<MeshRenderer>();
        sphereRenderer.material = mat;
        sphereRenderer.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    void UpdateSphereColor(Color color)
    {
        if (sphereRenderer != null)
            sphereRenderer.material.color = color;
    }

    void Update()
    {
        bool leftGrip = leftGripAction.action.ReadValue<float>() > 0.5f;
        bool leftTrigger = leftTriggerAction.action.ReadValue<float>() > 0.5f;

        // Find nearest object
        Collider[] hits = Physics.OverlapSphere(leftController.position, grabRadius);
        GameObject nearest = null;
        float minDist = float.MaxValue;

        foreach (var col in hits)
        {
            var xrGrab = col.GetComponent<XRGrabInteractable>();
            if (xrGrab != null)
            {
                float d = Vector3.Distance(leftController.position, col.transform.position);
                if (d < minDist) { minDist = d; nearest = col.gameObject; }
            }
        }

        // Hover
        if (nearest != hoveredObject && !isHolding)
        {
            if (hoveredHighlight != null)
                hoveredHighlight.SetProximityHover(false);

            hoveredObject = nearest;

            if (hoveredObject != null)
            {
                hoveredHighlight = hoveredObject.GetComponent<HighlightOnHover>();
                if (hoveredHighlight != null)
                    hoveredHighlight.SetProximityHover(true);
                UpdateSphereColor(sphereHoverColor);
            }
            else
            {
                hoveredHighlight = null;
                UpdateSphereColor(sphereIdleColor);
            }
        }

        // GRAB on grip press
        if (leftGrip && !gripWasPressed && hoveredObject != null && !isHolding)
        {
            var xrGrab = hoveredObject.GetComponent<XRGrabInteractable>();

            // Hand switch if right hand holds it
            if (xrGrab != null && xrGrab.isSelected)
            {
                xrGrab.interactionManager.SelectExit(
                    xrGrab.interactorsSelecting[0], xrGrab);
                StartCoroutine(GrabNextFrame(hoveredObject));
            }
            else
            {
                GrabObject(hoveredObject);
            }
        }

        // RELEASE on grip release
        if (!leftGrip && gripWasPressed && isHolding)
        {
            ReleaseObject();
        }

        // MOVE — grip only, no trigger
        if (isHolding && selectedObject != null && leftGrip && !leftTrigger)
        {
            selectedObject.transform.position =
                leftController.position + grabOffset;
        }

        // ROTATE — grip + trigger
        if (isHolding && selectedObject != null && leftGrip && leftTrigger)
        {
            selectedObject.transform.rotation =
                leftController.rotation * grabRotOffset;
        }

        if (isHolding) UpdateSphereColor(sphereGrabColor);

        gripWasPressed = leftGrip;
    }

    System.Collections.IEnumerator GrabNextFrame(GameObject obj)
    {
        yield return null;
        GrabObject(obj);
    }

    void GrabObject(GameObject obj)
    {
        selectedObject = obj;
        selectedRigidbody = selectedObject.GetComponent<Rigidbody>();
        if (selectedRigidbody != null) selectedRigidbody.isKinematic = true;

        grabOffset = selectedObject.transform.position - leftController.position;
        grabRotOffset = Quaternion.Inverse(leftController.rotation)
                       * selectedObject.transform.rotation;
        isHolding = true;

        var hl = selectedObject.GetComponent<HighlightOnHover>();
        if (hl != null) hl.SetProximitySelected(true);

        UpdateSphereColor(sphereGrabColor);
    }

    void ReleaseObject()
    {
        if (selectedObject != null)
        {
            if (selectedRigidbody != null)
                selectedRigidbody.isKinematic = false;
            var hl = selectedObject.GetComponent<HighlightOnHover>();
            if (hl != null) hl.SetProximitySelected(false);
        }
        selectedObject = null;
        selectedRigidbody = null;
        isHolding = false;
        UpdateSphereColor(hoveredObject != null ? sphereHoverColor : sphereIdleColor);
    }
}