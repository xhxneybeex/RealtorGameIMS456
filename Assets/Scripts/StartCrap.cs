using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StartCrap : MonoBehaviour
{

    private LightFlicker lf;
    // Start is called before the first frame update
    void Start()
    {

        lf = GameObject.Find("Point Light").GetComponent<LightFlicker>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void onEnterTrigger(Collider ollider)
    {
        if (ollider.CompareTag("Player"))
        {
            lf.flickIt();

        }
    }
}
