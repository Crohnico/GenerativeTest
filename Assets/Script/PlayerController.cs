using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool canMove = false;

    public IACell currentCell;
    private IACell destinyPos = null;
    public Transform playerObject;

    public int x, z = 0;
    public float height = .5f;
    public float walkAnimationTime = .2f;


   // public Action OnPlayerUpdatePosition;
    private bool walkingAnimation;
    private bool isMoving;

    public Vector2 lastDirection = Vector2.zero;

    Tweener tw;

    public void Initialize()
    {
        canMove = true;
    }

    private void OnEnable()
    {
        x = z = 0;
        canMove = false;
    }

    void FixedUpdate()
    {
        if (canMove)
        {
            float xDecimal = transform.position.x - Mathf.Floor(transform.position.x);
            float zDecimal = transform.position.z - Mathf.Floor(transform.position.z);

            TryMove();
        }
    }

    void TryMove()
    {
        if (currentCell == null)
        {
            currentCell = Grid.Istance.GetGridCell((int)transform.position.x, (int)transform.position.z);
            currentCell.IsOccupped = true;
        }

        x = currentCell.GridX;
        z = currentCell.GridZ;

        float movimientoHorizontal = Input.GetAxisRaw("Horizontal");
        float movimientoVertical = Input.GetAxisRaw("Vertical");
        Vector2 movement       = new Vector2(movimientoHorizontal, movimientoVertical);
        Vector2 parsedMovement = new Vector2(movimientoHorizontal, movimientoVertical);

        if (destinyPos == null || destinyPos == currentCell)
        {
            if (walkingAnimation) return;
            IACell targetCell = null;

            int Offset = 0;
            if (Mathf.Abs(movement.x) <= .1f && Mathf.Abs(movement.y) <= .1f) return;

            if (Mathf.Abs(movement.x) >= Mathf.Abs(movement.y)) 
                parsedMovement = new Vector2( (movement.x > 0) ? 1 : -1    ,     0);
            else 
                parsedMovement = new Vector2(0     ,      (movement.y > 0) ? 1 : -1);


            if (Mathf.Abs(parsedMovement.x) > Mathf.Abs(parsedMovement.y))
            {
                Offset = (parsedMovement.x > 0) ? 1 : -1;

                targetCell = Grid.Istance.GetGridCell(x + Offset, z);

                if (transform.localScale.x != Offset)
                {
                    tw?.Kill();
                    tw = transform.DOScale(new Vector3(Offset, transform.localScale.y, transform.localScale.z), .2f);
                }
            }
            else
            {
                Offset = (parsedMovement.y > 0) ? 1 : -1;
                targetCell = Grid.Istance.GetGridCell(x, z + Offset);
            }



            if (targetCell != null)
            {
                if (!targetCell.IsWalkable) targetCell = null;
                     destinyPos = targetCell;
            }

        }
        else
        {
            Vector3 newPosition = new Vector3(destinyPos.GridX, destinyPos.height, destinyPos.GridZ);
            Mover(newPosition);
        }

        lastDirection = parsedMovement;
    }


    void Mover(Vector3 newPosition)
    {
        if (isMoving) return;
        isMoving = true;
        TurnHolder.resolveTurn?.Invoke();
        transform.DOMove(newPosition, walkAnimationTime).SetEase(Ease.Linear).OnComplete(() =>
        {
            currentCell.IsOccupped = false;
            currentCell = Grid.Istance.GetGridCell((int)newPosition.x, (int)newPosition.z);
            currentCell.IsOccupped = true;

            destinyPos = null;
            DOTween.Sequence().AppendInterval(.05f).OnComplete(() =>
            {
                isMoving = false;
            });
        });

        WalkAnimation();   
    }

    private void WalkAnimation()
    {
        if (!walkingAnimation)
        {
            walkingAnimation = true;


            Vector3 InitPos = Vector3.zero;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(playerObject.transform.DOLocalMoveY(height, walkAnimationTime / 2));
            sequence.Append(playerObject.transform.DOLocalMoveY(InitPos.y, walkAnimationTime / 2));
            sequence.OnComplete(() => walkingAnimation = false);


            sequence.Play();
        }
    }
}
