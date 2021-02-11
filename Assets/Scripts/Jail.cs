using System.Collections;
using System.Collections.Generic;
using Extensions;
using UnityEngine;

public class Jail : MonoBehaviour 
{
    [SerializeField] private Team teamToPrison;

    List<IPiece> prisonedPieces = new List<IPiece>();
    [SerializeField] private int piecesAcross;
    [SerializeField] private Vector2 xzOffsets = new Vector2();

    public void Enprison(IPiece piece)
    {
        int xRot = teamToPrison == Team.White ? -90 : 90;
        Vector3 initialRot = piece.obj.transform.rotation.eulerAngles;
        piece.obj.transform.SetPositionAndRotation(
            position: GetNextPos(),
            Quaternion.Euler(initialRot.x, 180, initialRot.z)
            // Quaternion.identity
            // rotation: Quaternion.Euler(xRot, initialRot.y, initialRot.z)
        );
        piece.obj.transform.parent = transform;

        prisonedPieces.Add(piece);
    }

    public Vector3 GetNextPos()
    {
        int xCount = (prisonedPieces.Count % piecesAcross);
        int zCount = ((float)prisonedPieces.Count / (float)piecesAcross).Floor();
        return new Vector3(
            x: transform.position.x + (xzOffsets.x * xCount),
            y: transform.position.y,
            z: transform.position.z + (xzOffsets.y * zCount)
        );
    }
}