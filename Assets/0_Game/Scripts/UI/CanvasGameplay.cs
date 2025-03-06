using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasGameplay : UICanvas
{
    #region Fields & Inspector
    [SerializeField] private TextMeshProUGUI txtGold;

    private TowerData[] _towerDatas;

    [SerializeField] private TextMeshProUGUI[] towerCostTexts;

    [SerializeField] private Image[] icons;

    // Lưu giá hiện tại mỗi Tower (cứ mua xong thì +7)
    private static int[] currentCosts;
    #endregion

    #region Unity Methods
    private void Start()
    {
        PreLoadUI();
        UpdateGoldUI();
    }
    #endregion

    #region Khởi tạo UI
    /// <summary>
    /// Lấy towerDatas từ DataManager (nếu chưa load)
    /// Gán icon, cost ban đầu (baseCost) v.v.
    /// </summary>
    public void PreLoadUI()
    {
        if (_towerDatas != null) return;

        // Lấy mảng towerDatas từ DataManager
        _towerDatas = DataManager.ins.towerDatas;
        currentCosts = new int[_towerDatas.Length];

        for (int i = 0; i < _towerDatas.Length; i++)
        {
            if (_towerDatas[i] != null)
            {
                currentCosts[i] = _towerDatas[i].GetBaseCost();

                if (icons != null && icons.Length > i && icons[i] != null)
                {
                    icons[i].sprite = _towerDatas[i].GetIcon();
                }

                if (towerCostTexts != null && towerCostTexts.Length > i && towerCostTexts[i] != null)
                {
                    towerCostTexts[i].text = currentCosts[i].ToString();
                }
            }
        }
    }
    #endregion

    #region Hiển thị Vàng
    /// <summary>
    /// Cập nhật text hiển thị vàng. Lấy playerGold từ MapManager.
    /// </summary>
    public void UpdateGoldUI()
    {
        if (txtGold != null && MapManager.Ins != null)
        {
            txtGold.text = MapManager.Ins.playerGold.ToString();
        }
    }
    #endregion

    #region Button Tower
    /// <summary>
    /// Gọi khi bấm nút Tower từ towerIndex.
    /// Chưa trừ tiền, chỉ báo cho TowerDragManager spawn Tower "ghost".
    /// </summary>
    public void OnClickTowerButton(int towerIndex)
    {
        TowerData data = _towerDatas[towerIndex];
        if (!MapManager.Ins.CanBuy(currentCosts[towerIndex])){return; }
        // Gọi TowerDragManager => bắt đầu kéo-thả
        TowerDragManager.Instance.StartDragTower(towerIndex, data);
    }

    /// <summary>
    /// Hàm này được TowerDragManager gọi khi đặt tower thành công lên Slot.
    /// 1) Trừ tiền (MapManager.Ins.SpendGold)
    /// 2) Tăng cost +7
    /// 3) Cập nhật text cost & vàng trên UI
    /// </summary>
    public void OnTowerPlacedSuccessfully(int towerIndex)
    {
        int cost = currentCosts[towerIndex];


        if (!MapManager.Ins.CanBuy(cost))
        {
            return;
        }

        // Trừ tiền
        MapManager.Ins.SpendGold(cost);

        // Tăng giá tower đó thêm 7
        currentCosts[towerIndex] += 7;
        towerCostTexts[towerIndex].text = currentCosts[towerIndex].ToString();

        // Cập nhật UI gold
        UpdateGoldUI();
    }
    #endregion
}
