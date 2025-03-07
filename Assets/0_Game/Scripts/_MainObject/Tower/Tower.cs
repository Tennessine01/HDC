using UnityEngine;
using System.Collections.Generic;

public abstract class Tower : MonoBehaviour
{
    [Header("Link tới SO")]
    public TowerData soData;

    [Header("Level Tower (1..3)")]
    public int towerLevel = 1; // Cấp hiện tại

    protected float damage;
    public float range;
    protected float fireRate;
    protected float fireCooldown = 0f;

    // Target hiện tại (bot mà tower bắn)
    public Bot target;
    // Nếu cần lưu danh sách bot trong tầm (có thể dùng cho mục đích hiển thị, merge,...)
    public List<Bot> listTargetBots = new();

    // Slot hiện tại mà tower đứng (hỗ trợ merge/swap khi di chuyển)
    public Slot currentSlot;

    // Cho bắn: nếu tower bị nhấc lên, tạm dừng bắn
    protected bool canFire = true;

    [Header("Shooting Components")]
    public Transform turret;    // Phần xoay theo hướng bắn (ví dụ: sprite turret)
    public Transform firePoint; // Vị trí spawn đạn

    [Header("Visual Attack Range")]
    // Đối tượng con hiển thị tầm tấn công (hình tròn)
    public Transform attackRangeVisual;

    public GameObject visualRange;
    protected virtual void Start()
    {
        ApplyStats();
        UpdateAttackRangeVisual();
    }

    public void SetData(TowerData dt)
    {
        soData = dt;
    }

    protected virtual void Update()
    {
        if (!canFire) return;

        // Nếu target hiện tại bị mất hoặc ra khỏi tầm, cập nhật target mới
        if (target == null || Vector3.Distance(transform.position, target.transform.position) > range || target.isDespawn)
        {
            UpdateTarget();
        }

        if (target != null)
        {
            RotateTurretTowardsTarget();

            if (fireCooldown <= 0f)
            {
                Fire(target.transform);
                fireCooldown = 1f / fireRate;
            }
        }

        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;
    }

    /// <summary>
    /// Sử dụng OverlapCircleAll để quét các bot trong tầm và chọn bot có chỉ số nhỏ nhất trong MapManager.Ins.listBot.
    /// </summary>
    protected virtual void UpdateTarget()
    {
        // Giả sử các Bot nằm ở layer "Bot"
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, LayerMask.GetMask("Bot"));
        Bot selectedBot = null;
        int bestIndex = int.MaxValue;
        foreach (Collider2D col in hits)
        {
            Bot b = col.GetComponent<Bot>();
            if (b != null)
            {
                // Lấy vị trí bot trong danh sách active của MapManager
                int index = MapManager.Ins.listBot.IndexOf(b);
                if (index != -1 && index < bestIndex)
                {
                    bestIndex = index;
                    selectedBot = b;
                }
            }
        }
        target = selectedBot;
    }

    public void ApplyStats()
    {
        damage = soData.GetDamageByLevel(towerLevel);
        range = soData.GetRangeByLevel(towerLevel);
        fireRate = soData.GetFireRateByLevel(towerLevel);
    }

    public virtual void TargetBot(Bot bot)
    {
        this.target = bot;
    }

    public virtual void RemoveTarget(Bot bot)
    {
        if (target == bot)
            target = null;
    }
    public void UpdateAttackRangeVisual()
    {
        if (attackRangeVisual != null)
        {
            attackRangeVisual.localScale = new Vector3(range*2, range*2);
        }
    }

    private void RotateTurretTowardsTarget()
    {
        if (turret == null || target == null) return;
        Vector3 direction = target.transform.position - turret.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        turret.rotation = Quaternion.Euler(0, 0, angle);
        if (firePoint != null)
            firePoint.rotation = turret.rotation;
    }

    protected virtual void Fire(Transform tgt)
    {
        if (firePoint == null) return;
        // Spawn đạn từ Pool sử dụng bulletPoolType từ soData
        BulletBase bullet = SimplePool.Spawn<BulletBase>((PoolType)soData.bulletType, firePoint.position, firePoint.rotation);
        if (bullet != null)
            bullet.Initialize(this, tgt, damage);
    }

    public virtual void OnPickup()
    {
        canFire = false;
        target = null;
        listTargetBots.Clear();
        visualRange.SetActive(true);
    }

    public virtual void OnDrop()
    {
        canFire = true;
        visualRange.SetActive(false);
    }

    public virtual void UpgradeLevel()
    {
        Bot currentTarget = target;

        towerLevel++;
        if (towerLevel > 3) towerLevel = 3;
        ApplyStats();
        UpdateAttackRangeVisual();
        if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.transform.position) <= range)
        {
            target = currentTarget;
        }

    }

    private void OnValidate()
    {
        if (attackRangeVisual != null)
        {
            attackRangeVisual.localScale = new Vector3(range*2, range *2);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ vòng tròn tầm bắn cho dễ xem trong Editor
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
