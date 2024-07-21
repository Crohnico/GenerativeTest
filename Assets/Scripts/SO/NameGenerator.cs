using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class NameGenerator
{
    private Dictionary<char, int> consonantWeights = new Dictionary<char, int>()
  {
        { 'b', 2 },
        { 'c', 3 },
        { 'd', 4 },
        { 'f', 2 },
        { 'g', 3 },
        { 'h', 2 },
        { 'j', 1 },
        { 'k', 1 },
        { 'l', 7 },
        { 'm', 4 },
        { 'n', 5 },
        { 'p', 3 },
        { 'q', 1 },
        { 'r', 5 },
        { 's', 5 },
        { 't', 6 },
        { 'v', 1 },
        { 'w', 1 },
        { 'x', 1 },
        { 'y', 2 },
        { 'z', 1 }
    };


    private List<string> consonants = new List<string>();
    private List<string> vowels = new List<string>() { "a", "e", "i", "o", "u" };

    private void SetUpList()
    {
        consonants.Clear();
        vowels = new List<string>() { "a", "e", "i", "o", "u" };

        foreach (var value in consonantWeights)
        {
            for (int i = 0; i < value.Value; i++)
            {
                consonants.Add(value.Key.ToString());
            }
        }
    }


    public string GenerateProceduralName(int syllableCount)
    {
        //  Random.InitState(Seed.seed);
        SetUpList();

        if (syllableCount < 1 || syllableCount > 5)
        {
            return "paiaso";
        }

        string name = "";
        for (int i = 0; i < syllableCount; i++)
        {

            bool hasDoubleConsonant = (syllableCount == 1) ? true : Random.value < 0.5f;

            string randomConsonant1 = consonants[Random.Range(0, consonants.Count)];
            string randomVowel = vowels[Random.Range(0, vowels.Count)];


            if (hasDoubleConsonant)
            {

                string randomConsonant2 = consonants[Random.Range(0, consonants.Count)];
                name += randomConsonant1 + randomVowel + randomConsonant2;
            }
            else
            {
                name += randomConsonant1 + randomVowel;
            }
        }

        name = char.ToUpper(name[0]) + name.Substring(1);

        return name;
    }
}

