using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonTower : Tower
{
    private void Awake()
    {
        damage = 15f;
        fireRate = 1f;
        range = 6f;
    }
}
