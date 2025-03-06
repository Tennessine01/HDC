using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBot : Bot
{
    private void Awake()
    {
        maxHP = 100f;
        speed = 2f;
    }
}
