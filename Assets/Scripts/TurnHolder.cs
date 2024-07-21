using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnHolder : MonoBehaviour
{
    public static Action resolveTurn;

    public static TurnHolder istance;
    public TurnHolder Istance => GetIstance();

    private  TurnHolder GetIstance()
    {
        if (istance == null)
        {
            istance = this;
            return istance;
        }
        else
        { 
            Destroy(this.gameObject);
            return istance;
        }
    }
}
