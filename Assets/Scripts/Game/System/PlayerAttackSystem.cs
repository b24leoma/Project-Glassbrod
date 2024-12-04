using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    public class PlayerAttackSystem : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField]private GridSystem gridSystem;
        [Header("Assets")]
        [SerializeField] private GameObject endTurnButton;
        [SerializeField] private LineRenderer pathLine;
        [SerializeField] private Transform iconParent;
        private Entity hoveredEntity;
        private Vector2Int hoveredTile;
        private bool isActing;
        private Entity actingEntity;
        private bool isPlayerTurn;
        private Vector3 offsetFix;
        private BattleController battleController;
        void Start()
        {
            battleController = GetComponent<BattleController>();
            offsetFix = transform.InverseTransformPoint(Vector3.zero);
        }

        public void TileClicked(InputAction.CallbackContext context)
        {
            if (!isPlayerTurn) return;
            if (context.canceled)
            {
                hoveredEntity = gridSystem.GetTile(hoveredTile).linkedEntity;
                if (isActing) // MOVE AND ATTACK MODE
                {
                    if (hoveredEntity == null && pathLine.positionCount > 0) //MOVES TO EMPTY TILE
                    {
                        actingEntity.SetMoving(true);
                        Vector2Int newPos = GetPathLinePos(pathLine.positionCount - 1);
                        battleController.Move(actingEntity.Position, newPos);
                        pathLine.positionCount = 1;
                        SetPathLinePos(0, actingEntity.Position);
                    }
                    else if (hoveredEntity != null ) //TILE HAS ENTITY
                    {
                        if (hoveredEntity.isHuman && hoveredEntity != actingEntity) // SELECTS ANOTHER ACTOR
                        {
                            actingEntity = hoveredEntity;
                            pathLine.positionCount = 1;
                            SetPathLinePos(0, actingEntity.Position);
                        }
                        else // ATTACKS ENEMY
                        {
                            if (!actingEntity.hasAttacked && !hoveredEntity.isHuman && gridSystem.GetGridDistance(actingEntity.Position, hoveredEntity.Position) <=
                                actingEntity.AttackRange)
                            {
                                actingEntity.SetAttacking(true);
                                battleController.Attack(actingEntity, hoveredEntity);
                                battleController.UpdateCharacterDisplay(true, hoveredEntity);
                                if (hoveredEntity is Demon demon)
                                {
                                    demon.DisplayAttackingImage(false, Color.white);   
                                }
                                pathLine.positionCount = 1;
                                SetPathLinePos(0, actingEntity.Position);
                                isActing = false;

                            }
                        }
                    }

                }
                else
                {
                    actingEntity = gridSystem.GetTile(hoveredTile).linkedEntity;
                    if (actingEntity != null) // SWAP CHARACTER
                    {
                        if (actingEntity.isHuman)
                        {
                            isActing = true;
                            pathLine.positionCount = 1;
                            actingEntity = hoveredEntity;
                            SetPathLinePos(0, actingEntity.Position);
                        }
                    }
                }
            }
            UpdateBoard();
        }






        public void MouseMove(InputAction.CallbackContext context)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);

            if (hit.collider != null)
            {
                Vector2 pos = transform.InverseTransformPoint(hit.point) - offsetFix;
                Vector2Int newPos = new Vector2Int((int)Mathf.Round(pos.x +0.5f), (int)Mathf.Round(pos.y+0.5f));
                if (hoveredTile == newPos) return;
                hoveredTile = newPos;
                if (gridSystem.GetTile(hoveredTile).linkedEntity != null)
                {
                    hoveredEntity = gridSystem.GetTile(hoveredTile).linkedEntity;
                }
            }
            UpdateBoard();
        }





        void UpdateBoard()
        {
            //Reset board colors
            gridSystem.HighlightSquaresInRange(Vector2.zero, 50, Color.white);
            foreach (Transform child in iconParent)
            {
                Destroy(child.gameObject);
            }

            battleController.UpdateCharacterDisplay(gridSystem.GetTile(hoveredTile).linkedEntity != null,
                gridSystem.GetTile(hoveredTile).linkedEntity);
            if (gridSystem.GetTile(hoveredTile) == null) return;

            if (isActing)
            {
                //MOVE PATH IF WITHIN RANGE
                if (!actingEntity.hasMoved && gridSystem.GetGridDistance(actingEntity.Position, hoveredTile) <=
                    actingEntity.MoveRange)
                {
                    if (gridSystem.GetTile(hoveredTile).walkable &&
                        (gridSystem.GetTile(hoveredTile).linkedEntity == null ||
                         gridSystem.GetTile(hoveredTile).linkedEntity == actingEntity) &&
                        gridSystem.GetGridDistance(hoveredTile, GetPathLinePos(pathLine.positionCount - 1)) <= 1)
                    {
                        bool valid = true;
                        for (int i = 0; i < pathLine.positionCount; i++)
                        {
                            if (GetPathLinePos(i) == hoveredTile)
                            {
                                valid = false;
                                pathLine.positionCount = i + 1;
                            }
                        }

                        if (valid && pathLine.positionCount < actingEntity.MoveRange)
                        {
                            pathLine.positionCount++;
                            SetPathLinePos(pathLine.positionCount - 1, hoveredTile);
                        }
                    }
                }


                //GRID SPACE HIGHLIGHT
                if (actingEntity.hasMoved)
                {
                    if (!actingEntity.hasAttacked)
                    {
                        gridSystem.HighlightSquaresInRange(GetPathLinePos(pathLine.positionCount - 1),
                            actingEntity.AttackRange, new Color(0.8f, 0.8f, 0.8f));
                        foreach (Vector2Int pos in gridSystem.demons) // ATTACK HIGHLIGHT
                        {
                            Demon demon = gridSystem.GetTile(pos).linkedEntity as Demon;
                            if (gridSystem.GetGridDistance(actingEntity.Position, pos) <=
                                actingEntity.AttackRange)
                                demon.DisplayAttackingImage(true, hoveredTile == pos ? Color.red : Color.white);
                            else demon.DisplayAttackingImage(false, Color.white);

                        }
                    }
                }
                else
                {
                    if (actingEntity.hasAttacked)
                    {
                        gridSystem.HighlightSquaresInRange(GetPathLinePos(pathLine.positionCount - 1),
                            actingEntity.MoveRange - pathLine.positionCount, new Color(0.9f, 0.9f, 0.9f));
                    }
                    else
                    {
                        gridSystem.HighlightSquaresInRange(GetPathLinePos(pathLine.positionCount - 1),
                            actingEntity.MoveRange + actingEntity.AttackRange - pathLine.positionCount,
                            new Color(0.8f, 0.8f, 0.8f));
                        gridSystem.HighlightSquaresInRange(GetPathLinePos(pathLine.positionCount - 1),
                            actingEntity.MoveRange - pathLine.positionCount, new Color(0.9f, 0.9f, 0.9f));
                        gridSystem.SetColor(GetPathLinePos(pathLine.positionCount - 1), new Color(0.7f, 0.7f, 0.7f));
                        foreach (Vector2Int pos in gridSystem.demons) // ATTACK HIGHLIGHT
                        {
                            Demon demon = gridSystem.GetTile(pos).linkedEntity as Demon;
                            if (gridSystem.GetGridDistance(GetPathLinePos(pathLine.positionCount - 1) , pos) <=
                                actingEntity.MoveRange + actingEntity.AttackRange - pathLine.positionCount)
                            {
                                demon.DisplayAttackingImage(true, Color.white);
                            }
                            else demon.DisplayAttackingImage(false, Color.white);

                        }
                    }
                }
            }
            else
            {
                foreach (Vector2Int pos in gridSystem.demons) // ATTACK HIGHLIGHT
                {
                    Demon demon = gridSystem.GetTile(pos).linkedEntity as Demon;
                    demon.DisplayAttackingImage(false, Color.white);
                }
            }
            gridSystem.SetColor(hoveredTile, new Color(0.7f, 0.7f, 0.7f));
        }

        private Vector2Int GetPathLinePos(int pos)
        {
            return new Vector2Int((int)(pathLine.GetPosition(pos).x + 0.5f), (int)(pathLine.GetPosition(pos).y + 0.5f));
        }
        private void SetPathLinePos(int index, Vector2Int pos)
        {
            pathLine.SetPosition(index, new Vector3(pos.x - 0.5f, pos.y - 0.5f, -5));
        }

        public void EndTurn()
        {
            isActing = false;
            isPlayerTurn = false;
            endTurnButton.SetActive(false);
            foreach (Vector2Int p in gridSystem.humans)
            {
                Entity e = gridSystem.GetTile(p).linkedEntity;
                e.SetAttacking(false);
                e.SetMoving(false);
            }
        }

        public void StartTurn()
        {
            isPlayerTurn = true;
            endTurnButton.SetActive(true);
        }
    }
}