using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : MonoBehaviour, IPiece
{
    public Team team { get{ return _team; } set{ _team = value; } }
    private Team _team;
    public PieceType type { get{ return _type; } set{ _type = value; } }
    private PieceType _type;
    public Index location { get{ return _location; } set{ _location = value; } }
    private Index _location;
    private Index startLoc;
    
    public void Init(Team team, PieceType type, Index startingLocation)
    {
        this.team = team;
        this.type = type;
        this.location = startingLocation;
        startLoc = startingLocation;
    }

    public List<Hex> GetAllPossibleMoves(HexSpawner boardSpawner, BoardState boardState)
    {
        List<Hex> possible = new List<Hex>();
        int pawnOffset = team == Team.White ? 2 : -2;
        
        // One forward
        Hex normHex = boardSpawner.GetHexIfInBounds(location.row + pawnOffset, location.col);
        if(CanMove(normHex, boardState, ref possible))
            return possible; 
        
        // Two forward on 1st move
        if(location == startLoc)
        {
            Hex boostedHex = boardSpawner.GetHexIfInBounds(location.row + (pawnOffset * 2), location.col);
            if(CanMove(boostedHex, boardState, ref possible))
                return possible; 
        }
        return possible;
    }

    private bool CanMove(Hex hex, BoardState boardState, ref List<Hex> possible)
    {
        if(hex == null)
            return false;
        
        if(boardState.bidPiecePositions.ContainsKey(hex.hexIndex))
            return true;
        
        possible.Add(hex);
        return false;
    }

    public void MoveTo(Hex hex)
    {
        transform.position = hex.transform.position + Vector3.up;
        location = hex.hexIndex;
    }
}