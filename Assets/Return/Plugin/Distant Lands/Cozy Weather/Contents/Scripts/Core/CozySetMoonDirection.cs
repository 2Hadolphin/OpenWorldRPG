﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CozySetMoonDirection : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {

        Shader.SetGlobalVector("CZY_MoonDirection", -transform.forward);

    }
}
