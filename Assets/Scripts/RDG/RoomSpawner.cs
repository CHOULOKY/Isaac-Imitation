using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
	public int openingDirection;
	// 1 --> need bottom door
	// 2 --> need top door
	// 3 --> need left door
	// 4 --> need right door


	private RoomTemplates templates;
	private int rand;
	public bool spawned = false;

	public float waitTime = 4f;

    private void Awake()
    {
        templates = this.transform.parent.parent.parent.GetComponent<RoomTemplates>();
    }

    private void Start()
	{
		Destroy(gameObject, waitTime);
		Invoke(nameof(Spawn), 0.1f);
	}


	private void Spawn()
	{
        if (templates.createdRooms) return;
        
		if (spawned == false) {
			int randStart = templates.rooms.Count < templates.minRoomCount ? 1 : 0;
            if (openingDirection == 1) {
				// Need to spawn a room with a BOTTOM door.
				rand = Random.Range(randStart, templates.bottomRooms.Length);
				rand = templates.rooms.Count > templates.maxRoomCount ? 0 : rand;
                Instantiate(templates.bottomRooms[rand], transform.position, templates.bottomRooms[rand].transform.rotation, templates.transform);
			} else if (openingDirection == 2) {
				// Need to spawn a room with a TOP door.
				rand = Random.Range(randStart, templates.topRooms.Length);
                rand = templates.rooms.Count > templates.maxRoomCount ? 0 : rand;
                Instantiate(templates.topRooms[rand], transform.position, templates.topRooms[rand].transform.rotation, templates.transform);
			} else if (openingDirection == 3) {
				// Need to spawn a room with a LEFT door.
				rand = Random.Range(randStart, templates.leftRooms.Length);
                rand = templates.rooms.Count > templates.maxRoomCount ? 0 : rand;
                Instantiate(templates.leftRooms[rand], transform.position, templates.leftRooms[rand].transform.rotation, templates.transform);
			} else if (openingDirection == 4) {
				// Need to spawn a room with a RIGHT door.
				rand = Random.Range(randStart, templates.rightRooms.Length);
                rand = templates.rooms.Count > templates.maxRoomCount ? 0 : rand;
                Instantiate(templates.rightRooms[rand], transform.position, templates.rightRooms[rand].transform.rotation, templates.transform);
			}
			spawned = true;
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (templates.createdRooms) return;

		if (other.CompareTag("SpawnPoint")) {
			if (other.GetComponent<RoomSpawner>().spawned == false && this.spawned == false) {
				Instantiate(templates.closedRoom, transform.position, Quaternion.identity, templates.transform);
				Destroy(this.gameObject);
			}
			spawned = true;
		}
	}
}
