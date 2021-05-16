using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour {
    public GameObject hand;
    public GameObject[] weaponsList;
    private GameObject curWeapon;
    private int index = 0;

    void Start() {
        curWeapon = hand.transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    public void ChangeWeapon() {
        StartCoroutine(Change());
    }



    IEnumerator Change() {
        if (index >= weaponsList.Length) index = 0;
        Destroy(curWeapon);
        Instantiate(weaponsList[index],hand.transform);
        yield return new WaitForSeconds(0.1f);
        curWeapon = hand.transform.GetChild(0).gameObject;
        index++;
    }
}
