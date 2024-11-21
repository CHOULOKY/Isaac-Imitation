using System.Collections.Generic;
using UnityEngine;

public class MonsterTearFactory : TearFactory
{
      protected override void Awake()
      {
            pool = new List<GameObject>[tearList.Count];
            parent = new Transform[tearList.Count];
            poolIndex = new int[tearList.Count];
            for (int i = 0; i < tearList.Count; i++) {
                  pool[i] = new List<GameObject>();
                  GameObject _parent = new GameObject(tearList[i].name);
                  _parent.transform.position = new Vector3(2000, 1000, 0);
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
}
