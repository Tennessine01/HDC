using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperTower : Tower
{
    private void Awake()
    {
        damage = 20f;
        fireRate = 0.5f; 
        range = 10f;
    }
}
