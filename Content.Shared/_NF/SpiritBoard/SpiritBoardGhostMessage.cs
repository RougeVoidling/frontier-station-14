using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._NF.SpiritBoard;

[Serializable, NetSerializable]
public sealed class SpiritBoardGhostMessage : EuiMessageBase
{

    public readonly string Message;

    public SpiritBoardGhostMessage(string message)
    {
        Message = message;
    }
}
