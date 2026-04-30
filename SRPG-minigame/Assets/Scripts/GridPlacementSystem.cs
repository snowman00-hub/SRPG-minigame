using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GridPlacementSystem : MonoBehaviour
{
    [Header("References")]
    public Map map;
    public Transform cursorVisual; // 씬에 배치할 커서 오브젝트

    [Header("Settings")]
    public float moveDelay = 0.15f; // 커서 이동 속도 (초 단위)

    private Vector2Int cursorPos;
    private Unit pickedUnit;
    private int cycleIndex = -1;

    private GameInput input;
    private float moveTimer;
    private bool isActive = false;

    private void Awake()
    {
        input = new GameInput();
        // 입력 이벤트 연결
        input.Player.Confirm.performed += _ => OnConfirm();
        input.Player.Cancel.performed += _ => OnCancel();
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (active)
        {
            input.Enable();
            if (cursorVisual != null) cursorVisual.gameObject.SetActive(true);
            
            // 처음 활성화 시 (0,0) 또는 첫 번째 유닛 위치로 초기화
            if (map.spawnedPlayerUnits.Count > 0)
            {
                Unit first = map.spawnedPlayerUnits[0];
                UpdateCursorPosition(new Vector2Int(first.x, first.z));
            }
            else
            {
                UpdateCursorPosition(Vector2Int.zero);
            }
        }
        else
        {
            input.Disable();
            if (cursorVisual != null) cursorVisual.gameObject.SetActive(false);
            pickedUnit = null;
        }
    }

    private void Update()
    {
        if (!isActive) return;

        HandleMovement();
        HandleCycleInput();
    }

    private void HandleMovement()
    {
        Vector2 moveVec = input.Player.Move.ReadValue<Vector2>();

        if (moveVec.sqrMagnitude > 0.1f)
        {
            if (moveTimer <= 0)
            {
                Vector2Int dir = Vector2Int.zero;
                if (Mathf.Abs(moveVec.x) > Mathf.Abs(moveVec.y))
                    dir.x = moveVec.x > 0 ? 1 : -1;
                else
                    dir.y = moveVec.y > 0 ? 1 : -1;

                MoveCursor(dir);
                moveTimer = moveDelay;
            }
            moveTimer -= Time.deltaTime;
        }
        else
        {
            moveTimer = 0;
        }
    }

    private void MoveCursor(Vector2Int dir)
    {
        Vector2Int nextPos = cursorPos + dir;
        if (nextPos.x >= 0 && nextPos.x < map.horizontalSize && nextPos.y >= 0 && nextPos.y < map.verticalSize)
        {
            UpdateCursorPosition(nextPos);
        }
    }

    private void UpdateCursorPosition(Vector2Int newPos)
    {
        cursorPos = newPos;
        if (cursorVisual != null)
        {
            Tile tile = map.GetTile(cursorPos.x, cursorPos.y);
            if (tile != null)
            {
                float yPos = tile.transform.position.y + 0.6f;
                cursorVisual.position = new Vector3(cursorPos.x * map.spacing, yPos, cursorPos.y * map.spacing);
            }
        }
    }

    private void HandleCycleInput()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            var units = map.spawnedPlayerUnits;
            if (units.Count == 0) return;

            cycleIndex = (cycleIndex + 1) % units.Count;
            Unit target = units[cycleIndex];
            if (target != null)
            {
                UpdateCursorPosition(new Vector2Int(target.x, target.z));
            }
        }
    }

    private void OnConfirm()
    {
        Tile currentTile = map.GetTile(cursorPos.x, cursorPos.y);
        if (currentTile == null) return;

        Unit unitOnTile = currentTile.unitOnTile;

        if (pickedUnit == null)
        {
            if (unitOnTile != null && unitOnTile.team == Team.Player)
            {
                pickedUnit = unitOnTile;
                map.SetMarkerHighlight(cursorPos.x, cursorPos.y, true); // 집어 올린 위치 마커 하이라이트 ON
                Debug.Log($"유닛 선택: {pickedUnit.unitName}");
            }
        }
        else
        {
            if (currentTile.isSpawnPoint)
            {
                // 어떤 동작을 하든 일단 기존 하이라이트는 끕니다.
                map.SetMarkerHighlight(pickedUnit.x, pickedUnit.z, false);

                if (unitOnTile == pickedUnit)
                {
                    pickedUnit = null;
                }
                else if (unitOnTile == null)
                {
                    pickedUnit.SetPosition(cursorPos.x, cursorPos.y, map);
                    pickedUnit = null;
                }
                else if (unitOnTile.team == Team.Player)
                {
                    int oldX = pickedUnit.x;
                    int oldZ = pickedUnit.z;

                    Tile oldTile = map.GetTile(oldX, oldZ);
                    if (oldTile != null) oldTile.unitOnTile = null;
                    currentTile.unitOnTile = null;

                    unitOnTile.SetPosition(oldX, oldZ, map);
                    pickedUnit.SetPosition(cursorPos.x, cursorPos.y, map);
                    pickedUnit = null;
                }
            }
        }
    }

    private void OnCancel()
    {
        if (pickedUnit != null)
        {
            map.SetMarkerHighlight(pickedUnit.x, pickedUnit.z, false); // 취소 시 하이라이트 OFF
            pickedUnit = null;
            Debug.Log("선택 취소");
        }
    }
}
