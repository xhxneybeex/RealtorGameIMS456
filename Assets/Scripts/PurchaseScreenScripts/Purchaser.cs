using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Purchaser : MonoBehaviour
{

    [SerializeField] private Text money;

    [SerializeField] private GameObject warning;

    private PurchaseManager pm;

    Ray cameraRay;

    public int amount = 100;

    // Start is called before the first frame update
    void Start()
    {
        pm = GameObject.Find("PurchaseManager").GetComponent<PurchaseManager>();

    }

    // Update is called once per frame
    void Update()
    {
        money.text = "Money: $" + amount;
        if (Input.GetMouseButton(0))
        {
            FireRayCast();
        }


    }

    public void subtractMoney(int money)
    {
        if (money <= amount)
        {
            amount = amount - money;
        }
        else
        {
            warning.gameObject.SetActive(true);
            StartCoroutine(NotReady(2.5f));
        }
    }

    void FireRayCast()
    {
        cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(cameraRay, out RaycastHit hitObject))
        {
            if (hitObject.collider.CompareTag("SpaceHeater"))
            {
                pm.spaceHeaters();
            }
            else if (hitObject.collider.CompareTag("Furnace"))
            {
                pm.furnaces();
            }
        }
    }

    private IEnumerator NotReady(float f)
    {
        yield return new WaitForSeconds(f);
        warning.gameObject.SetActive(false);
    }
}
