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

	public List<GameObject> rooms;
    public GameObject closedRoom;

    public int maxRoomCount = 10;
    public int minRoomCount = 5;
	public bool refreshedRooms;

	public float waitTime;
	public bool createdRooms;

	private void Update()
	{
		// Test code
		if (Input.GetKeyDown(KeyCode.Escape)) {
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}

		if (waitTime <= 0 && createdRooms == false) {
            createdRooms = true;
            StartCoroutine(RefreshRooms());
        } else if (rooms.Count >= minRoomCount && waitTime > 0) {
			waitTime -= Time.deltaTime;
		}
	}

	private IEnumerator RefreshRooms()
	{
        for (int i = 1; i < rooms.Count; i++) {
			if (rooms[i].GetComponentInChildren<Modifyer>()) {
				rooms[i].GetComponentInChildren<Modifyer>().RefreshRoom();
			}
            yield return new WaitForSeconds(0.1f);
        }
		refreshedRooms = true;
    }
}
