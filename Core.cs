using HarmonyLib;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(JugglerHelper.Core), "JugglerHelper", "1.0.2", "alydev", null)]
[assembly: MelonGame("Finn Schuuring", "King of the Bridge")]

namespace JugglerHelper;

public class Core : MelonMod
{
    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg("Initialized.");
    }

    [HarmonyPatch(typeof(ChessMatchManager), nameof(ChessMatchManager.CompleteMove))]
    public static class CompleteMoveWatcher
    {
        private static bool _haveSeen;
        private static bool _lastIsJuggler;
        private static ChessPieceObject _tinted;
        private static Color _tintedOriginal;

        public static void Postfix(ChessMatchManager __instance)
        {
            ChessPieceObject piece = __instance.lastMovedPieceByWhite;

            bool isJuggler = __instance.isJuggler;

            if (!_haveSeen)
            {
                _haveSeen = true;
                _lastIsJuggler = isJuggler;
            }
            else if (isJuggler != _lastIsJuggler)
            {
                _lastIsJuggler = isJuggler;
                if (!isJuggler)
                {
                    Melon<Core>.Logger.Warning($"Lost juggler after {__instance.juggleCounter} by moving {piece.pieceColor} {piece.pieceData.name}");
                    ClearTint();
                }
            }

            HighlightLastMoved(piece);
        }

        private static void ClearTint()
        {
            _tinted?.spriteRenderer?.color = _tintedOriginal;
            _tinted = null;
        }

        private static void HighlightLastMoved(ChessPieceObject piece)
        {
            if (piece == null || ReferenceEquals(piece, _tinted))
                return;
            ClearTint();

            if (piece.spriteRenderer == null || !_lastIsJuggler)
                return;
            _tintedOriginal = piece.spriteRenderer.color;
            piece.spriteRenderer.color = new(1f, 0.4f, 0.4f);
            _tinted = piece;
        }

        internal static void Reset()
        {
            _haveSeen = false;
            _lastIsJuggler = false;
            _tinted = null;
        }
    }

    [HarmonyPatch(typeof(ChessMatchManager), nameof(ChessMatchManager.SetupChessMatch))]
    public static class SetupMatchWatcher
    {
        public static void Postfix()
        {
            Melon<Core>.Logger.Msg("Resetting.");
            CompleteMoveWatcher.Reset();
        }
    }
}
