using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBot : Bot
{
    private void Awake()
    {
        maxHP = 60f;
        speed = 0.7f;
        money = 15;
    }
    public override void Die()
    {
        OnDespawn();
        MapManager.Ins.AddGold(money);
        SimplePool.Despawn(this);
    }
}
