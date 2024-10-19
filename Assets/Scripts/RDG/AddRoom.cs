using System.Collections;
using System.Collections.Generic;
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
                OnClearBoolChanged(isClear);
            }
        }
    }

    private void Start()
	{
        templates = this.transform.parent.GetComponent<RoomTemplates>();
        if (templates && !templates.createdRooms) templates.rooms.Add(this.gameObject);
        if (this.gameObject == templates.rooms[0]) StartCoroutine(FirstRoomCoroutine(3f));
    }
    private IEnumerator FirstRoomCoroutine(float _duration = 3f)
    {
        yield return new WaitForSeconds(_duration);

        IsClear = true;
    }

    private void OnClearBoolChanged(bool _isClear)
    {
        if (!templates.refreshedRooms) return;

        GameObject miniRoom = GameManager.Instance.minimap.miniRoomsList[templates.rooms.IndexOf(this.gameObject)];
        if (_isClear) {
            foreach (SpriteRenderer renderer in miniRoom.GetComponentsInChildren<SpriteRenderer>()) {
                renderer.color = new(1, 1, 1, 0.5f);
            }
        }
        else {
            foreach (SpriteRenderer renderer in miniRoom.GetComponentsInChildren<SpriteRenderer>()) {
                renderer.color = new(1, 1, 1, 0.2f);
            }
        }
    }
}
