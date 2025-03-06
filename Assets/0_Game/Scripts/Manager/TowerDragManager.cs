using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TowerDragManager : MonoBehaviour
{
    public static TowerDragManager Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [SerializeField] private TowerData[] towerDataaa;

    [Header("===== Tower Mới (từ UI) =====")]
    [SerializeField] private List<GameObject> towerDragIconPrefabList;
    [SerializeField] private RectTransform uiCanvas;
    private GameObject currentDragIcon;
    private RectTransform dragIconRect;
    private TowerData currentTowerData;
    private int currentTowerIndex;
    private bool isDraggingNew = false;

    [Header("===== Tower Cũ (trên scene) =====")]
    [SerializeField] private LayerMask slotLayer;
    private Tower towerInHand = null;
    private Slot oldSlot = null;
    private bool isDraggingExisting = false;

    private Slot hoveredSlot = null;

    private void Start()
    {
        // Lấy towerDatas từ DataManager
        towerDataaa = DataManager.ins.towerDatas;
    }

    private void Update()
    {
        if (isDraggingNew && currentDragIcon != null)
        {
            Vector2 screenPos = Input.mousePosition;
            dragIconRect.position = screenPos;

            CheckSlotUnderMouse(screenPos, isExistingTower: false);

            if (Input.GetMouseButtonUp(0))
            {
                EndDragNewTower();
            }
        }
        else if (isDraggingExisting && towerInHand != null)
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            worldPos.z = 0f;
            towerInHand.transform.position = worldPos;

            CheckSlotUnderMouse(mousePos, isExistingTower: true);

            if (Input.GetMouseButtonUp(0))
            {
                EndDragExistingTower();
            }
        }
    }

    public void StartDragTower(int towerIndex, TowerData data)
    {
        if (isDraggingExisting) return;

        isDraggingNew = true;
        currentTowerIndex = towerIndex;
        currentTowerData = data;

        GameObject iconPrefab = towerDragIconPrefabList[towerIndex];
        currentDragIcon = Instantiate(iconPrefab, uiCanvas);
        dragIconRect = currentDragIcon.GetComponent<RectTransform>();
        dragIconRect.position = Input.mousePosition;
    }

    private void EndDragNewTower()
    {
        Destroy(currentDragIcon);
        isDraggingNew = false;

        if (hoveredSlot != null && !hoveredSlot.hasTower)
        {
            Vector3 slotPos = hoveredSlot.transform.position;
            GameObject towerObj = Instantiate(currentTowerData.towerPrefab, slotPos, Quaternion.identity);
            Tower newTow = towerObj.GetComponent<Tower>();
            newTow.SetData(currentTowerData);
            if (newTow != null && hoveredSlot != null)
            {
                hoveredSlot.SetTower(newTow);
            }

            CanvasGameplay ui = FindObjectOfType<CanvasGameplay>();
            if (ui != null)
            {
                ui.OnTowerPlacedSuccessfully(currentTowerIndex);
            }

            hoveredSlot.SetHighlight(false);
            hoveredSlot = null;
        }
        else
        {
            Debug.Log("Không đặt tower, hủy icon UI");
        }

        if (currentDragIcon != null) Destroy(currentDragIcon);
        currentDragIcon = null;
        dragIconRect = null;
        currentTowerData = null;
        currentTowerIndex = -1;
    }

    public void PickUpExistingTower(Tower tower)
    {
        if (isDraggingNew) return;

        isDraggingExisting = true;
        towerInHand = tower;

        oldSlot = tower.currentSlot;
        if (oldSlot != null)
        {
            oldSlot.RemoveTower();
        }

        towerInHand.OnPickup();
        Debug.Log("Pickup tower cũ: " + towerInHand.name);
    }

    private void EndDragExistingTower()
    {
        isDraggingExisting = false;

        if (hoveredSlot != null)
        {
            if (!hoveredSlot.hasTower)
            {
                towerInHand.transform.position = hoveredSlot.transform.position;
                hoveredSlot.SetTower(towerInHand);
                towerInHand.OnDrop();
            }
            else
            {
                Tower occupant = hoveredSlot.occupantTower;
                if (occupant != null)
                {
                    if (CanMerge(towerInHand, occupant))
                    {
                        occupant.UpgradeLevel();
                        Destroy(towerInHand.gameObject);
                    }
                    else
                    {
                        if (oldSlot != null)
                        {
                            oldSlot.SetTower(occupant);
                            occupant.transform.position = oldSlot.transform.position;
                        }
                        hoveredSlot.SetTower(towerInHand);
                        towerInHand.transform.position = hoveredSlot.transform.position;
                        towerInHand.OnDrop();
                    }
                }
                else
                {
                    // if occupantTower = null nhưng hasTower = true => code cũ chưa đồng bộ
                    // Tùy bạn xử lý, tạm in:
                    Debug.LogWarning("hasTower = true nhưng occupantTower == null?");
                }
            }

            hoveredSlot.SetHighlight(false);
            hoveredSlot = null;
        }
        else
        {
            if (oldSlot != null)
            {
                oldSlot.SetTower(towerInHand);
                towerInHand.transform.position = oldSlot.transform.position;
                towerInHand.OnDrop();
            }
            else
            {
                Debug.Log("Không thả vào slot, không có oldSlot => hủy Tower?");
                Destroy(towerInHand.gameObject);
            }
        }

        towerInHand = null;
        oldSlot = null;
    }

    private void CheckSlotUnderMouse(Vector2 screenPos, bool isExistingTower)
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, slotLayer);
        if (hit.collider != null)
        {
            Slot slot = hit.collider.GetComponent<Slot>();
            if (slot != null)
            {
                bool canHighlight = isExistingTower ? true : !slot.hasTower;
                if (canHighlight)
                {
                    if (slot != hoveredSlot)
                    {
                        if (hoveredSlot != null) hoveredSlot.SetHighlight(false);
                        hoveredSlot = slot;
                        hoveredSlot.SetHighlight(true);
                    }
                    return;
                }
            }
        }
        if (hoveredSlot != null)
        {
            hoveredSlot.SetHighlight(false);
            hoveredSlot = null;
        }
    }

    private bool CanMerge(Tower t1, Tower t2)
    {
        if (t1.soData == t2.soData && t1.towerLevel == t2.towerLevel)
        {
            if (t2.towerLevel < 3) return true;
        }
        return false;
    }
}
