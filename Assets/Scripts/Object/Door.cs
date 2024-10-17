using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    private AddRoom thisRoom;

    public int doorDirection;
    // 0 --> center
    // 1 --> top : -3
    // 2 --> bottom : +3
    // 3 --> right : -5.4
    // 4 --> left : +5.4

    private Vector2 nextRoomPos = Vector2.zero;
    private BoxCollider2D nextRoomCollider = null;

    private IsaacBody isaac;

    private MainCamera mainCamera;
    public Vector2 maxBoundaryFromCenter;
    public Vector2 minBoundaryFromCenter;

    private void Awake()
    {
        thisRoom = transform.parent.parent.GetComponent<AddRoom>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && thisRoom.isClear) {
            isaac = isaac != null ? isaac : collision.GetComponent<IsaacBody>();

            if (nextRoomPos == Vector2.zero) {
                switch (doorDirection) {
                    case 1:
                        nextRoomPos.y += 40;
                        break;
                    case 2:
                        nextRoomPos.y += -40;
                        break;
                    case 3:
                        nextRoomPos.x += 40;
                        break;
                    case 4:
                        nextRoomPos.x += -40;
                        break;
                }
            }

            StartCoroutine(CreateAndRemoveCollider(nextRoomPos));
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
                            nextIsaacPos.y += -3;
                            break;
                        case 2:
                            nextIsaacPos.y += 3;
                            break;
                        case 3:
                            nextIsaacPos.x += -5.4f;
                            break;
                        case 4:
                            nextIsaacPos.x += -5.4f;
                            break;
                    }
                    isaac.GetComponent<Rigidbody2D>().position = nextIsaacPos;

                    SetBoundaryForCamera();
                    break;
                }
            }
        }
    }

    private IEnumerator CreateAndRemoveCollider(Vector2 _nextRoomPos)
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
}
