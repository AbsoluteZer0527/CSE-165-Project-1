using UnityEngine;
using TMPro;

public class ControllerIndicator : MonoBehaviour
{
    [Header("Controllers")]
    public Transform leftController;
    public Transform rightController;

    [Header("Offset above controller")]
    public Vector3 travelIndicatorOffset = new Vector3(0, 0.12f, 0);
    public Vector3 selectionIndicatorOffset = new Vector3(0, 0.08f, 0.05f);

    private GameObject leftTravelIndicator;
    private GameObject rightTravelIndicator;
    private GameObject leftSelectionIndicator;
    private GameObject rightSelectionIndicator;

    void Start()
    {
        // Travel indicators
        leftTravelIndicator = CreateIndicator(
            leftController,
            "⊕ Move",
            Color.cyan,
            travelIndicatorOffset);

        rightTravelIndicator = CreateIndicator(
            rightController,
            "↻ Turn",
            Color.yellow,
            travelIndicatorOffset);

        // Selection indicators
        leftSelectionIndicator = CreateIndicator(
            leftController,
            "Grip = Grab\nTrigger = Rotate",
            Color.green,
            selectionIndicatorOffset);

        rightSelectionIndicator = CreateIndicator(
            rightController,
            "Grip = Ray Grab\nTrigger = Scale",
            Color.magenta,
            selectionIndicatorOffset);
    }

    GameObject CreateIndicator(Transform controller, string label,
        Color color, Vector3 offset)
    {
        // Root object parented to controller
        GameObject root = new GameObject("Indicator_" + label);
        root.transform.SetParent(controller);
        root.transform.localPosition = offset;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        // Canvas
        GameObject canvasObj = new GameObject("Canvas");
        canvasObj.transform.SetParent(root.transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasObj.transform.localPosition = Vector3.zero;
        canvasObj.transform.localRotation = Quaternion.identity;
        canvasObj.transform.localScale = Vector3.one * 0.001f;

        // Add canvas scaler
        UnityEngine.UI.CanvasScaler scaler =
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;

        // Background panel
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvasObj.transform);
        UnityEngine.UI.Image bg = panel.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(color.r, color.g, color.b, 0.75f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(220, 80);
        panelRect.localPosition = Vector3.zero;
        panelRect.localRotation = Quaternion.identity;
        panelRect.localScale = Vector3.one;
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);

        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(panel.transform);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.color = Color.black;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(210, 75);
        textRect.localPosition = Vector3.zero;
        textRect.localRotation = Quaternion.identity;
        textRect.localScale = Vector3.one;
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);

        return root;
    }

    void Update()
    {
        // Always face the main camera
        Camera cam = Camera.main;
        if (cam == null) return;

        FaceCamera(leftTravelIndicator, cam);
        FaceCamera(rightTravelIndicator, cam);
        FaceCamera(leftSelectionIndicator, cam);
        FaceCamera(rightSelectionIndicator, cam);
    }

    void FaceCamera(GameObject indicator, Camera cam)
    {
        if (indicator == null) return;

        indicator.transform.LookAt(
            indicator.transform.position + cam.transform.rotation * Vector3.forward,
            cam.transform.rotation * Vector3.up);
    }
}