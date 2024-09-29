using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomTemplates : MonoBehaviour
{
	public GameObject[] allRooms;

	public GameObject[] bottomRooms;
	public GameObject[] topRooms;
	public GameObject[] leftRooms;
	public GameObject[] rightRooms;

	public GameObject closedRoom;

	public List<GameObject> rooms;
    public int maxRoomCount = 10;
    public int minRoomCount = 5;

	public float waitTime;
	public bool spawnedBoss;
	public GameObject boss;

	private void Update()
	{
		// Test code
		if (Input.GetKeyDown(KeyCode.Escape)) {
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}

		if (waitTime <= 0 && spawnedBoss == false) {
			Instantiate(boss, rooms[^1].transform.position, Quaternion.identity);
			spawnedBoss = true;
		} else if (rooms.Count >= minRoomCount) {
			waitTime -= Time.deltaTime;
		}
	}
}
