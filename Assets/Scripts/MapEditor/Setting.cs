 using UnityEngine;  
 using System.Collections;  
 using UnityEngine.EventSystems;  
 using UnityEngine.UI;
 
public class Setting : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public Color baseColor;
    public Color hoverColor;
    
    public void OnPointerEnter(PointerEventData eventData){
        gameObject.GetComponentInChildren<Text>().color = hoverColor; 
     }
 
    public void OnPointerExit(PointerEventData eventData){
        gameObject.GetComponentInChildren<Text>().color = baseColor; 
    }
}