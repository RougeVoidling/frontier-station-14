namespace Content.Shared._NF.SpiritBoard;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class SpiritBoardComponent : Component
{
    //If true, there already is a seance in progress and further attempts should be blocked
    [DataField, AutoNetworkedField]
    public bool IsSeanceInProgress = false;

    //If true, a living entity is calling forth the spirits
    //TODO: This needs to be edited by the entity system, for now leave it as true for debugging purposes
    //TODO:
    [DataField, AutoNetworkedField, Obsolete("This can be axed, and replaced with a ChannelingEntity != null")]
    public bool IsLivingChanneling = true;

    /// <summary>
    /// The entity currently channeling ghosts. Used for distance checks to keep the channeling going, and to apply damage.
    /// </summary>
    [DataField]
    public EntityUid? ChannelingEntity = null;


}
