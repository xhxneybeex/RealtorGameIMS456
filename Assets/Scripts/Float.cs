using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Float : MonoBehaviour
{
    public float speed = 2f;
    public float height = 1.0f;

    public bool beginF = false;

    private Vector3 startPosition;
    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;

    }

    // Update is called once per frame
    void Update()
    {


        float newY = startPosition.y + Mathf.Sin(Time.time * speed) * height;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);



    }

    public void startFloat()
    {
        beginF = true;
    }

}
