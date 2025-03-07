using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Ins;

    [Header("Player HP")]
    public int playerHP = 10;
    [Header("Danh sách TowerData (4 loại)")]
    [SerializeField] private TowerData[] towerDatas;
    [Header("Line Points")]
    // Điểm đầu, điểm cuối, và (tùy ý) các điểm road trung gian
    public Transform startPoint;
    public Transform endPoint;
    public List<Transform> middleRoadPoints = new List<Transform>();

    // Danh sách các vị trí liên tục để Bot di chuyển
    // (start → middleRoadPoints… → end)
    [HideInInspector]
    public List<Vector3> fullRoadPoints = new List<Vector3>();

    public List<Bot> listBot;
    public Bot activeBots;
    private int aliveBots = 0;

    private CanvasGameplay cachedUI;


    // Để biết đã spawn xong tất cả wave chưa
    private bool allWavesSpawned = false;

    private void Awake()
    {
        Ins = this;
    }

    private void Start()
    {
        towerDatas = DataManager.ins.towerDatas;
        cachedUI = FindObjectOfType<CanvasGameplay>();
        cachedUI.UpdateGoldUI();
        cachedUI.UpdateHealth();
        BuildRoadLine();
        StartCoroutine(SpawnWaves());
    }
   
    private void BuildRoadLine()
    {
        fullRoadPoints.Clear();

        if (startPoint != null)
            fullRoadPoints.Add(startPoint.position);

        for (int i = 0; i < middleRoadPoints.Count; i++)
        {
            if (middleRoadPoints[i] != null)
                fullRoadPoints.Add(middleRoadPoints[i].position);
        }

        if (endPoint != null)
            fullRoadPoints.Add(endPoint.position);
    }

    /// <summary>
    /// Quản lý 4 đợt spawn: 
    /// 1) 10 Bot Normal
    /// 2) 5s => 10 Bot Weak
    /// 3) 5s => 5 Bot Tough
    /// 4) 5s => 20 Bot Normal
    /// </summary>
    private IEnumerator SpawnWaves()
    {
        //yield return new WaitForSeconds(1f);

        // Wave 1: 10 Bot Normal
        yield return StartCoroutine(SpawnOneWave(PoolType.Minion_1, 10));

        yield return new WaitForSeconds(30f);

        // Wave 2: 10 Bot Weak
        yield return StartCoroutine(SpawnOneWave(PoolType.Minion_2, 10));

        yield return new WaitForSeconds(10f);

        // Wave 3: 5 Bot Tough
        yield return StartCoroutine(SpawnOneWave(PoolType.Minion_3, 5));

        yield return new WaitForSeconds(25f);

        // Wave 4: 20 Bot Normal
        yield return StartCoroutine(SpawnOneWave(PoolType.Minion_1, 20));

        // Đã spawn xong tất cả wave
        allWavesSpawned = true;
    }

    private IEnumerator SpawnOneWave(PoolType botType, int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Spawn từ SimplePool
            // Giả sử ta lấy vị trí spawn = startPoint
            Vector3 spawnPos = (startPoint != null) ? startPoint.position : Vector3.zero;

            Bot newBot = SimplePool.Spawn<Bot>(botType, spawnPos, Quaternion.identity);
            // Gọi OnInit() để khởi tạo Bot
            listBot.Add(newBot);
            newBot.OnInit();

            yield return new WaitForSeconds(2.3f);
        }
    }
    public void OnBotSpawned()
    {
        aliveBots++;
    }
    public Bot TakeFirstBot()
    {
        if (listBot.Count > 0)
        {
            return listBot[0];
        }
        else { return null; }
    }
    public void OnBotDespawned(Bot bot)
    {
        listBot.Remove(bot);
        aliveBots--;
        if (aliveBots < 0) aliveBots = 0;

        if (allWavesSpawned && aliveBots <= 0)
        {
            Debug.Log("<color=cyan>YOU WIN!</color>");
            // TODO: Gọi UI Win, v.v.
        }
    }

    public void PlayerTakeDamage(int dmg)
    {
        playerHP -= dmg;
        cachedUI.UpdateHealth();
        Debug.Log("Player HP = " + playerHP);

        if (playerHP <= 0)
        {
            Debug.Log("<color=red>YOU LOSE!</color>");
            // TODO: Gọi UI Lose, v.v.
        }
    }

    // =========================== GOLD LOGIC ===========================
    [Header("Player Gold")]
    public int playerGold = 100;
    public bool CanBuy(int cost)
    {
        return playerGold >= cost;
    }

    public void SpendGold(int cost)
    {
        playerGold -= cost;
    }
    public void AddGold(int amount)
    {
        playerGold += amount;
        cachedUI.UpdateGoldUI();
    }
    private void UpdateGold()
    {
        if (cachedUI != null)
        {
            cachedUI.UpdateGoldUI();
        }
    }
}
