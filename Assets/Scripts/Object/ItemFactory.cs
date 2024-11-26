using System.Collections.Generic;
using UnityEngine;

namespace ItemSpace
{
      public class ItemFactory : MonoBehaviour
      {
            public enum Items { Bomb, BombPickup, Heart }

            [SerializeField] private List<GameObject> itemList;

            [SerializeField] private int[] poolSize;
            private List<GameObject>[] pool;
            private Transform[] parent;
            private int[] poolIndex;


            private void Awake()
            {
                  pool = new List<GameObject>[itemList.Count];
                  parent = new Transform[itemList.Count];
                  poolIndex = new int[itemList.Count];
                  for (int i = 0; i < itemList.Count; i++) {
                        pool[i] = new List<GameObject>();
                        GameObject _parent = new GameObject(itemList[i].name);
                        _parent.transform.position = new Vector3(-1000, -1000, 0);
                        parent[i] = _parent.transform;
                        poolIndex[i] = default;
                  }

                  GameObject created = default;
                  for (int i = 0; i < itemList.Count; i++) {
                        for (int j = 0; j < poolSize[i]; j++) {
                              created = Instantiate(itemList[i], parent[i]);
                              created.SetActive(false);
                              pool[i].Add(created);
                        }
                  }
            }

            public GameObject GetItem(Items _type, bool _setActive = true)
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

                  selected = Instantiate(itemList[index], parent[index]);
                  pool[index].Add(selected);
                  return selected;
            }
      }
}
