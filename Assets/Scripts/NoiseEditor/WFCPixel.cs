using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WFCPixel
{
    private List<int> possibleValues = new List<int>();
    public bool collapsed = false;

    public float finalValue => (float)collapsedValue * minDif;
    public int collapsedValue;
    public int divisions;

    private float minDif;
    private int maxDif; //How many minDif 

    private Dictionary<float, int> savedFloats = new Dictionary<float, int>();

    public WFCPixel(int maxDif, float minDif)
    {
        this.minDif = minDif;
        this.maxDif = maxDif; 

        possibleValues = new List<int>();

        float parts = 1f / minDif;
        divisions = Mathf.RoundToInt(parts);

        for (int i = 0; i <= divisions; i++)
        {
            possibleValues.Add(i);
            savedFloats.Add(i * minDif, i);
        }

    }

    public int Entropy => collapsed ? 0 : possibleValues.Count;

    public void UpdatePossibles(int minNeightbourValue, int maxNeightbourValue)
    {
        List<int> newPossibles = new List<int>();

        int minLimit = minNeightbourValue - maxDif;
        int maxLimit = maxNeightbourValue + maxDif;

        for (int i = minLimit; i <= maxLimit; i ++)
        {
            if (possibleValues.Contains(i))
            {
                newPossibles.Add(i);
            }
        }

        if (newPossibles.Count == 0)
        {

            int min = MinValue();
            int max = MaxValue();

            if (max - maxNeightbourValue < min - minNeightbourValue)
            {
                float newMax = (max + maxNeightbourValue) / 2;
                newPossibles.Add(Mathf.RoundToInt(newMax));
            }
            else
            {
                float newMin = (min + minNeightbourValue) / 2;
                newPossibles.Add(Mathf.RoundToInt(newMin));
            }

        }

        possibleValues = new List<int>(newPossibles);

        if (possibleValues.Count == 1)
        {
            CollapseTo(intValue: possibleValues[0]);
        }
    }

    public void CollapseTo(float floatValue = -1,int intValue = -1, bool forceCollapse = false)
    {
        int parsedValue = (floatValue > 0) ? savedFloats[floatValue] : intValue;

        if (possibleValues.Count == 0)
        {
            Debug.LogError("Attempting to collapse with no possible values.");
            return;
        }

        if (!forceCollapse)
        {
            if (!possibleValues.Contains(parsedValue))
            {
                int max = possibleValues.Max();
                int min = possibleValues.Min();
                parsedValue = (Mathf.Abs(min - parsedValue) < Mathf.Abs(max - parsedValue)) ? min : max;
            }
        }

        collapsed = true;
        collapsedValue = parsedValue;

        possibleValues.Clear();
        possibleValues.Add(parsedValue);
    }

    public void Collapse() 
    {
        int value = possibleValues[Random.Range(0, possibleValues.Count)];

        collapsed = true;
        collapsedValue = value;

        possibleValues.Clear();
        possibleValues.Add(value);
    }

    public float GetAverageValue()
    {
        if (collapsed) return finalValue;
        if (possibleValues.Count == 0) return 0;

        return (float)(possibleValues.Average() * minDif);
    }

    public int MinValue() 
    {
        if (collapsed) return collapsedValue;

        return possibleValues.Min();
    }

    public int MaxValue() 
    {
        if (collapsed) return collapsedValue;

        return possibleValues.Max();
    }
}
