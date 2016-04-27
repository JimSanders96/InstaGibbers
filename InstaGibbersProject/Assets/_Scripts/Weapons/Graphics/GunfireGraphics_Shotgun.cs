﻿using UnityEngine;
using System.Collections;

public class GunfireGraphics_Shotgun : GunfireGraphics {

    public void SetLineRendererLength(float length)
    {
        lineRenderer.SetPosition(1, new Vector3(0, 0, length));
    }
}