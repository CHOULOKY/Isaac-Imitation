using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    public GameObject[] miniRooms;
    private List<GameObject> miniRoomsList;
    public GameObject boss;

    public RoomTemplates templates;
    private bool isSetMinimap;

    private void Awake()
    {
        miniRoomsList = new List<GameObject>();

        if (!templates) {
            templates = FindAnyObjectByType<RoomTemplates>();
        }
    }

    private void Update()
    {
        if (templates.refreshedRooms && !isSetMinimap) {
            isSetMinimap = true;

            for (int i = 0; i < templates.rooms.Count; i++) {
                string roomName = templates.rooms[i].name.Replace("(Clone)", "");
                for (int j = 0; j < miniRooms.Length; j++) {
                    if (roomName == miniRooms[j].name) {
                        float roomX = this.transform.position.x + templates.rooms[i].transform.position.x / 40 * 0.5f;
                        float roomY = this.transform.position.y + templates.rooms[i].transform.position.y / 40 * 0.5f;
                        miniRoomsList.Add(Instantiate(miniRooms[j], new Vector2(roomX, roomY), miniRooms[j].transform.rotation, this.transform));
                        break;
                    }
                }
            }
            Instantiate(boss, miniRoomsList[^1].transform.position, Quaternion.identity);
        }
    }
}
