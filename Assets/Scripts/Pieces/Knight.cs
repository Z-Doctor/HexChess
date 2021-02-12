using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : MonoBehaviour, IPiece
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
        int offset = location.row % 2 == 0 ? 1 : -1;

        possible.Add((board.GetHexIfInBounds(location.row + 5, location.col), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row + 5, location.col + offset), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row + 4, location.col + offset), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row + 4, location.col - offset), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row + 1, location.col + (2 * offset)), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row + 1, location.col - offset), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row - 1, location.col - offset), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row - 1, location.col + (2 * offset)), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row - 4, location.col - offset), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row - 4, location.col + offset), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row - 5, location.col + offset), MoveType.Move));
        possible.Add((board.GetHexIfInBounds(location.row - 5, location.col), MoveType.Move));

        for(int i = possible.Count - 1; i >= 0; i--)
        {
            (Hex possibleHex, MoveType moveType) = possible[i];
            if(possibleHex == null)
            {
                possible.RemoveAt(i);
                continue;
            }
            
            if(boardState.biDirPiecePositions.ContainsKey(possibleHex.index))
            {
                (Team occupyingTeam, Piece occupyingType) = boardState.biDirPiecePositions[possibleHex.index];
                if(occupyingTeam == team)
                    possible.RemoveAt(i);
                else
                    possible[i] = (possibleHex, MoveType.Attack);
            }
        }

        return possible;
    }

    public void MoveTo(Hex hex)
    {
        transform.position = hex.transform.position + Vector3.up;
        location = hex.index;
    }
}
