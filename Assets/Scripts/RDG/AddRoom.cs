using System.Collections;
using UnityEngine;

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
                        if (isClear) {
                              OnBoolChanged(1, 1, 1, 0.5f);
                        }
                        else {
                              OnBoolChanged(1, 1, 1, 0.2f);
                        }
                  }
            }
      }

      [SerializeField] private bool currentRoom = false;
      public bool CurrentRoom
      {
            get { return currentRoom; }
            set {
                  if (currentRoom != value) {
                        currentRoom = value;
                        if (currentRoom) {
                              OnBoolChanged(1, 1, 0, 0.75f);
                              if (!IsClear) {
                                    SpawnMonsters();
                              }
                        }
                        else {
                              if (IsClear) {
                                    if (IsBossRoom) OnBoolChanged(0, 1, 0, 0.75f);
                                    else OnBoolChanged(1, 1, 1, 0.5f);
                              }
                              else {
                                    OnBoolChanged(1, 1, 1, 0.2f);
                              }
                        }
                  }
            }
      }

      [SerializeField] private bool isBossRoom = false;
      public bool IsBossRoom
      {
            get { return isBossRoom; }
            set {
                  if (isBossRoom != value) {
                        isBossRoom = value;
                        if (isBossRoom) {
                              OnBoolChanged(1, 0, 0, 0.75f);
                        }
                  }
            }
      }

      private void Start()
      {
            templates = this.transform.parent.GetComponent<RoomTemplates>();
            if (templates && !templates.createdRooms) templates.rooms.Add(this.gameObject);
            if (this.gameObject == templates.rooms[0]) StartCoroutine(SetInitialRoomCoroutine());
      }

      private IEnumerator SetInitialRoomCoroutine()
      {
            yield return new WaitUntil(() => templates.refreshedRooms);
            yield return null;

            IsClear = true;
            CurrentRoom = true;

            yield return null;

            AddRoom bossRoom = templates.rooms[^1].GetComponent<AddRoom>();
            bossRoom.IsBossRoom = true;
      }

      private void OnBoolChanged(float r, float g, float b, float a)
      {
            if (!templates.refreshedRooms) return;

            GameObject miniRoom = GameManager.Instance.minimap.miniRoomsList[templates.rooms.IndexOf(this.gameObject)];
            foreach (SpriteRenderer renderer in miniRoom.GetComponentsInChildren<SpriteRenderer>()) {
                  renderer.color = new(r, g, b, a);
            }
      }

      private void SpawnMonsters()
      {
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
