using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class IsaacTearFactory : TearFactory
{
    public List<GameObject> tearList;

    public int[] poolSize;
    private List<GameObject>[] pool;
    private Transform[] parent;
    private int[] poolIndex;

    private void Awake()
    {
        pool = new List<GameObject>[tearList.Count];
        parent = new Transform[tearList.Count];
        poolIndex = new int[tearList.Count];
        for (int i = 0; i < tearList.Count; i++) {
            pool[i] = new List<GameObject>();
            GameObject _parent = new GameObject("Tear" + i);
            _parent.transform.position = new Vector3(1000, 1000, 0);
            parent[i] = _parent.transform;
            poolIndex[i] = default;
        }

        GameObject created = default;
        for (int i = 0; i < tearList.Count; i++) {
            for (int j = 0; j < poolSize[i]; j++) {
                created = Instantiate(tearList[i], parent[i]);
                created.SetActive(false);
                pool[i].Add(created);
            }
        }
    }

    public override GameObject GetTear(Tears _type, bool _setActive = true)
    {
        int index = (int)_type;
        GameObject selected;
        for (int i = 0; i < pool[index].Count; i++) {
            if (!pool[index][poolIndex[index]].activeSelf) {
                selected = pool[index][poolIndex[index]];
                selected.SetActive(_setActive);
                return selected;
            }
            poolIndex[index] = ++poolIndex[index] % pool[index].Count;
        }

        selected = Instantiate(tearList[index], parent[index]);
        pool[index].Add(selected);
        return selected;
    }
}
