using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PurchaseManager : MonoBehaviour
{
    [SerializeField] public Text objectName;
    [SerializeField] public Text objectDescription;
    [SerializeField] public Text objectPrice;

    [SerializeField] public GameObject purchaseScreen;
    private int amount = 0;

    public bool spaceHeater = false;

    private Purchaser p;

    public bool furnace = false;
    // Start is called before the first frame update
    void Start()
    {
        p = GameObject.Find("Selector").GetComponent<Purchaser>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void spaceHeaters()
    {
        spaceHeater = true;
        purchaseScreen.gameObject.SetActive(true);

        objectName.text = "Space Heater";
        objectDescription.text = "Small Device meant to keep an area warm";
        amount = 50;
        objectPrice.text = "Price: $" + amount;
    }

    public void furnaces()
    {
        furnace = true;
        purchaseScreen.gameObject.SetActive(true);
        objectName.text = "Furnace";
        objectDescription.text = "Meant to heat the entire home!";
        amount = 250;
        objectPrice.text = "Price: $" + amount;
    }

    public void buy()
    {
        p.subtractMoney(amount);
        purchaseScreen.gameObject.SetActive(false);

    }

    public void cont()
    {
        SceneManager.LoadScene("MainScene");
    }

}
