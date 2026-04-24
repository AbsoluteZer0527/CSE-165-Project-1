using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HighlightOnHover : MonoBehaviour
{
    public Material highlightHoverMaterial;
    public Material highlightGrabMaterial;

    private bool isProximityHovered = false;
    private bool isProximitySelected = false;

    private MeshRenderer[] allRenderers;
    private Material[][] originalMaterials; // Store ALL material slots
    private XRGrabInteractable interactable;

    private bool isHovered = false;
    private bool isGrabbed = false;

    void Start()
    {
        allRenderers = GetComponentsInChildren<MeshRenderer>();

        // Store ALL material slots from ALL renderers
        originalMaterials = new Material[allRenderers.Length][];
        for (int i = 0; i < allRenderers.Length; i++)
        {
            // Use sharedMaterials to get all slots
            originalMaterials[i] = allRenderers[i].materials;
        }

        interactable = GetComponent<XRGrabInteractable>();
        interactable.hoverEntered.AddListener(_ => { isHovered = true; UpdateMaterial(); });
        interactable.hoverExited.AddListener(_ => { isHovered = false; UpdateMaterial(); });
        interactable.selectEntered.AddListener(_ => { isGrabbed = true; UpdateMaterial(); });
        interactable.selectExited.AddListener(_ => { isGrabbed = false; UpdateMaterial(); });
    }

    void UpdateMaterial()
    {
        if (isGrabbed || isProximitySelected)
            SetAllSlots(highlightGrabMaterial);      // Green = grabbed or proximity selected
        else if (isHovered || isProximityHovered)
            SetAllSlots(highlightHoverMaterial);      // Yellow = hovered
        else
            RestoreOriginal();
    }

    void SetAllSlots(Material mat)
    {
        foreach (var renderer in allRenderers)
        {
            if (renderer == null) continue;

            // Create array filled with highlight material
            // matching the number of material slots
            Material[] newMats = new Material[renderer.materials.Length];
            for (int i = 0; i < newMats.Length; i++)
                newMats[i] = mat;

            renderer.materials = newMats;
        }
    }

    void RestoreOriginal()
    {
        for (int i = 0; i < allRenderers.Length; i++)
        {
            if (allRenderers[i] != null)
                allRenderers[i].materials = originalMaterials[i];
        }
    }

    public void SetScaling(bool scaling)
    {
        isGrabbed = scaling;
        UpdateMaterial();
    }

    // Add these two new public methods:
    public void SetProximityHover(bool hover)
    {
        isProximityHovered = hover;
        UpdateMaterial();
    }

    public void SetProximitySelected(bool selected)
    {
        isProximitySelected = selected;
        UpdateMaterial();
    }

}
