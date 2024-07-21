using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropManager : MonoBehaviour
{
    public static PropManager Instance;
    public GameObject generator;

    private void Awake()
    {
        Instance = this;
    }

    public void GetTrees(int amount, GameObject floor,TreeData data, out GameObject[] result)
    {
        List<GameObject> list = new List<GameObject> ();

        /*for (int i = 0; i < amount; i++)
        {
            GameObject go = new GameObject();
            go.transform.parent = transform;

            GameObject tree = Instantiate(generator, go.transform);
            tree.transform.rotation = Quaternion.Euler(90, 0, 0);
            list.Add(go);

            Instantiate(floor, go.transform);
            go.AddComponent<CellBehaviour>();

            if (tree.TryGetComponent(out TreeGenerator treeGenerator))
            {
                Debug.Log(data.branchDensity);
                treeGenerator.Init(data);
            }
        }*/

        result = list.ToArray();
    }

}
