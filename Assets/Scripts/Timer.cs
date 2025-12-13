using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] float reamainingTime = 300f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (reamainingTime > 0)
        {
            reamainingTime -= Time.deltaTime;
        }
        else if (reamainingTime < 0)
        {
            reamainingTime = 0;
        }
        int minute = Mathf.FloorToInt(reamainingTime / 60);
        int seconds = Mathf.FloorToInt(reamainingTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minute, seconds);

    }
}
