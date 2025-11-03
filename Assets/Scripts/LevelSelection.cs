using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelection : MonoBehaviour
{
    public bool single = false;
    public bool doub = false;

    [SerializeField] public GameObject warning;

    public bool triple = false;

    public bool singleWin = false;
    public bool doubleWin = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void singles()
    {
        single = true;
        SceneManager.LoadScene("PurchaseScreen");

    }

    public void doubles()
    {
        if (singleWin == true)
        {
            doub = true;
            SceneManager.LoadScene("PurchasScreen");
        }
        else
        {
            warning.gameObject.SetActive(true);
            StartCoroutine(NotReady(2.5f));
        }

    }

    public void triples()
    {
        if (singleWin == true && doubleWin == true)
        {
            triple = true;
            SceneManager.LoadScene("PurchaseScreen");
        }
        else
        {
            warning.gameObject.SetActive(true);
            StartCoroutine(NotReady(2.5f));
        }

    }

    private IEnumerator NotReady(float f)
    {
        yield return new WaitForSeconds(f);
        warning.gameObject.SetActive(false);
    }
}
