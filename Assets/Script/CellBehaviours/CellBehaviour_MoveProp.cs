using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellBehaviour_MoveProp : MonoBehaviour
{
    private CellBehaviour cellBehaviour;
    public GameObject prop;


    private void OnEnable()
    {
        if (TryGetComponent<CellBehaviour>(out cellBehaviour))
        {
            float zOffset = Random.Range(-0.02f, 0.04f);
            prop.transform.localPosition += Vector3.forward * zOffset;
        }
    }


    private void OnDisable()
    {
        Unsuscribe();

    }

    private void OnDestroy()
    {

    }

    private void Unsuscribe()
    {

    }
}
