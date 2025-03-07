using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeakBot : Bot
{
    private void Awake()
    {
        maxHP = 20f;
        speed = 2f;
        money = 5;
    }
}
