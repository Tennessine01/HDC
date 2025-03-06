using UnityEngine;

public class Slot : MonoBehaviour
{
    // Tower đang chiếm slot (nếu có). Null nếu slot trống.
    public Tower occupantTower = null;

    [SerializeField] private GameObject highlightObj;

    private void Start()
    {
        if (highlightObj != null)
            highlightObj.SetActive(false);
    }

    // Gọi khi cần bật/tắt highlight slot
    public void SetHighlight(bool on)
    {
        if (highlightObj != null)
            highlightObj.SetActive(on);
    }

    /// <summary>
    /// Đặt Tower vào slot này (chỉ gán reference, không parent).
    /// </summary>
    public void SetTower(Tower tower)
    {
        occupantTower = tower;
        if (tower != null)
        {
            tower.currentSlot = this;
        }
    }

    /// <summary>
    /// Gỡ tower khỏi slot này.
    /// </summary>
    public void RemoveTower()
    {
        occupantTower = null;
    }

    /// <summary>
    /// Kiểm tra slot đang rỗng hay không.
    /// </summary>
    public bool IsEmpty()
    {
        return occupantTower == null;
    }

    /// <summary>
    /// Thuộc tính hasTower (nếu bạn muốn giữ logic kiểu bool cũ).
    /// </summary>
    public bool hasTower
    {
        get { return occupantTower != null; }
    }
}
