namespace Content.Shared._NF.SpiritBoard;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class SpiritBoardComponent : Component
{
    //If true, there already is a seance in progress and further attempts should be blocked
    //TODO: Rename this so it lines up with other variable names
    [DataField, AutoNetworkedField]
    public bool IsSeanceInProgress = false;

    /// <summary>
    /// The entity currently channeling ghosts, or null if nobody currently is. Used for distance checks to keep the channeling going, and to apply damage.
    /// </summary>
    [DataField]
    public EntityUid? ChannelingEntity = null;

    /// <summary>
    /// The time at which the next letter should be displayed.
    /// </summary>
    [DataField]
    public TimeSpan NextLetterTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan LetterSendInterval = TimeSpan.FromSeconds(1.5);

    /// <summary>
    /// The message that is being sent. If an empty string, then no message is being sent, and the board is neutral.
    /// </summary>
    [DataField]
    public string Message = "";

    /// <summary>
    /// The index of the next letter to send
    /// </summary>
    [DataField]
    public int CurrentLetterIndex = 0;
}
