using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : MonoBehaviour, IPiece
{
    public GameObject obj {get => gameObject; set{}}
    public Team team { get{ return _team; } set{ _team = value; } }
    private Team _team;
    public Piece type { get{ return _type; } set{ _type = value; } }
    private Piece _type;
    public Index location { get{ return _location; } set{ _location = value; } }
    private Index _location;
    private Index startLoc;
    public bool captured { get{ return _captured; } set{ _captured = value; } }
    private bool _captured = false;

    public bool passantable = false;
    public int turnsPassed = 0;

    private Board board;
    
    public void Init(Team team, Piece type, Index startingLocation)
    {
        this.team = team;
        this.type = type;
        this.location = startingLocation;
        startLoc = startingLocation;
    }

    public List<(Hex, MoveType)> GetAllPossibleMoves(Board board, BoardState boardState)
    {
        List<(Hex, MoveType)> possible = new List<(Hex, MoveType)>();
        int pawnOffset = team == Team.White ? 2 : -2;
        int attackOffset = location.row % 2 == 0 ? 1 : -1;

        // Check takes
        Hex take1 = board.GetHexIfInBounds(location.row + (pawnOffset / 2), location.col + attackOffset);
        if(CanTake(take1, boardState))
            possible.Add((take1, MoveType.Attack));
        
        Hex take2 = board.GetHexIfInBounds(location.row + (pawnOffset / 2), location.col);
        if(CanTake(take2, boardState))
            possible.Add((take2, MoveType.Attack));
        
        // Check en passant
        Hex passant1 = board.GetHexIfInBounds(location.row - (pawnOffset / 2), location.col + attackOffset);
        if(CanPassant(passant1, boardState))
            possible.Add((take1, MoveType.EnPassant));
        
        Hex passant2 = board.GetHexIfInBounds(location.row - (pawnOffset / 2), location.col);
        if(CanPassant(passant2, boardState))
            possible.Add((take2, MoveType.EnPassant));

        // One forward
        Hex normHex = board.GetHexIfInBounds(location.row + pawnOffset, location.col);
        if(CanMove(normHex, boardState, ref possible))
            return possible; 
        
        // Two forward on 1st move
        if(location == startLoc)
        {
            Hex boostedHex = board.GetHexIfInBounds(location.row + (pawnOffset * 2), location.col);
            if(CanMove(boostedHex, boardState, ref possible))
                return possible; 
        }
        return possible;
    }

    private bool CanMove(Hex hex, BoardState boardState, ref List<(Hex, MoveType)> possible)
    {
        if(hex == null)
            return false;
        
        if(boardState.biDirPiecePositions.ContainsKey(hex.index))
            return true;
        
        possible.Add((hex, MoveType.Move));
        return false;
    }

    private bool CanTake(Hex hex, BoardState boardState)
    {
        if(hex == null)
            return false;

        if(boardState.biDirPiecePositions.ContainsKey(hex.index))
        {
            (Team occupyingTeam, Piece occupyingType) = boardState.biDirPiecePositions[hex.index];
            if(occupyingTeam != team)
                return true;
        }
        return false;
    }

    private bool CanPassant(Hex passantToHex, BoardState boardState)
    {
        if(passantToHex == null)
            return false;
        
        if(boardState.biDirPiecePositions.ContainsKey(passantToHex.index))
        {
            (Team occupyingTeam, Piece occupyingType) = boardState.biDirPiecePositions[passantToHex.index];
            if(occupyingTeam == team)
                return false;

            if(passantToHex.board.activePieces.ContainsKey((occupyingTeam, occupyingType)))
            {
                IPiece piece = passantToHex.board.activePieces[(occupyingTeam, occupyingType)];
                if(piece is Pawn otherPawn && otherPawn.passantable)
                    return true;
            }
        }
        return false;
    }

    public void MoveTo(Hex hex)
    {
        Index startLoc = location;
        int pawnOffset = team == Team.White ? 2 : -2;
        // If the pawn is moved to it's boosed location, it becomes open to an enpassant
        Index boostedLoc = new Index(location.row + (pawnOffset * 2), location.col);
        if(hex.index == boostedLoc)
        {
            board = hex.board;
            board.newTurn += TurnPassed;
            passantable = true;
        }

        transform.position = hex.transform.position + Vector3.up;
        location = hex.index;
        
        // If the pawn reaches the other side of the board, it can Promote
        int goal = team == Team.White ? 18 - (location.row % 2) : location.row % 2;
        if(location.row == goal)
            hex.board.QueryPromote(this);
    }

    private void TurnPassed(BoardState newState)
    {
        if(turnsPassed >= 1)
        {
            passantable = false;
            turnsPassed = 0;
            board.newTurn -= TurnPassed;
            board = null;
        }
        else
            turnsPassed++;
    }
}