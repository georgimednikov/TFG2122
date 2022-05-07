using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float night = 0;
    public float nightLight = 0.1f;
    public float dayLight = 1;
    public Material skyboxMaterial;

    float lightDiff;
    Light lightSource;

    private void Start()
    {
        lightDiff = dayLight - nightLight;
        lightSource = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        skyboxMaterial.SetFloat("_Blend", night);
        lightSource.intensity = Lerp();
    }

    float Lerp()
    {
        return dayLight - (lightDiff * night);
    }
}
