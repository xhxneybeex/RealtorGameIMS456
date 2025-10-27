using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StartCrap : MonoBehaviour
{

    private LightFlicker lf;

    private Float f;
    // Start is called before the first frame update
    void Start()
    {

        lf = GameObject.Find("Point Light").GetComponent<LightFlicker>();
        f = GameObject.Find("Plate").GetComponent<Float>();

    }

    // Update is called once per frame
    void Update()
    {


    }

    private void onEnterTrigger(Collider ollider)
    {
        if (ollider.CompareTag("Trigger"))
        {
            lf.flickIt();
            f.startFloat();

        }

    }
}
