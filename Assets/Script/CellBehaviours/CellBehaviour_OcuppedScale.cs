using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellBehaviour_OcuppedScale : MonoBehaviour
{
    private Tweener scaleModifation;
    Sequence sequence ;
    private CellBehaviour cellBehaviour;

    public GameObject prop;
    private Vector3 initScale;

    private void OnEnable()
    {
        initScale = prop.transform.localScale;
        if (TryGetComponent<CellBehaviour>(out cellBehaviour))
        {
            cellBehaviour.OnCellIn += OnCellIn;
            cellBehaviour.OnCellOut += OnCellOut;
        }
    }

    private void OnCellIn() 
    {
        scaleModifation?.Kill();
        scaleModifation = prop.transform.DOScale(Vector3.zero, .1f);
    }
    private void OnCellOut() 
    {
        scaleModifation?.Kill();
        float time = Random.Range(1f, 3f);
        scaleModifation = prop.transform.DOScale(initScale, time);
    }

    private void OnDisable()
    {
        Unsuscribe();
        scaleModifation?.Kill();
        sequence?.Kill();
    }

    private void OnDestroy()
    {
        Unsuscribe();
        scaleModifation?.Kill();
        sequence?.Kill();
    }
    private  void Unsuscribe() 
    {
        cellBehaviour.OnCellIn  -= OnCellIn;
        cellBehaviour.OnCellOut -= OnCellOut;
    }
}
