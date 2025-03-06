using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoeTower : Tower
{
    private void Awake()
    {
        damage = 8f;
        fireRate = 1.5f;
        range = 4f;
    }

    // Ví dụ bắn lan
    protected override void Fire(Transform target)
    {
        // Tìm tất cả bot trong bán kính “range nhỏ” quanh target để gây damage
        Collider2D[] hits = Physics2D.OverlapCircleAll(target.position, 1f);
        foreach (Collider2D col in hits)
        {
            Bot bot = col.GetComponent<Bot>();
            if (bot != null)
            {
                bot.TakeDamage(damage);
            }
        }
    }
}
