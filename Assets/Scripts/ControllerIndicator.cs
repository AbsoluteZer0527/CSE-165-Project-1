using UnityEngine;
using TMPro;

public class ControllerIndicator : MonoBehaviour
{
    [Header("Controllers")]
    public Transform leftController;
    public Transform rightController;

    [Header("Offset above controller")]
    public Vector3 indicatorOffset = new Vector3(0, 0.1f, 0);

    private GameObject leftCanvas;
    private GameObject rightCanvas;

    void Start()
    {
        leftCanvas = CreateIndicator(leftController, "← Move", Color.cyan);
        rightCanvas = CreateIndicator(rightController, "↻ Turn", Color.yellow);
    }

    GameObject CreateIndicator(Transform controller, string label, Color color)
    {
        // Root object follows controller
        GameObject root = new GameObject("Indicator_" + label);
        root.transform.SetParent(controller);
        root.transform.localPosition = indicatorOffset;
        root.transform.localRotation = Quaternion.identity;

        // Canvas
        Canvas canvas = new GameObject("Canvas").AddComponent<Canvas>();
        canvas.transform.SetParent(root.transform);
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.transform.localPosition = Vector3.zero;
        canvas.transform.localRotation = Quaternion.identity;
        canvas.transform.localScale = Vector3.one * 0.001f;

        // Background panel
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvas.transform);
        UnityEngine.UI.Image bg = panel.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(color.r, color.g, color.b, 0.7f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(200, 60);
        panelRect.localPosition = Vector3.zero;
        panelRect.localRotation = Quaternion.identity;
        panelRect.localScale = Vector3.one;

        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(panel.transform);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 28;
        tmp.color = Color.black;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(200, 60);
        textRect.localPosition = Vector3.zero;
        textRect.localRotation = Quaternion.identity;
        textRect.localScale = Vector3.one;

        return root;
    }

    void Update()
    {
        // Always face the camera
        Camera cam = Camera.main;
        if (cam == null) return;

        if (leftCanvas != null)
            leftCanvas.transform.LookAt(
                leftCanvas.transform.position + cam.transform.rotation * Vector3.forward,
                cam.transform.rotation * Vector3.up);

        if (rightCanvas != null)
            rightCanvas.transform.LookAt(
                rightCanvas.transform.position + cam.transform.rotation * Vector3.forward,
                cam.transform.rotation * Vector3.up);
    }
}