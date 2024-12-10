using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using Photon.Pun;

public class AddRoom : MonoBehaviour
{
      private RoomTemplates templates;

      [SerializeField] private bool isClear = false;
      public bool IsClear
      {
            get { return isClear; }
            set {
                  if (isClear != value) {
                        isClear = value;
                        photonView.RPC(nameof(RPC_SetIsClear), RpcTarget.OthersBuffered, value);
                        if (isClear) {
                              //OnBoolChanged(1, 1, 1, 0.5f);
                              photonView.RPC(nameof(RPC_OnBoolChanged), RpcTarget.AllBuffered, 1f, 1f, 1f, 0.5f);
                              if(templates.RefreshedRooms) SpawnItemInRoom();
                        }
                        else {
                              //OnBoolChanged(1, 1, 1, 0.2f);
                              photonView.RPC(nameof(RPC_OnBoolChanged), RpcTarget.AllBuffered, 1f, 1f, 1f, 0.2f);
                        }
                  }
            }
      }
      [PunRPC]
      private void RPC_SetIsClear(bool value)
      {
            isClear = value;
      }

      [SerializeField] private bool currentRoom = false;
      public bool CurrentRoom
      {
            get { return currentRoom; }
            set {
                  if (currentRoom != value) {
                        currentRoom = value;
                        photonView.RPC(nameof(RPC_SetCurrentRoom), RpcTarget.OthersBuffered, value);
                        if (currentRoom) {
                              //OnBoolChanged(1, 1, 0, 0.75f);
                              photonView.RPC(nameof(RPC_OnBoolChanged), RpcTarget.AllBuffered, 1f, 1f, 0f, 0.75f);
                              if (!IsClear) {
                                    //SpawnMonsters();
                                    photonView.RPC(nameof(RPC_SpawnMonsters), RpcTarget.AllBuffered);
                              }
                        }
                        else {
                              if (IsClear) {
                                    if (IsBossRoom) {
                                          //OnBoolChanged(0, 1, 0, 0.75f);
                                          photonView.RPC(nameof(RPC_OnBoolChanged), RpcTarget.AllBuffered, 0f, 1f, 0f, 0.75f);
                                    }
                                    else {
                                          //OnBoolChanged(1, 1, 1, 0.5f);
                                          photonView.RPC(nameof(RPC_OnBoolChanged), RpcTarget.AllBuffered, 1f, 1f, 1f, 0.5f);
                                    }
                              }
                              else {
                                    //OnBoolChanged(1, 1, 1, 0.2f);
                                    photonView.RPC(nameof(RPC_OnBoolChanged), RpcTarget.AllBuffered, 1f, 1f, 1f, 0.2f);
                              }
                        }
                  }
            }
      }
      [PunRPC]
      private void RPC_SetCurrentRoom(bool value)
      {
            currentRoom = value;
      }

      [SerializeField] private bool isBossRoom = false;
      public bool IsBossRoom
      {
            get { return isBossRoom; }
            set {
                  if (isBossRoom != value) {
                        isBossRoom = value;
                        photonView.RPC(nameof(RPC_SetIsBossRoom), RpcTarget.OthersBuffered, value);
                        if (isBossRoom) {
                              //OnBoolChanged(1, 0, 0, 0.75f);
                              photonView.RPC(nameof(RPC_OnBoolChanged), RpcTarget.AllBuffered, 1f, 0f, 0f, 0.75f);
                        }
                  }
            }
      }
      [PunRPC]
      private void RPC_SetIsBossRoom(bool value)
      {
            isBossRoom = value;
      }

      [SerializeField] private int monsterCount = 0;
      public int MonsterCount
      {
            get => monsterCount;
            set {
                  monsterCount = value;
                  //Debug.LogError(monsterCount);
                  photonView.RPC(nameof(RPC_SetMonsterCount), RpcTarget.OthersBuffered, value);

                  // monsterCount가 0이 되었을 때 동작 수행
                  if (monsterCount == 0) {
                        IsClear = true;
                        //Debug.LogError(IsClear);
                  }
            }
      }
      [PunRPC]
      private void RPC_SetMonsterCount(int value)
      {
            monsterCount = value;
            //Debug.LogError(monsterCount);
      }

      private bool isSpecialRoom = false;
      public bool IsSpecialRoom
      {
            get => isSpecialRoom;
            set {
                  if (isSpecialRoom != value) {
                        isSpecialRoom = value;
                        photonView.RPC(nameof(RPC_SetIsSpecialRoom), RpcTarget.OthersBuffered, value);
                  }
            }
      }
      [PunRPC]
      private void RPC_SetIsSpecialRoom(bool value)
      {
            isSpecialRoom = value;
      }



      private int goldRoomDirection;
      public int GoldRoomDirection
      {
            get => goldRoomDirection;
            set {
                  if (goldRoomDirection != value) {
                        goldRoomDirection = value;
                        photonView.RPC(nameof(RPC_SetGoldRoomDirection), RpcTarget.OthersBuffered, value);
                  }
            }
      }
      [PunRPC]
      private void RPC_SetGoldRoomDirection(int value)
      {
            goldRoomDirection = value;
      }

      private PhotonView photonView;

      private void Awake()
      {
            templates = FindAnyObjectByType<RoomTemplates>();
            //templates = this.transform.parent.GetComponent<RoomTemplates>();

            photonView = GetComponent<PhotonView>();
      }

      private void Start()
      {
            // 마스터 클라이언트만 실행
            if (!PhotonNetwork.IsMasterClient) return;

            // RDG
            if (templates && !templates.createdRooms) templates.rooms.Add(this.gameObject);

            // Room & Door
            if (this.gameObject == templates.rooms[0]) {
                  GoldRoomDirection = UnityEngine.Random.Range(1, 5);
            }

            // Minimap & MonsterCount
            StartCoroutine(SetInitializationRoom());
      }

      private IEnumerator SetInitializationRoom()
      {
            yield return new WaitUntil(() => templates.RefreshedRooms);
            foreach (var player in PhotonNetwork.PlayerList) {
                  yield return new WaitUntil(()
                        => (player.CustomProperties.ContainsKey("SetMinimap") && (bool)player.CustomProperties["SetMinimap"]));
            }

            if (this.gameObject == templates.rooms[0]) {
                  IsClear = true;
                  CurrentRoom = true;
            }

            yield return null;

            foreach (Transform child in GetComponentsInChildren<Transform>(true)) {
                  if (child.CompareTag("Monster")) {
                        if (child.name.StartsWith("Gaper_Head")) continue;
                        else MonsterCount += 1;
                  }
            }

            if (this.gameObject == templates.rooms[^1]) {
                  IsBossRoom = true;
                  MonsterCount = 1;
                  //AddRoom bossRoom = templates.rooms[^1].GetComponent<AddRoom>();
                  //bossRoom.IsBossRoom = true;
                  //bossRoom.MonsterCount = 1;
            }
      }

      [PunRPC]
      private void RPC_OnBoolChanged(float r, float g, float b, float a)
      {
            if (!templates.RefreshedRooms) return;

            //Debug.LogError(GameManager.Instance.minimap.miniRoomsList.Count + " + " +templates.rooms.IndexOf(this.gameObject));
            GameObject miniRoom = GameManager.Instance.minimap.miniRoomsList[templates.rooms.IndexOf(this.gameObject)];
            foreach (SpriteRenderer renderer in miniRoom.GetComponentsInChildren<SpriteRenderer>()) {
                  renderer.color = new(r, g, b, a);
            }

            //foreach (Door door in GetComponentsInChildren<Door>()) {
            //      if (door.doorDirection == 0) {
            //            door.RefreshDoorAnimators();
            //            break;
            //      }
            //}
      }

      [PunRPC]
      private void RPC_SpawnMonsters()
      {
            if (isBossRoom) {
                  // 보스 몬스터 활성화
                  foreach (Transform child in GetComponentsInChildren<Transform>(true)) {
                        if (child.name.StartsWith("BossRoomSet")) {
                              string bossType = default;
                              switch (GameManager.Instance.CurrentStage) {
                                    case 1:
                                          bossType = MonsterType.Monstro.ToString();
                                          break;
                              }
                              foreach (Transform child2 in GetComponentsInChildren<Transform>(true)) {
                                    if (child2.name.Contains(bossType)) {
                                          child2.gameObject.SetActive(true);
                                          break;
                                    }
                              }
                              break;
                        }
                  }
                  // 보스 체력바 스폰
                  GameManager.Instance.uiManager.SetActiveBossSlider(true);
            }
            else if (isSpecialRoom) {
                  return;
            }
            else {
                  // 일반 몬스터 활성화
                  foreach (Transform child in GetComponentsInChildren<Transform>(true)) {
                        if (child.CompareTag("Monster")) {
                              if (!child.parent.gameObject.activeSelf) {
                                    child.parent.gameObject.SetActive(true);
                                    break;
                              }
                        }
                  }
            }
      }

      private void SpawnItemInRoom()
      {
            //GameObject item = null;
            int itemIndex = (int)ItemSpace.ItemFactory.Items.Bomb;

            // 보스 룸 클리어 보상
            if (isBossRoom) {
                  // 보스 체력바 제거
                  GameManager.Instance.uiManager.SetActiveBossSlider(false);

                  int itemCount = UnityEngine.Random.Range(2, 4);
                  while (itemCount-- != 0) {
                        itemIndex = (int)ItemSpace.ItemFactory.Items.Bomb;
                        while (itemIndex == (int)ItemSpace.ItemFactory.Items.Bomb) {
                              // 사용하는 Bomb이 아닌 Pickup 아이템이 나올 때까지 반복
                              itemIndex = UnityEngine.Random.Range(0, Enum.GetValues(typeof(ItemSpace.ItemFactory.Items)).Length);
                        }
                        //item = GameManager.Instance.itemFactory.GetItem((ItemSpace.ItemFactory.Items)itemIndex, false);
                        //if (TryGetBossScript(out MonoBehaviour script))
                        //      item.transform.position = script.transform.position; // 보스 몬스터 위치에서 생성
                        //item.SetActive(true);
                        photonView.RPC(nameof(RPC_SpawnBossClear), RpcTarget.AllBuffered, itemIndex);
                  }

                  //// exitDoor 생성
                  //Instantiate(templates.exitDoor, transform.position + Vector3.up * 1.5f, Quaternion.identity, transform);
                  //// prop 생성
                  //Instantiate(templates.prop, transform.position + Vector3.down * 1.5f, Quaternion.identity, transform);
                  photonView.RPC(nameof(RPC_SpawnClearExit), RpcTarget.AllBuffered);
            }
            // 2분의 1 확률로 룸 클리어 보상
            else if (UnityEngine.Random.Range(0, 2) == 0) {
                  while (itemIndex == (int)ItemSpace.ItemFactory.Items.Bomb) {
                        itemIndex = UnityEngine.Random.Range(0, Enum.GetValues(typeof(ItemSpace.ItemFactory.Items)).Length);
                  }
                  //item = GameManager.Instance.itemFactory.GetItem((ItemSpace.ItemFactory.Items)itemIndex, false);
                  //item.transform.position = transform.position;
                  //item.SetActive(true);
                  photonView.RPC(nameof(RPC_SpawnRoomClear), RpcTarget.AllBuffered, itemIndex);
            }
      }
      [PunRPC]
      private void RPC_SpawnBossClear(int itemIndex)
      {
            GameObject item = GameManager.Instance.itemFactory.GetItem((ItemSpace.ItemFactory.Items)itemIndex, false);
            if (TryGetBossScript(out MonoBehaviour script))
                  item.transform.position = script.transform.position; // 보스 몬스터 위치에서 생성
            item.SetActive(true);
      }
      [PunRPC]
      private void RPC_SpawnClearExit()
      {
            // exitDoor 생성
            Instantiate(templates.exitDoor, transform.position + Vector3.up * 1.5f, Quaternion.identity, transform);
            // prop 생성
            Instantiate(templates.prop, transform.position + Vector3.down * 1.5f, Quaternion.identity, transform);
      }
      [PunRPC]
      private void RPC_SpawnRoomClear(int itemIndex)
      {
            GameObject item = GameManager.Instance.itemFactory.GetItem((ItemSpace.ItemFactory.Items)itemIndex, false);
            item.transform.position = transform.position;
            item.SetActive(true);
      }

      private bool TryGetBossScript(out MonoBehaviour script)
      {
            // 초기화
            script = null;
            
            MonoBehaviour[] scripts = GetComponentsInChildren<MonoBehaviour>();
            foreach (MonoBehaviour s in scripts) {
                  Type baseType = s.GetType()?.BaseType; // 부모: Monster
                  if (baseType != null && baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(Monster<>)) {
                        script = s;
                        return true;
                  }
            }
            return false;
      }


      private void OnDestroy()
      {
            //Debug.LogError(gameObject.name + " + " + photonView.ViewID);
      }
}
