using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddRoom : MonoBehaviour
{
	private RoomTemplates templates;

	private void Start()
	{
        templates = this.transform.parent.GetComponent<RoomTemplates>();
        if (templates && !templates.spawnedBoss) templates.rooms.Add(this.gameObject);
    }
}
