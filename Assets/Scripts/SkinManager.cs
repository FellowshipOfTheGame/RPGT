using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinManager : MonoBehaviour {

    public Sprite[] skinList;
    private SpriteRenderer spriteR;
    private int index = 0;

    void Start() {
        spriteR = gameObject.GetComponent<SpriteRenderer>();
    }

    public void ChangeSkin() {
        if (index >= skinList.Length) index = 0;
        spriteR.sprite = skinList[index];
        index++;
    }

}
