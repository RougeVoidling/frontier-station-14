using Content.Server.EUI;
using Content.Shared._NF.SpiritBoard;
using Content.Shared.Eui;

namespace Content.Server._NF.SpiritBoard;

public sealed class SpiritBoardGhostEui : BaseEui
{
    private readonly SpiritBoardSystem _system;
    private readonly EntityUid _board;

    public SpiritBoardGhostEui(SpiritBoardSystem system, EntityUid board)
    {
        _system = system;
        _board = board;
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is SpiritBoardGhostMessage ghostMessage)
        {

            _system.OnGhostMessage(_board, this.Player.AttachedEntity!.Value, ghostMessage.Message);
            Close();

        }
    }
}
