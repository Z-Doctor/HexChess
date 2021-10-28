#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using Extensions;

public class BoardStateTests
{
    static readonly Team[] Teams = new Team[] { Team.White, Team.Black };

    private static BoardState CreateBoardState(IEnumerable<(Index location, Team team, Piece piece)>? pieces)
    {
        return CreateBoardState(Team.White, pieces);
    }
    private static BoardState CreateBoardState(Team toMove, IEnumerable<(Index location, Team team, Piece piece)>? pieces)
    {
        var allPiecePositions = new BidirectionalDictionary<(Team, Piece), Index>();
        if (pieces != null)
        {
            foreach (var piece in pieces)
            {
                allPiecePositions.Add((piece.team, piece.piece), piece.location);
            }
        }

        return new BoardState(allPiecePositions, toMove, Team.None, Team.None, 0);
    }

    #region IsChecking tests
    [Test]
    public void IsChecking_NoPiecesTest()
    {
        var bs = CreateBoardState(Array.Empty<(Index, Team, Piece)>());
        Assert.False(MoveValidator.IsChecking(Team.White, bs, null));
        Assert.False(MoveValidator.IsChecking(Team.Black, bs, null));
    }
    [Test]
    public void IsChecking_KingOnlyNoCheckTest()
    {
        var bs = CreateBoardState(new[] {
            (new Index(1, 'E'), Team.White, Piece.King),
            (new Index(9, 'E'), Team.Black, Piece.King),
        });

        Assert.False(MoveValidator.IsChecking(Team.White, bs, null));
        Assert.False(MoveValidator.IsChecking(Team.Black, bs, null));
    }

    [Test]
    public void IsChecking_KingCheckEachOtherTest()
    {
        var bs = CreateBoardState(new[] {
            (new Index(5, 'E'), Team.White, Piece.King),
            (new Index(6, 'E'), Team.Black, Piece.King),
        });

        Assert.True(MoveValidator.IsChecking(Team.White, bs, null));
        Assert.True(MoveValidator.IsChecking(Team.Black, bs, null));
    }

    [Test]
    public void IsChecking_PawnCheckTest()
    {
        var bs = CreateBoardState(new[] {
            (new Index(1, 'A'), Team.White, Piece.King),
            (new Index(5, 'E'), Team.Black, Piece.King),
            (new Index(5, 'F'), Team.White, Piece.Pawn1),
        });

        Assert.True(MoveValidator.IsChecking(Team.White, bs, null));
        Assert.False(MoveValidator.IsChecking(Team.Black, bs, null));
    }
    
    [Test]
    public void IsChecking_BishopCheckTest()
    {
        var bs1 = CreateBoardState(new[] {
            (new Index(1, 'A'), Team.White, Piece.King),
            (new Index(5, 'E'), Team.Black, Piece.King),
            (new Index(7, 'A'), Team.White, Piece.KingsBishop),
        });
        Assert.True(MoveValidator.IsChecking(Team.White, bs1, null));

        var bs2 = CreateBoardState(new[] {
            (new Index(1, 'A'), Team.White, Piece.King),
            (new Index(5, 'E'), Team.Black, Piece.King),
            (new Index(7, 'A'), Team.White, Piece.KingsBishop),
            (new Index(7, 'B'), Team.White, Piece.KingsRook), // blocking bishop
        });
        Assert.False(MoveValidator.IsChecking(Team.White, bs2, null));
    }

    [Test]
    public void IsChecking_RookCheckTest()
    {
        var bs1 = CreateBoardState(new[] {
            (new Index(1, 'A'), Team.White, Piece.King),
            (new Index(5, 'E'), Team.Black, Piece.King),
            (new Index(5, 'A'), Team.White, Piece.KingsRook),
        });
        Assert.True(MoveValidator.IsChecking(Team.White, bs1, null));

        var bs2 = CreateBoardState(new[] {
            (new Index(1, 'A'), Team.White, Piece.King),
            (new Index(5, 'E'), Team.Black, Piece.King),
            (new Index(5, 'B'), Team.White, Piece.KingsRook),
        });
        Assert.False(MoveValidator.IsChecking(Team.White, bs2, null));

        var bs3 = CreateBoardState(new[] {
            (new Index(1, 'A'), Team.White, Piece.King),
            (new Index(5, 'E'), Team.Black, Piece.King),
            (new Index(5, 'A'), Team.White, Piece.KingsRook),
            (new Index(5, 'C'), Team.White, Piece.Pawn5), // blocks rook
        });
        Assert.False(MoveValidator.IsChecking(Team.White, bs3, null));
    }
    [Test]
    public void IsChecking_KnightCheckTest()
    {
        var bs1 = CreateBoardState(new[] {
            (new Index(1, 'A'), Team.White, Piece.King),
            (new Index(5, 'E'), Team.Black, Piece.King),
            (new Index(5, 'H'), Team.White, Piece.KingsKnight),
        });
        Assert.True(MoveValidator.IsChecking(Team.White, bs1, null));

        var bs2 = CreateBoardState(new[] {
            (new Index(1, 'A'), Team.White, Piece.King),
            (new Index(5, 'E'), Team.Black, Piece.King),
            (new Index(4, 'H'), Team.White, Piece.KingsKnight),
        });
        Assert.False(MoveValidator.IsChecking(Team.White, bs2, null));

        var bs3 = CreateBoardState(new[] {
            (new Index(1, 'A'), Team.White, Piece.King),
            (new Index(5, 'E'), Team.Black, Piece.King),
            (new Index(5, 'H'), Team.Black, Piece.KingsKnight), // Knight on same team as king
        });
        Assert.False(MoveValidator.IsChecking(Team.White, bs3, null));

    }
    [Test]
    public void IsChecking_SquireCheckTest()
    {
        var bs1 = CreateBoardState(new[] {
            (new Index(1, 'A'), Team.White, Piece.King),
            (new Index(5, 'E'), Team.Black, Piece.King),
            (new Index(4, 'F'), Team.White, Piece.WhiteSquire),
        });
        Assert.True(MoveValidator.IsChecking(Team.White, bs1, null));

        var bs2 = CreateBoardState(new[] {
            (new Index(1, 'A'), Team.White, Piece.King),
            (new Index(5, 'E'), Team.Black, Piece.King),
            (new Index(4, 'G'), Team.White, Piece.WhiteSquire),
        });
        Assert.False(MoveValidator.IsChecking(Team.White, bs2, null));
    }

    [Test]
    public void IsChecking_PawnPromotionCheckTest()
    {
        var bs = CreateBoardState(new[] {
            (new Index(1, 'A'), Team.White, Piece.King),
            (new Index(5, 'E'), Team.Black, Piece.King),
            (new Index(1, 'E'), Team.White, Piece.Pawn1),
        });

        Assert.False(MoveValidator.IsChecking(Team.Black, bs, null));
        Assert.False(MoveValidator.IsChecking(Team.White, bs, null));
        Assert.True(MoveValidator.IsChecking(Team.White, bs, new List<Promotion>() { new Promotion(Team.White, Piece.Pawn1, Piece.Queen, 0) }));
        Assert.False(MoveValidator.IsChecking(Team.Black, bs, new List<Promotion>() { new Promotion(Team.White, Piece.Pawn1, Piece.Queen, 0) }));
    }
    #endregion

    #region ApplyMove tests
    [Test]
    public void ApplyMove_MoveTest()
    {
        var bs = CreateBoardState(new[] {
            (new Index(1, 'A'), Team.White, Piece.King),
            (new Index(5, 'E'), Team.Black, Piece.King),
            (new Index(1, 'E'), Team.White, Piece.Pawn1),
        });

        (Team, Piece) piece = (Team.White, Piece.Pawn1);
        (Index target, MoveType moveType) move = (new Index(2, 'E'), MoveType.Move);
        (BoardState newState, List<Promotion> promotions) = HexachessagonEngine.QueryMove(new Index(1, 'E'), move, bs, Piece.Pawn1, null);

        Assert.Null(promotions);
        Assert.True(newState.IsOccupiedBy(move.target, piece));
        Assert.False(newState.IsOccupied(new Index(1, 'E')));
    }
    [Test]
    public void ApplyMove_PromotionTest()
    {
        var bs = CreateBoardState(new[] {
            (new Index(1, 'A'), Team.White, Piece.King),
            (new Index(5, 'E'), Team.Black, Piece.King),
            (new Index(8, 'E'), Team.White, Piece.Pawn1),
        });

        (BoardState newState, List<Promotion> promotions) = HexachessagonEngine.QueryMove(new Index(8, 'E'), (new Index(9, 'E'), MoveType.Move), bs, Piece.Queen, null);

        Assert.AreEqual(promotions[0], new Promotion(Team.White, Piece.Pawn1, Piece.Queen, 1));
        Assert.AreEqual(1, promotions.Count);

        Assert.NotNull(promotions);
        Assert.True(newState.IsOccupiedBy(new Index(9, 'E'), (Team.White, Piece.Pawn1)));
        Assert.False(newState.IsOccupied(new Index(8, 'E')));
    }

    [Test]
    public void ApplyMove_AttackTest()
    {
        Index victimLocation = new Index(5, 'E');
        Index attackerLocation = new Index(1, 'E');

        var bs = CreateBoardState(new[] {
            (new Index(1, 'A'), Team.White, Piece.King),
            (new Index(9, 'I'), Team.Black, Piece.King),
            (attackerLocation, Team.White, Piece.KingsRook),
            (victimLocation, Team.Black, Piece.Pawn1),
        });

        (Index target, MoveType moveType) move = (victimLocation, MoveType.Attack);
        var attacker = bs.allPiecePositions[attackerLocation];
        (BoardState newState, List<Promotion> promotions) = HexachessagonEngine.QueryMove(attackerLocation, move, bs, Piece.Pawn1, null);

        Assert.Null(promotions);
        Assert.True(newState.IsOccupiedBy(move.target, attacker));
        Assert.False(newState.IsOccupied(attackerLocation));
    }

    [Test]
    public void ApplyMove_DefendTest()
    {
        Index victimLocation = new Index(2, 'E');
        Index defenderLocation = new Index(1, 'E');

        var bs = CreateBoardState(new[] {
            (new Index(9, 'I'), Team.Black, Piece.King),
            (victimLocation, Team.White, Piece.King),
            (defenderLocation, Team.White, Piece.KingsRook),
        });

        (Index target, MoveType moveType) move = (victimLocation, MoveType.Defend);
        var defender = bs.allPiecePositions[defenderLocation];
        var victim = bs.allPiecePositions[victimLocation];

        (BoardState newState, List<Promotion> promotions) = HexachessagonEngine.QueryMove(defenderLocation, move, bs, Piece.Pawn1, null);

        Assert.Null(promotions);
        Assert.True(newState.IsOccupiedBy(victimLocation, defender));
        Assert.True(newState.IsOccupiedBy(defenderLocation, victim));
    }

    [Test]
    public void ApplyMove_EnPassantTest()
    {
        Index attackerLocation = new Index(6, 'B');
        Index victimLocation = new Index(6, 'A');

        var bs = CreateBoardState(new[] {
            (new Index(9, 'I'), Team.Black, Piece.King),
            (new Index(1, 'A'), Team.White, Piece.King),
            (victimLocation, Team.Black, Piece.Pawn1),
            (attackerLocation, Team.White, Piece.Pawn1),
        });

        (Index target, MoveType moveType) move = (victimLocation.GetNeighborAt(HexNeighborDirection.Up)!.Value, MoveType.EnPassant);
        var victim = bs.allPiecePositions[victimLocation];
        var attacker = bs.allPiecePositions[attackerLocation];
        (BoardState newState, List<Promotion> promotions) = HexachessagonEngine.QueryMove(attackerLocation, move, bs, Piece.Pawn1, null);

        Assert.Null(promotions);
        Assert.False(newState.IsOccupied(attackerLocation));
        Assert.False(newState.IsOccupied(victimLocation));
        Assert.True(newState.IsOccupiedBy(move.target, attacker));
    }
    #endregion


    #region HasAnyValidMoves tests

    [Test]
    public void AnyValid_EmptyBoardTest()
    {
        var board1 = CreateBoardState(null);
        Assert.False(MoveValidator.HasAnyValidMoves(Team.White, null, board1, default));
        Assert.False(MoveValidator.HasAnyValidMoves(Team.Black, null, board1, default));

        var board2 = CreateBoardState(new[] {
            (new Index(5, 'E'), Team.White, Piece.King),
        });

        Assert.True(MoveValidator.HasAnyValidMoves(Team.White, null, board2, default));
        Assert.False(MoveValidator.HasAnyValidMoves(Team.Black, null, board2, default));

        var board3 = CreateBoardState(new[] {
            (new Index(5, 'E'), Team.Black, Piece.King),
        });

        Assert.False(MoveValidator.HasAnyValidMoves(Team.White, null, board3, default));
        Assert.True(MoveValidator.HasAnyValidMoves(Team.Black, null, board3, default));
    }

    [Test]
    public void AnyValid_StalemateTest([ValueSource(nameof(Teams))] Team attacker)
    {
        Team defender = attacker.Enemy();
        var bs = CreateBoardState(defender, new[] {
            (new Index(9, 'A'), attacker, Piece.King),
            (new Index(3, 'B'), attacker, Piece.Queen),
            (new Index(1, 'A'), defender, Piece.King),
        });
        Assert.False(MoveValidator.HasAnyValidMoves(defender, null, bs, default));
    }

    #endregion

    #region Move generation and validation

    [Test]
    public void ValidateEnPassantTest()
    {
        // White pawn C2 -> C4
        var state1 = CreateBoardState(Team.White, new[] {
            (new Index(9, 'I'), Team.Black, Piece.King),
            (new Index(1, 'A'), Team.White, Piece.King),
            (new Index(2, 'C'), Team.White, Piece.Pawn1),
            (new Index(4, 'B'), Team.Black, Piece.Pawn1),
        });
        // Black pawn on B4 can enpassant -> C3
        var state2 = CreateBoardState(Team.Black, new[] {
            (new Index(9, 'I'), Team.Black, Piece.King),
            (new Index(1, 'A'), Team.White, Piece.King),
            (new Index(4, 'C'), Team.White, Piece.Pawn1),
            (new Index(4, 'B'), Team.Black, Piece.Pawn1),
        });

        var enPassantMove = (new Index(4, 'B'), new Index(3, 'C'), MoveType.EnPassant, Piece.Pawn1);
        var moves = MoveGenerator.GenerateAllValidMoves(Team.Black, null, state2, state1).ToArray();
        Assert.That(moves, Has.Member(enPassantMove));

        moves = MoveGenerator.GenerateAllValidMoves(Team.Black, null, state2, state2).ToArray();
        Assert.That(moves, Has.No.Member(enPassantMove));
    }
    #endregion
}