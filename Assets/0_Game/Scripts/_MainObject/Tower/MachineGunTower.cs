using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGunTower : Tower
{
    private void Awake()
    {
        damage = 5f;
        fireRate = 4f; 
        range = 5f;
    }
}
