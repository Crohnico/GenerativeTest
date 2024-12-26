using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WFCPixel
{
    private List<float> possibleValues = new List<float>();
    public bool collapsed = false;
    public float finalValue;

    private float minDif;
    private float maxDif;


    public WFCPixel(float maxDif, float minDif)
    {
        this.minDif = minDif;
        this.maxDif = maxDif;

        possibleValues = new List<float>();
        float parts = 1 / minDif;
        int divisions = Mathf.RoundToInt(parts);

        for (float i = 0f; i <= divisions; i += 1)
        {
            possibleValues.Add(i * minDif);
        }

    }

    public int Entropy => collapsed ? 0 : possibleValues.Count;


    public float MinValue => GetMin();
    public float MaxValue => GetMax();

    public void UpdatePossibles(float minNeightbourValue, float maxNeightbourValue)
    {
        List<float> newPossibles = new List<float>();

        float minLimit = Mathf.Max(0f, minNeightbourValue - maxDif);
        float maxLimit = Mathf.Min(1f, maxNeightbourValue + maxDif);

        minLimit = Mathf.Round(minLimit * 1000f) / 1000f;
        maxLimit = Mathf.Round(maxLimit * 1000f) / 1000f;

        for (float i = minLimit; i <= maxLimit; i += minDif)
        {
            i = Mathf.Round(i * 1000f) / 1000f;

            if (possibleValues.Contains(i))
            {
                newPossibles.Add(i);
            }
        }

        possibleValues = new List<float>(newPossibles);

        if (possibleValues.Count == 0)
        {
            Debug.LogError("No possible values remain! Something went wrong.");
        }

        if (possibleValues.Count == 1)
        {
            CollapseTo(possibleValues[0]);
        }
    }

    public void CollapseTo(float value)
    {
        if (possibleValues.Count == 0)
        {
            Debug.LogError("Attempting to collapse with no possible values.");
            return;
        }

        if (!possibleValues.Contains(value))
        {
            value = (Mathf.Abs(MinValue - value) < Mathf.Abs(MaxValue - value)) ? MinValue : MaxValue;
        }


        collapsed = true;
        finalValue = value;

        possibleValues.Clear();
        possibleValues.Add(value);
    }

    public void Collapse() 
    {
        float value = possibleValues[Random.Range(0, possibleValues.Count)];

        collapsed = true;
        finalValue = value;

        possibleValues.Clear();
        possibleValues.Add(value);
    }

    public float GetAverageValue()
    {
        if (collapsed) return finalValue;
        return possibleValues.Average();
    }

    public float GetMin() 
    {
        return possibleValues.Min();
    }

    public float GetMax()
    {
        return possibleValues.Max();
    }

}
