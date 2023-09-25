using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchLight : MonoBehaviour
{
    public float minIntensity, maxIntensity, frequency;

    private new Light light;

    private void Awake()
    {
        light = GetComponent<Light>();
    }

    private void Update()
    {
        light.intensity = minIntensity + Mathf.PingPong(frequency * Time.time, maxIntensity - minIntensity);
    }
}
