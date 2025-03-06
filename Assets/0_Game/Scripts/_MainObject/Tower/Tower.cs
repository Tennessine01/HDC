using UnityEngine;

public abstract class Tower : MonoBehaviour
{
    [Header("Link tới SO")]
    public TowerData  soData;  

    [Header("Level Tower (1..3)")]
    public int towerLevel = 1; // Cấp hiện tại

    protected float damage;
    protected float range;
    protected float fireRate;

    protected float fireCooldown = 0f;
    protected Transform target;  // Mục tiêu hiện tại

    // Slot hiện tại mà tower đang đứng (để hỗ trợ merge/swap khi di chuyển)
    public Slot currentSlot;

    // Điều khiển bắn: khi tower bị nhấc lên thì tạm dừng bắn
    protected bool canFire = true;

    [Header("Shooting Components")]
    public Transform turret;     // Phần của Tower sẽ xoay theo hướng bắn (ví dụ: hình ảnh, sprite turret)
    public Transform firePoint;  // Vị trí spawn đạn ra

    protected virtual void Start()
    {
        ApplyStats();
    }
    public void SetData(TowerData dt)
    {
        soData = dt;
    }
    protected virtual void Update()
    {
        if (!canFire) return;

        // Nếu có target được xác định qua trigger, xoay turret và bắn đạn
        if (target != null)
        {
            RotateTurretTowardsTarget();
            if (fireCooldown <= 0f)
            {
                Fire(target);
                fireCooldown = 1f / fireRate;
            }
        }
        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;
    }

    public void ApplyStats()
    {
        damage = soData.GetDamageByLevel(towerLevel);
        range = soData.GetRangeByLevel(towerLevel);
        fireRate = soData.GetFireRateByLevel(towerLevel);
    }

    // Thay vì find toàn bộ bot, dùng trigger để nhận target
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bot"))
        {
            Bot bot = other.GetComponent<Bot>();
            if (MapManager.Ins.TakeFirstBot() != null && MapManager.Ins.TakeFirstBot() == bot)
            {
                target = other.transform;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Bot"))
        {
            Bot bot = other.GetComponent<Bot>();
            if (MapManager.Ins.TakeFirstBot() != null && MapManager.Ins.TakeFirstBot() == bot)
            {
                target = other.transform;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Bot"))
        {
            if (target == other.transform)
            {
                target = null;
            }
        }
    }

    private void RotateTurretTowardsTarget()
    {
        if (turret == null || target == null) return;
        Vector3 direction = target.position - turret.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        turret.rotation = Quaternion.Euler(0, 0, angle);
        if (firePoint != null)
        {
            firePoint.rotation = turret.rotation;
        }
    }

    protected virtual void Fire(Transform tgt)
    {
        if (firePoint == null) return;
        // Spawn đạn từ Pool sử dụng bulletPoolType trong SO
        BulletBase bullet = SimplePool.Spawn<BulletBase>((PoolType)soData.bulletType, firePoint.position, firePoint.rotation);
        if (bullet != null)
        {
            bullet.Initialize(this, tgt, damage);
        }
    }

    public virtual void OnPickup()
    {
        canFire = false;
    }

    public virtual void OnDrop()
    {
        canFire = true;
    }

    public virtual void UpgradeLevel()
    {
        towerLevel++;
        if (towerLevel > 3) towerLevel = 3;
        ApplyStats();
    }

    private void OnMouseDown()
    {
        // Khi click trực tiếp vào tower, báo cho TowerDragManager xử lý (pickup, merge, swap,...)
        TowerDragManager.Instance.PickUpExistingTower(this);
    }
}
