using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queen : MonoBehaviour, IPiece
{
    public GameObject obj {get => gameObject; set{}}
    public Team team { get{ return _team; } set{ _team = value; } }
    private Team _team;
    public Piece type { get{ return _type; } set{ _type = value; } }
    private Piece _type;
    public Index location { get{ return _location; } set{ _location = value; } }
    private Index _location;
    public bool captured { get{ return _captured; } set{ _captured = value; } }
    private bool _captured = false;

    public void Init(Team team, Piece type, Index startingLocation)
    {
        this.team = team;
        this.type = type;
        this.location = startingLocation;
    }

    public List<(Hex, MoveType)> GetAllPossibleMoves(Board board, BoardState boardState)
    {
        List<(Hex, MoveType)> possible = new List<(Hex, MoveType)>();
        int offset = location.row % 2;

        // Up
        for(int row = location.row + 2; row <= board.hexGrid.rows; row += 2)
            if(!CanMove(board, boardState, row, location.col, ref possible))
                break;
        // Down
        for(int row = location.row - 2; row >= 0; row -= 2)
            if(!CanMove(board, boardState, row, location.col, ref possible))
                break;
        // Left
        for(int col = location.col - 1; col >= 0; col--)
            if(!CanMove(board, boardState, location.row, col, ref possible))
                break;
        // Right
        for(int col = location.col + 1; col <= board.hexGrid.cols - 2 + location.row % 2; col++)
            if(!CanMove(board, boardState, location.row, col, ref possible))
                break;

        // Top Left
        for(
            (int row, int col, int i) = (location.row + 1, location.col - offset, 0); 
            row <= board.hexGrid.rows && col >= 0; 
            row++, i++
        ){
            if(!CanMove(board, boardState, row, col, ref possible))
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
            if(!CanMove(board, boardState, row, col, ref possible))
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
            if(!CanMove(board, boardState, row, col, ref possible))
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
            if(!CanMove(board, boardState, row, col, ref possible))
                break;

            if(i % 2 != offset)
                col++;
        }

        return possible;
    }

    private bool CanMove(Board board, BoardState boardState, int row, int col, ref List<(Hex, MoveType)> possible)
    {
        Hex hex = board.GetHexIfInBounds(row, col);
        if(hex != null)
        {
            if(boardState.biDirPiecePositions.ContainsKey(hex.index))
            {
                (Team occupyingTeam, Piece occupyingType) = boardState.biDirPiecePositions[hex.index];
                if(occupyingTeam != team)
                    possible.Add((hex, MoveType.Attack));
                return false;
            }
            possible.Add((hex, MoveType.Move));
            return true;
        }
        return false;
    }

    public void MoveTo(Hex hex)
    {
        transform.position = hex.transform.position + Vector3.up;
        location = hex.index;
    }
}
