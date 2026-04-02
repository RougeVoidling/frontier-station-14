using Content.Client.Eui;
using JetBrains.Annotations;

namespace Content.Client._NF.SpiritBoard;

[UsedImplicitly]
public sealed class SpiritBoardGhostEui : BaseEui
{

    private readonly SpiritBoardGhostWindow _window;

    public SpiritBoardGhostEui()
    {
        _window = new SpiritBoardGhostWindow();

        _window.OnSend += () =>
        {
            _window.Close();
        };

        _window.OnCancel += =>
        {
            _window.Close();
        };
    }
}
