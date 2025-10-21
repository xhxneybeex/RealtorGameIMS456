using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    private Light light;

    public float minIntensity = .5f;
    public float maxIntensity = 5.0f;

    public float flickerSpeed = 4.0f;

    public float randomIntensity;

    public bool flick = false;
    // Start is called before the first frame update
    void Start()
    {

        light = GetComponent<Light>();
        InvokeRepeating("Flicker", 0f, flickerSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        if (flick)
        {
            Flicker();
        }
        else if (!flick)
        {
            light.intensity = maxIntensity;
        }

    }

    private void Flicker()
    {
        randomIntensity = Random.Range(minIntensity, maxIntensity);

        light.intensity = randomIntensity;
    }

    public void flickIt()
    {
        flick = true;
    }

    public void stopFlick()
    {
        flick = false;
    }
}
