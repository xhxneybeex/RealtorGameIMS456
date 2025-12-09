using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    private Light lightComponent;
    private float originalIntensity;
    private bool isFlickering = false;

    public float minFlickerTime = 0.05f;
    public float maxFlickerTime = 0.3f;
    public float minIntensity = 0.1f;

    private float nextFlickerTime;

    void Awake()
    {
        lightComponent = GetComponent<Light>();
        if (lightComponent != null)
        {
            originalIntensity = lightComponent.intensity;
        }
    }

    void Update()
    {
        if (!isFlickering || lightComponent == null)
        {
            return;
        }

        if (Time.time >= nextFlickerTime)
        {
            lightComponent.intensity = Random.Range(minIntensity, originalIntensity);
            lightComponent.enabled = Random.value > 0.2f;
            
            nextFlickerTime = Time.time + Random.Range(minFlickerTime, maxFlickerTime);
        }
    }

    public void StartFlickering()
    {
        isFlickering = true;
        nextFlickerTime = Time.time;
    }

    public void StopFlickering()
    {
        isFlickering = false;
        
        if (lightComponent != null)
        {
            lightComponent.intensity = originalIntensity;
            lightComponent.enabled = true;
        }
    }
}
