using System;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : MonoBehaviour, IPiece
{
    public GameObject obj {get => gameObject; set{}}
    public Team team { get{ return _team; } set{ _team = value; } }
    private Team _team;
    public Piece piece { get{ return _piece; } set{ _piece = value; } }
    private Piece _piece;
    public Index location { get{ return _location; } set{ _location = value; } }
    private Index _location;
    public bool captured { get{ return _captured; } set{ _captured = value; } }
    private bool _captured = false;
    public ushort value {get => 3; set{}}
    private Vector3? targetPos = null;
    public float speed = 15f;
    
    public void Init(Team team, Piece piece, Index startingLocation)
    {
        this.team = team;
        this.piece = piece;
        this.location = startingLocation;
    }

    public IEnumerable<(Hex, MoveType)> GetAllPossibleMoves(Board board, BoardState boardState, bool includeBlocking = false)
    {
        List<(Hex, MoveType)> possible = new List<(Hex, MoveType)>();
        int offset = location.row % 2;
        
        // Top Left
        for(
            (int row, int col, int i) = (location.row + 1, location.col - offset, 0); 
            row <= board.hexGrid.rows && col >= 0; 
            row++, i++
        ){
            if(!CanMove(board, boardState, row, col, ref possible, includeBlocking))
                break;

            if(i % 2 == offset)
                col--;
        }
        // Top Right
        for(
            (int row, int col, int i) = (location.row + 1, location.col + Mathf.Abs(1 - offset), 0);
            row <= board.hexGrid.rows && col <= board.hexGrid.cols;
            row++, i++
        ){
            if(!CanMove(board, boardState, row, col, ref possible, includeBlocking))
                break;

            if(i % 2 != offset)
                col++;
        }
        // Bottom Left
        for(
            (int row, int col, int i) = (location.row - 1, location.col - offset, 0);
            row >= 0 && col >= 0;
            row--, i++
        ){
            if(!CanMove(board, boardState, row, col, ref possible, includeBlocking))
                break;

            if(i % 2 == offset)
                col--;
        }
        // Bottom Right
        for(
            (int row, int col, int i) = (location.row - 1, location.col + Mathf.Abs(1 - offset), 0);
            row >= 0 && col <= board.hexGrid.cols;
            row--, i++
        ){
            if(!CanMove(board, boardState, row, col, ref possible, includeBlocking))
                break;

            if(i % 2 != offset)
                col++;
        }

        return possible;
    }


    private bool CanMove(Board board, BoardState boardState, int row, int col, ref List<(Hex, MoveType)> possible, bool includeBlocking = false)
    {
        Hex hex = board.GetHexIfInBounds(row, col);
        if(hex != null)
        {
            if(boardState.allPiecePositions.ContainsKey(hex.index))
            {
                (Team occupyingTeam, Piece occupyingType) = boardState.allPiecePositions[hex.index];
                if(occupyingTeam != team || includeBlocking)
                    possible.Add((hex, MoveType.Attack));
                return false;
            }
            possible.Add((hex, MoveType.Move));
            return true;
        }
        return false;
    }

    public void MoveTo(Hex hex, Action action = null)
    {
        targetPos = hex.transform.position + Vector3.up;
        location = hex.index;
    }

    private void Update() => MoveOverTime();

    private void MoveOverTime()
    {
        if(!targetPos.HasValue)
            return;

        transform.position = Vector3.Lerp(transform.position, targetPos.Value, Time.deltaTime * speed);
        if((transform.position - targetPos.Value).magnitude < 0.03f)
        {
            transform.position = targetPos.Value;
            targetPos = null;
        }
    }

    public string GetPieceString() => "Bishop";
}