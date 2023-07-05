using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    public GameObject FollowObject, nextObject;
    private const float Z = -6.840625f;
    public Vector3 posModificated;
    public bool moved = false, keepFollowing = true;
    private Vector3 positionPreMoved;

    void Start()
    {
        //print("Decomentar Update cuando CPU implementada, tb separar MainCamera de Player");
    }
    
    void Update()
    {
        //camera didnt move and focused on an object
        if (!moved && keepFollowing)
            transform.position = new Vector3(FollowObject.transform.position.x - posModificated.x, FollowObject.transform.position.y - posModificated.y, Z);
        //else
        //    transform.position = new Vector3(positionPreMoved.x - posModificated.x, positionPreMoved.y - posModificated.y, Z);

    }

    public void Center()
    {
        //Current object as a center
        posModificated = Vector3.zero;
        StartCoroutine(SetActiveButton(false));
    }

    internal void ModifyCoords(Vector3 mod)
    {
        CheckPositionPreMoved();
        posModificated = mod;
        checkMoved();
    }

    private void CheckPositionPreMoved()
    {
        //If camera moved, wont follow current object
        if (posModificated.y == 0 && posModificated.x == 0)
        {
            positionPreMoved = transform.position;
        }
    }

    private void checkMoved()
    {
        //Check if camera is focused on the object
        if (posModificated.x != 0 || posModificated.y != 0)
        {
            StartCoroutine(SetActiveButton(true));
        }
        else
        {
            StartCoroutine(SetActiveButton(false));
        }
    }

    internal void ChangeFollowObject(Vector3 position)
    {
        //Change centered position object
        if (!moved)
            positionPreMoved = position;
        keepFollowing = false;
        StartCoroutine(ChangeObject());
    }

    private IEnumerator ChangeObject()
    {
        //Change centered position object and turns
        yield return new WaitForSeconds(5f);
        FollowObject = nextObject;
        Center();
        keepFollowing = true;
        //print("Descomentar cuando CPU implementada");
        
        if (GameManager.instance.GetComponent<GameManager>().turn >= 4 && GameManager.instance.GetComponent<GameManager>().turn <= 6)
            GameManager.instance.GetComponent<GameManager>().turn = 1;
        else
            GameManager.instance.GetComponent<GameManager>().turn++;
        
        yield break;
    }

    private IEnumerator SetActiveButton(bool active)
    {
        //Activate Centre button
        yield return new WaitForEndOfFrame();
        GameManager.instance.camButton.gameObject.SetActive(active);
        moved = active;
        yield break;
    }

    internal void ChangeFollowObject(GameObject go, GameObject nextObject)
    {
        //Change focused object
        GameManager.instance.GetComponent<GameManager>().turn++;
        FollowObject = go;
        this.nextObject = nextObject;
    }

}
