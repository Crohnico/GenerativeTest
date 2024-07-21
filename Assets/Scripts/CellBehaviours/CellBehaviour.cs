using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellBehaviour : MonoBehaviour
{
    public Action OnCellIn;
    public Action OnCellOut;

    
    public IACell cellReference;


    public void SetUp(IACell cell) 
    {
        cell.OnCellIn += CellIn;
        cell.OnCellOut += CellOut;
    }

    public virtual void CellIn() 
    {
        OnCellIn?.Invoke();
    }

    public virtual void CellOut()
    {
        OnCellOut?.Invoke();
    }
    private void OnDestroy()
    {
        if (cellReference == null) return;

        cellReference.OnCellIn  -= CellIn;
        cellReference.OnCellOut -= CellOut;
    }
}
