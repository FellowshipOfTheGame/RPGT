using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour{
    public float normalSpeed;
    public float fastSpeed;
    public float movementSpeed;
    public float movementTime;
    public float rotationAmount;

    private float vertical;
    private float horizontal;

    public Vector3 newPosition;
    public Quaternion newRotation;

    public Vector3 dragStartPosition;
    public Vector3 dragCurrentPosition;
    public Vector3 rotateStartPosition;
    public Vector3 rotateCurrentPosition;

    void Start(){
        newPosition = transform.position;
        newRotation = transform.rotation;
    }

    void Update(){
        HandleMouseInput();
        HandleMovementInput();
    }

    void HandleMouseInput(){
        if(Input.GetMouseButtonDown(0)){
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            float entry;
            if(plane.Raycast(ray, out entry)) dragStartPosition = ray.GetPoint(entry);   
        }

        if(Input.GetMouseButton(0) && !IsPointerOverUIElement() && !MapMaker.highlightBlock.activeSelf){
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            float entry;
            if(plane.Raycast(ray, out entry)){
                dragCurrentPosition = ray.GetPoint(entry);   
                newPosition = transform.position + dragStartPosition - dragCurrentPosition;
            }
        }

        if(Input.GetMouseButtonDown(2)) rotateStartPosition = Input.mousePosition;

        if(Input.GetMouseButton(2)){
            rotateCurrentPosition = Input.mousePosition;
            Vector3 difference = rotateStartPosition - rotateCurrentPosition;
            rotateStartPosition = rotateCurrentPosition;
            newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));
        }
    }

    void HandleMovementInput(){
        if(Input.GetKey(KeyCode.LeftShift)) movementSpeed = fastSpeed;
        else movementSpeed = normalSpeed;

        // Deslocamento da câmera
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        if(vertical > 0) newPosition += (transform.forward * movementSpeed);
        if(vertical < 0) newPosition += (transform.forward * -movementSpeed);
        if(horizontal > 0) newPosition += (transform.right * movementSpeed); 
        if(horizontal < 0) newPosition += (transform.right * -movementSpeed);

        // Rotação da câmera
        if(Input.GetKey(KeyCode.Q)) newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
        if(Input.GetKey(KeyCode.E)) newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);

        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);
    }

    bool IsPointerOverUIElement(){
        var eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Where(r => r.gameObject.layer == 5).Count() > 0;
    }
}