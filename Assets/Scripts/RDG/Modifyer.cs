using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Modifyer : MonoBehaviour
{
      private RoomTemplates templates;

      private List<int> directionList;

      private void Awake()
      {
            templates = this.transform.parent.parent.GetComponent<RoomTemplates>();

            directionList = new List<int>();
      }

      private void OnTriggerEnter2D(Collider2D other)
      {
            if (templates.createdRooms) return;

            if (other.CompareTag("SpawnPoint")) {
                  directionList.Add(other.GetComponent<RoomSpawner>().openingDirection);
            }
      }

      public void RefreshRoom()
      {
            string thisName = this.transform.parent.name;
            if (!thisName.Contains("Closed")) {
                  if (thisName.Contains('B')) directionList.Add(1);
                  if (thisName.Contains('T')) directionList.Add(2);
                  if (thisName.Contains('L')) directionList.Add(3);
                  if (thisName.Contains('R')) directionList.Add(4);
                  directionList = directionList.Distinct().OrderBy(n => n).ToList(); // 중복 제거 & 오름차순 정렬
            }

            char dirB = default;
            string roomName = "";
            foreach (int direction in directionList) {
                  switch (direction) {
                        case 1:
                              dirB = 'B';
                              break;
                        case 2:
                              roomName += "T";
                              break;
                        case 3:
                              roomName += "L";
                              break;
                        case 4:
                              roomName += "R";
                              break;
                  }
            }
            roomName += dirB == default ? "" : dirB;

            for (int i = 0; i < templates.allRooms.Length; i++) {
                  if (templates.allRooms[i].name.Equals(roomName)) {
                        templates.rooms[templates.rooms.IndexOf(this.transform.parent.gameObject)] =
                            Instantiate(templates.allRooms[i], transform.position, templates.allRooms[i].transform.rotation, templates.transform);
                        Destroy(this.transform.parent.gameObject);
                        break;
                  }
            }
      }

      public void SetSpecialRoom(RoomType roomType)
      {
            AddRoom thisRoom = GetComponentInParent<AddRoom>();

            switch (roomType) {
                  case RoomType.Gold:
                        thisRoom.IsClear = true;
                        break;
            }

            // transform.parent.transform = Current Room's Transform
            foreach (Transform target in transform.parent.GetComponentsInChildren<Transform>(true)) {
                  if (target.name == "Monsters" || target.name == "Obstacles") {
                        Destroy(target.gameObject);
                  }
            }
      }
}
