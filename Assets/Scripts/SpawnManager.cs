using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> spawnableModels;
    private int selectedIndex;
    private Vector3 lookPoint;
    private float placeAngle;

    [Header("Asset Refs")]
    [SerializeField] private GameObject spawnEquipmentButtonPrefab;
    [SerializeField] private Material onHoverMaterial;
    [SerializeField] private Material onGrabMaterial;

    [SerializeField] private InputActionReference toggleSpawnMenu;
    [SerializeField] private InputActionReference spawnObject;
    [SerializeField] private InputActionReference navigateUp;
    [SerializeField] private InputActionReference navigateDown;
    [SerializeField] private InputActionReference stickRotate;

    [Header("Scene Refs")]
    [SerializeField] private Canvas spawnCanvas;
    [SerializeField] private Transform equipmentTransform;
    [SerializeField] private GameObject placementGhost;

    private void Start()
    {
        spawnCanvas.worldCamera = Camera.main;
        PopulateEquipment();

        SelectButton(0);
    }

    private void Update()
    {
        // Ghost
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 5, Color.green);
        Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hitInfo, 5);
        if (hitInfo.collider != null)
        {
            lookPoint = hitInfo.point;
        }
        else
        {
            lookPoint = Camera.main.transform.position + Camera.main.transform.forward * 5;
        }
        if (placementGhost != null && placementGhost.GetComponent<MeshRenderer>() != null)
        {
            placementGhost.transform.position = lookPoint + Vector3.up * placementGhost.GetComponent<MeshRenderer>().bounds.size.y / 2f;
            placementGhost.transform.rotation = Quaternion.Euler(0, placeAngle, 0);
        }

        // Input
        if (toggleSpawnMenu.action.WasPressedThisFrame())
        {
            spawnCanvas.gameObject.SetActive(!spawnCanvas.gameObject.activeSelf);

            if (!spawnCanvas.gameObject.activeSelf)
            {
                Destroy(placementGhost);
            }
            else
            {
                SelectButton(selectedIndex);
            }
        }

        if (navigateUp.action.WasPressedThisFrame())
        {
            selectedIndex -= 1;
            if (selectedIndex < 0 )
            {
                selectedIndex = spawnableModels.Count - 1;
            }
            SelectButton(selectedIndex);
        }
        if (navigateDown.action.WasPressedThisFrame())
        {
            selectedIndex += 1;
            selectedIndex %= spawnableModels.Count;
            SelectButton(selectedIndex);
        }

        if (spawnObject.action.WasPressedThisFrame())
        {
            SpawnEquipment(selectedIndex);
        }

        placeAngle += stickRotate.action.ReadValue<Vector2>().x * Time.deltaTime * 100;
    }

    private void SelectButton(int index)
    {
        for (int i = 0; i < spawnCanvas.transform.childCount; i++)
        {
            Image image = spawnCanvas.transform.GetChild(i).GetComponent<Image>();
            image.color = new Color(0, 0, 0, 60 / 255f);
        }

        spawnCanvas.transform.GetChild(index).GetComponent<Image>().color = new Color(0, 0, 0, 200/255f);

        Destroy(placementGhost);
        placementGhost = Instantiate(spawnableModels[index]);
    }

    [ContextMenu("Populate Equipment")]
    public void PopulateEquipment()
    {
        if (spawnCanvas == null) return;

        for(int i = 0; i < transform.childCount; i++)
        {
            Destroy(spawnCanvas.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < spawnableModels.Count; i++)
        {
            GameObject model = spawnableModels[i];
            GameObject button = Instantiate(spawnEquipmentButtonPrefab, spawnCanvas.transform);
            button.GetComponentInChildren<TextMeshProUGUI>().text = model.name;
            int buttonIndex = i;
            button.GetComponent<Button>().onClick.AddListener(() => SpawnEquipment(buttonIndex));
        }
    }

    public void SpawnEquipment(int index)
    {
        if (index > spawnableModels.Count) return;
        GameObject spawnedModel = Instantiate(spawnableModels[index], equipmentTransform);
        spawnedModel.tag = "Interactable";
        spawnedModel.AddComponent<Rigidbody>();
        BoxCollider col = spawnedModel.AddComponent<BoxCollider>();
        spawnedModel.AddComponent<XRGrabInteractable>();
        HighlightOnHover highlight = spawnedModel.AddComponent<HighlightOnHover>();
        highlight.highlightHoverMaterial = onHoverMaterial;
        highlight.highlightGrabMaterial = onGrabMaterial;

        spawnedModel.transform.position = lookPoint + Vector3.up * col.size.y / 2;
        spawnedModel.transform.rotation = Quaternion.Euler(0, placeAngle, 0);
    }
}
