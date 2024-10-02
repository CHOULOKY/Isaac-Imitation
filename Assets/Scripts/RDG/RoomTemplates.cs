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

	[SerializeField] public List<GameObject> rooms;
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
            StartCoroutine(RefreshRooms());
        } else if (rooms.Count >= minRoomCount && waitTime > 0) {
			waitTime -= Time.deltaTime;
		}
	}

	private IEnumerator RefreshRooms()
	{
        for (int i = 0; i < rooms.Count; i++) {
			if (rooms[i].GetComponentInChildren<Modifyer>()) {
				rooms[i].GetComponentInChildren<Modifyer>().RefreshRoom();
			}
            yield return new WaitForSeconds(0.1f);
        }
    }
}
