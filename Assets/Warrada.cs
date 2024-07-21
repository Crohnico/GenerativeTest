using PlayerX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warrada : MonoBehaviour
{
    public PX_Camera cameraScript;
    public GameObject player;
    public void Initialize()
    {  
        transform.parent.transform.position = new Vector3(1500 / 2, (100 + 100 / 2) + 10, 1500 / 2);

        cameraScript.followCamera = new Camera();
    }

}
