using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToughBot : Bot
{
    private void Awake()
    {
        maxHP = 200f; 
        speed = 0.5f;
        money = 20;
    }
}
