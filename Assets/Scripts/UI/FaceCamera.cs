using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public GameObject mainCam;

    // Start is called before the first frame update
    void Start()
    {
        mainCam = UIManager.inst.mainCamera;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(2 * gameObject.transform.position - mainCam.transform.position);
    }
}
