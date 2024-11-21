using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Door : MonoBehaviour
{
    public RoomTemplates templates;

    private AddRoom thisRoom;
    public List<Animator> doorAnimators;
    public GameObject doorObject;
    public bool isChangedDoor = false;

    public int doorDirection;
    // 0 --> center
    // 1 --> top : -3
    // 2 --> bottom : +3
    // 3 --> right : -5.4
    // 4 --> left : +5.4

    private Vector2 nextRoomPosition = Vector2.zero;
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

        doorAnimators = new List<Animator>(GetComponentsInChildren<Animator>());
        if (doorDirection == 0) {
            isaac = isaac != null ? isaac : FindAnyObjectByType<IsaacBody>();
            foreach (Door door in GetComponentsInChildren<Door>()) {
                door.isaac = door.isaac != null ? door.isaac : isaac;
            }
        }
    }

    private bool isDoorOpen = false;
    private void Update()
    {
        if (doorDirection == 0) {
            if (thisRoom.IsClear && !isDoorOpen) {
                isDoorOpen = true;
                DoorAnimatorsPlay("Open");
            }
            else if (!thisRoom.IsClear && isDoorOpen) {
                isDoorOpen = false;
                DoorAnimatorsPlay("Close");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!templates.refreshedRooms) return;

        if (collision.CompareTag("Player") && thisRoom.IsClear && doorDirection != 0) {
            nextRoomPosition = GetNextRoomPosition(nextRoomPosition);
            StartCoroutine(CreateAndDisableCollider(nextRoomPosition));
        }
        else if (collision.CompareTag("Door") && doorDirection == 0) {
            int needThisDoor = GetNeedDoorDirection(collision);
            if (!thisRoom.IsClear && !isChangedDoor && collision.GetComponent<Door>().isChangedDoor) {
                foreach (Door door in GetComponentsInChildren<Door>()) {
                    if (door.doorDirection == needThisDoor) {
                        StartCoroutine(door.ChangeToSelectedDoor(collision.GetComponent<Door>().doorObject));
                        break;
                    }
                }
            }
            else {
                MoveIsaacPositionToDoor(collision, needThisDoor);
            }
        }
    }

    private Vector2 GetNextRoomPosition(Vector2 _nextRoomPosition)
    {
        Vector2 nextRoomPos = Vector2.zero;
        if (_nextRoomPosition == Vector2.zero) {
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
        return nextRoomPos != Vector2.zero ? nextRoomPos : _nextRoomPosition;
    }

    // doorDirection != 0
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

    private int GetNeedDoorDirection(Collider2D collision)
    {
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
        return needThisDoor;
    }

    private void MoveIsaacPositionToDoor(Collider2D collision, int thisDoor)
    {
        foreach (Door door in GetComponentsInChildren<Door>()) {
            if (door.doorDirection == thisDoor) {
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

                collision.GetComponent<Door>().thisRoom.CurrentRoom = false;
                thisRoom.CurrentRoom = true;

                SetForCameras(thisDoor);
                break;
            }
        }
    }

    // doorDirection == 0
    private void SetForCameras(int thisDoor)
    {
        mainCamera = mainCamera != null ? mainCamera : Camera.main.GetComponent<MainCamera>();

        mainCamera.maxBoundary = (Vector2)transform.position + maxBoundaryFromCenter;
        mainCamera.minBoundary = (Vector2)transform.position + minBoundaryFromCenter;

        Transform minimapTransform = GameManager.Instance.minimap.transform;
        Vector3 offset = Vector3.zero;
        switch (thisDoor) {
            case 1:
                offset.y += 0.5f;
                break;
            case 2:
                offset.y += -0.5f;
                break;
            case 3:
                offset.x += 0.5f;
                break;
            case 4:
                offset.x += -0.5f;
                break;
        }
        minimapTransform.position += offset;
    }

    // doorDirection == 0
    private void DoorAnimatorsPlay(string _name)
    {
        RemoveMissingAnimators(doorAnimators);
        for (int i = 0; i < doorAnimators.Count; i++) {
            doorAnimators[i].SetTrigger(_name);
        }
    }

    // doorDirection != 0
    public IEnumerator ChangeToSelectedDoor(GameObject _doorObject)
    {
        ChangeDoor(_doorObject);

        yield return null;
        
        RefreshDoorAnimators();
    }
    public IEnumerator ChangeToSelectedDoorCoroutine(GameObject _doorObject)
    {
        ChangeDoor(_doorObject);

        nextRoomPosition = GetNextRoomPosition(nextRoomPosition);
        yield return StartCoroutine(CreateAndDisableCollider(nextRoomPosition));
        yield return null;

        RefreshDoorAnimators();
    }

    private void ChangeDoor(GameObject _doorObject)
    {
        isChangedDoor = true;

        doorObject = _doorObject;

        Destroy(doorAnimators[0].gameObject);
        Instantiate(_doorObject, transform.position, transform.rotation, transform);
    }
    
    public void RefreshDoorAnimators()
    {
        doorAnimators.Clear();
        foreach (Animator animator in GetComponentsInChildren<Animator>()) {
            doorAnimators.Add(animator);
        }
        
        List<Animator> parentDoorAnimators = transform.parent.GetComponent<Door>().doorAnimators;
        parentDoorAnimators.Clear();
        foreach (Animator animator in transform.parent.GetComponentsInChildren<Animator>()) {
            parentDoorAnimators.Add(animator);
        }

        if (thisRoom.IsClear && !isDoorOpen) {
            DoorAnimatorsPlay("Open");
        }
        else if (!thisRoom.IsClear && isDoorOpen) {
            DoorAnimatorsPlay("Close");
        }
    }

    private void RemoveMissingAnimators(List<Animator> animators)
    {
        animators.RemoveAll(animator => animator == null);
    }
}
