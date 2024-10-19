using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public RoomTemplates templates;

    private AddRoom thisRoom;
    public Animator[] doorAnimators;

    private GameObject bossDoor;

    public int doorDirection;
    // 0 --> center
    // 1 --> top : -3
    // 2 --> bottom : +3
    // 3 --> right : -5.4
    // 4 --> left : +5.4

    private Vector2 nextRoomPos = Vector2.zero;
    private BoxCollider2D nextRoomCollider = null;

    public IsaacBody isaac;

    private MainCamera mainCamera;
    public Vector2 maxBoundaryFromCenter;
    public Vector2 minBoundaryFromCenter;

    private void Awake()
    {
        templates = transform.parent.parent.GetComponent<RoomTemplates>();
        templates = templates != null ? templates : transform.parent.parent.parent.GetComponent<RoomTemplates>();

        thisRoom = transform.parent.GetComponent<AddRoom>();
        thisRoom = thisRoom != null ? thisRoom : transform.parent.parent.GetComponent<AddRoom>();

        if (doorDirection == 0) {
            doorAnimators = GetComponentsInChildren<Animator>();
            isaac = isaac != null ? isaac : FindAnyObjectByType<IsaacBody>();
            foreach (Transform _transform in transform) {
                if (_transform.name == "BossDoor") {
                    bossDoor = _transform.gameObject;
                    if (bossDoor) {
                        foreach (Door door in GetComponentsInChildren<Door>()) {
                            door.bossDoor = bossDoor;
                        }
                    }
                    break;
                }
            }
        }
    }

    private void Start()
    {
        if (doorDirection == 0) {
            foreach (Door door in GetComponentsInChildren<Door>()) {
                door.isaac = door.isaac != null ? door.isaac : isaac;
                if (thisRoom.isClear) {
                    DoorAnimatorsPlay("Open");
                }
            }
        }
    }

    private bool isDoorOpen = false, spawnedBossDoor = false;
    private void Update()
    {
        if (doorDirection == 0) {
            if (templates.refreshedRooms && templates.rooms[^1] == thisRoom && !spawnedBossDoor) {
                spawnedBossDoor = true;
                doorAnimators[0].gameObject.SetActive(false);
                
            }

            if (thisRoom.isClear && !isDoorOpen) {
                isDoorOpen = true;
                DoorAnimatorsPlay("Open");
            }
            else if (!thisRoom.isClear && isDoorOpen) {
                isDoorOpen = false;
                DoorAnimatorsPlay("Close");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!templates.refreshedRooms) return;

        if (collision.CompareTag("Player") && thisRoom.isClear && doorDirection != 0) {
            isaac = isaac != null ? isaac : collision.GetComponent<IsaacBody>();

            if (nextRoomPos == Vector2.zero) {
                switch (doorDirection) {
                    case 1:
                        nextRoomPos.y += 40 - (transform.position.y - thisRoom.transform.position.y);
                        break;
                    case 2:
                        nextRoomPos.y += 40 - (thisRoom.transform.position.y - transform.position.y);
                        break;
                    case 3:
                        nextRoomPos.y += 40 - (transform.position.x - thisRoom.transform.position.x);
                        break;
                    case 4:
                        nextRoomPos.y += 40 - (thisRoom.transform.position.x - transform.position.x);
                        break;
                }
            }

            StartCoroutine(CreateAndDisableCollider(nextRoomPos));
        }
        else if (collision.CompareTag("Door") && doorDirection == 0) {
            int beforeDirection = collision.GetComponent<Door>().doorDirection;
            int needThisDoor = default;
            switch (beforeDirection) {
                case 1:
                    needThisDoor = 2;
                    break;
                case 2:
                    needThisDoor = 1;
                    break;
                case 3:
                    needThisDoor = 4;
                    break;
                case 4:
                    needThisDoor = 3;
                    break;
            }

            foreach (Door door in GetComponentsInChildren<Door>()) {
                if (door.doorDirection == needThisDoor) {
                    Vector2 nextIsaacPos = door.transform.position;
                    switch (door.doorDirection) {
                        case 1:
                            nextIsaacPos.y += -1;
                            break;
                        case 2:
                            nextIsaacPos.y += 1;
                            break;
                        case 3:
                            nextIsaacPos.x += -1;
                            break;
                        case 4:
                            nextIsaacPos.x += 1;
                            break;
                    }
                    isaac.GetComponent<Rigidbody2D>().position = nextIsaacPos;
                    
                    SetBoundaryForCamera();
                    // break;
                }
            }


        }
    }

    private IEnumerator CreateAndDisableCollider(Vector2 _nextRoomPos)
    {
        if (nextRoomCollider == null) {
            nextRoomCollider = gameObject.AddComponent<BoxCollider2D>();

            nextRoomCollider.isTrigger = true;
            nextRoomCollider.offset = _nextRoomPos;
            nextRoomCollider.size = Vector2.one;
        }

        nextRoomCollider.enabled = true;
        
        yield return new WaitForSeconds(0.25f);

        nextRoomCollider.enabled = false;
    }

    private void SetBoundaryForCamera()
    {
        mainCamera = mainCamera != null ? mainCamera : Camera.main.GetComponent<MainCamera>();

        mainCamera.maxBoundary = (Vector2)transform.position + maxBoundaryFromCenter;
        mainCamera.minBoundary = (Vector2)transform.position + minBoundaryFromCenter;
    }

    private void DoorAnimatorsPlay(string _name)
    {
        for (int i = 0; i < doorAnimators.Length; i++) {
            doorAnimators[i].SetTrigger(_name);
        }
    }
}
