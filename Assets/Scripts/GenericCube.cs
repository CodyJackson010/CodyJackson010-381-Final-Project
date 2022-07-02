using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericCube : MonoBehaviour
{
    public float x;
    public float y;

    public GameObject cubeRef;

    //------- Materials ---------//
    public Material debugMat;
    //public Material x

    //---------------------------//

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMaterial(Material newMat)
    {
        cubeRef.GetComponent<MeshRenderer>().material = newMat;
    }
}
