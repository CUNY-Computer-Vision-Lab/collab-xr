using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadAddTrack : MonoBehaviour {

    public GameObject head;


    // Use this for initialization
    void Start() {
        this.transform.position = new Vector3(0, 0, 0);

        GameObject cameraMain = GameObject.FindGameObjectWithTag("MainCamera");

        if(cameraMain != null)
            this.gameObject.transform.parent = cameraMain.transform;
        else {
            Debug.LogError("Cant find main camera");
        }

        //head = GlobalSceneManager.mainCamera.gameObject;
        //this.gameObject.transform.SetParent(head.transform);

        ////update faster
        //try {
        //    this.gameObject.GetComponent<TransferObject>().updateRate = .1f;
        //}
        //catch (System.Exception) {
        //}
    }

    // Update is called once per frame
    void Update() {
        //if(head != null) {
        //    //this.gameObject.transform.position = head.transform.position;
        //    //this.gameObject.transform.rotation = head.transform.rotation;
        //}
        //else {
        //    Debug.LogError("Main camera is null");
        //}
    }
}
