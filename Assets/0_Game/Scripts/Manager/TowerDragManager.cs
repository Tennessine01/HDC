using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TowerDragManager : MonoBehaviour
{
    #region Singleton
    public static TowerDragManager Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    #endregion

    [SerializeField] private TowerData[] towerDataaa;

    [Header("===== Tower Mới (từ UI) =====")]
    [SerializeField] private List<GameObject> towerDragIconPrefabList;
    // Prefab UI icon tương ứng với từng tower (4 loại)
    [SerializeField] private RectTransform uiCanvas;
    // Canvas chứa icon UI
    private GameObject currentDragIcon;    // Icon UI đang kéo
    private RectTransform dragIconRect;
    private TowerData currentTowerData;    // Dữ liệu của tower mới (SO)
    private int currentTowerIndex;         // Index của tower (để cập nhật giá, UI, v.v.)
    private bool isDraggingNew = false;    // Đang kéo tower mới?

    [Header("===== Tower Cũ (trên scene) =====")]
    [SerializeField] private LayerMask slotLayer;
    [SerializeField] private LayerMask towerLayer;
    private Tower towerInHand = null;      // Tower cũ đang được nhấc
    private Slot oldSlot = null;           // Slot cũ của tower được nhấc
    private bool isDraggingExisting = false;

    // Slot đang được highlight (dùng chung cho cả hai luồng)
    private Slot hoveredSlot = null;

    private void Start()
    {
        // Lấy towerDatas từ DataManager
        towerDataaa = DataManager.ins.towerDatas;
    }

    private void Update()
    {
        // 1) Nếu không đang kéo gì, kiểm tra bấm chuột để pick tower cũ (sử dụng raycast thủ công)
        if (!isDraggingNew && !isDraggingExisting)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = Input.mousePosition;
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
                worldPos.z = 0f;
                RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, towerLayer);
                if (hit.collider != null)
                {
                    Tower tower = hit.collider.GetComponent<Tower>();
                    if (tower != null)
                    {
                        PickUpExistingTower(tower);
                    }
                }
            }
        }

        // 2) Nếu đang kéo tower mới từ UI
        if (isDraggingNew && currentDragIcon != null)
        {
            Vector2 screenPos = Input.mousePosition;
            dragIconRect.position = screenPos;
            CheckSlotUnderMouse(screenPos, isExistingTower: true);

            if (Input.GetMouseButtonUp(0))
            {
                EndDragNewTower();
            }
        }
        // 3) Nếu đang kéo tower cũ (đã pick up trên scene)
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

    #region [1] KÉO TOWER MỚI (TỪ UI)
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
        isDraggingNew = false;
        // Hủy icon UI sau khi thả
        if (currentDragIcon != null)
            Destroy(currentDragIcon);

        // Nếu có slot được highlight
        if (hoveredSlot != null)
        {
            // Nếu slot trống => spawn tower mới
            if (!hoveredSlot.hasTower)
            {
                Vector3 slotPos = hoveredSlot.transform.position;
                GameObject towerObj = Instantiate(currentTowerData.towerPrefab, slotPos, Quaternion.identity);
                Tower newTower = towerObj.GetComponent<Tower>();
                if (newTower != null)
                {
                    newTower.SetData(currentTowerData);
                    // Gán tower mới vào slot (chỉ cập nhật vị trí, không làm parent)
                    hoveredSlot.SetTower(newTower);
                }
                // Thông báo cho UI (trừ tiền, tăng giá, vv.)
                CanvasGameplay ui = FindObjectOfType<CanvasGameplay>();
                if (ui != null)
                {
                    ui.OnTowerPlacedSuccessfully(currentTowerIndex);
                }
            }
            else if(hoveredSlot.hasTower)
            {
                Debug.Log("===========");
                Tower occupant = hoveredSlot.occupantTower;
                if (occupant != null && occupant.soData == currentTowerData && occupant.towerLevel == 1)
                {
                    // Merge: nâng cấp tower đã đặt
                    occupant.UpgradeLevel();
                    CanvasGameplay ui = FindObjectOfType<CanvasGameplay>();
                    if (ui != null)
                    {
                        ui.OnTowerPlacedSuccessfully(currentTowerIndex);
                    }
                }
                else
                {
                    Debug.Log("Slot đã có tower nhưng không merge được, hủy đặt tower mới.");
                }
            }
            hoveredSlot.SetHighlight(false);
            hoveredSlot = null;
        }
        else
        {
            Debug.Log("Không thả vào slot, hủy icon UI");
        }

        currentDragIcon = null;
        dragIconRect = null;
        currentTowerData = null;
        currentTowerIndex = -1;
    }
    #endregion

    #region [2] KÉO TOWER CŨ (pickup, merge, swap)
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
                    Debug.LogWarning("Slot báo có tower nhưng occupant null.");
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
                Debug.Log("Không thả vào slot và không có oldSlot, hủy tower.");
                Destroy(towerInHand.gameObject);
            }
        }
        towerInHand = null;
        oldSlot = null;
    }
    #endregion

    #region [3] CheckSlotUnderMouse (dùng chung)
    private void CheckSlotUnderMouse(Vector2 screenPos, bool isExistingTower)
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, slotLayer);
        if (hit.collider != null)
        {
            Slot slot = hit.collider.GetComponent<Slot>();
            if (slot != null)
            {
                // Khi kéo tower mới, chỉ highlight slot rỗng; khi kéo tower cũ, highlight tất cả
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
    #endregion

    #region [4] Merge Condition
    private bool CanMerge(Tower t1, Tower t2)
    {
        // Giả sử merge khi cùng soData và cùng cấp
        if (t1.soData == t2.soData && t1.towerLevel == t2.towerLevel)
        {
            // Giới hạn merge tối đa, ví dụ level < 3
            if (t2.towerLevel < 3)
                return true;
        }
        return false;
    }
    #endregion
}
