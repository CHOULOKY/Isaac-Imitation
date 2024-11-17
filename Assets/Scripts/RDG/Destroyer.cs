using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Room") && other.transform != this.transform.parent) {
            other.transform.parent.GetComponent<RoomTemplates>().rooms.Remove(other.gameObject);
			Destroy(other.gameObject);
		}
	}
}
