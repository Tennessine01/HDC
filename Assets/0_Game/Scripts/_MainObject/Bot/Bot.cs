using UnityEngine;
using System.Collections.Generic;

public class Bot : GameUnit
{
    [Header("Bot Stats")]
    public float speed = 2f;
    public float maxHP = 50f;
    private float currentHP;

    // Chỉ số damage lên nhà (khi đến End)
    public int damageToPlayerBase = 1;

    // Để di chuyển dọc line
    private int currentPathIndex = 0;
    public override void OnInit()
    {
        // Reset HP
        currentHP = maxHP;

        // Reset chỉ số line
        currentPathIndex = 0;

        // Thông báo MapManager
        MapManager.Ins.OnBotSpawned();
    }

    public override void OnDespawn()
    {
        MapManager.Ins.OnBotDespawned(this);
    }
    public void TakeDamage(float dmg)
    {
        currentHP -= dmg;
        if (currentHP <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        // Thông báo MapManager
        OnDespawn();
        // Đưa bot về Pool
        SimplePool.Despawn(this);
    }

    #region Movement
    private void Update()
    {
        MoveAlongPath();
    }
    private void MoveAlongPath()
    {
        List<Vector3> path = MapManager.Ins.fullRoadPoints;
        if (path == null || path.Count == 0) return;

        // Nếu chưa đi hết path
        if (currentPathIndex < path.Count)
        {
            Vector3 target = path[currentPathIndex];
            // Di chuyển về phía target
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                speed * Time.deltaTime
            );

            // Nếu đã gần đến target
            float dist = Vector3.Distance(transform.position, target);
            if (dist < 0.05f)
            {
                currentPathIndex++;
                // Nếu vượt quá cuối => bot đã đến END
                if (currentPathIndex >= path.Count)
                {
                    // Gây damage cho Player
                    MapManager.Ins.PlayerTakeDamage(damageToPlayerBase);

                    // Bot này xong nhiệm vụ => despawn
                    MapManager.Ins.OnBotDespawned(this);
                    SimplePool.Despawn(this);
                }
            }
        }
    }
    #endregion
}
