using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeakBot : Bot
{
    private void Awake()
    {
        maxHP = 50f;
        speed = 5f;
    }
}
