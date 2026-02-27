using Content.Server.Interaction;
using Content.Server.Popups;
using Content.Shared._NF.SpiritBoard;
using Content.Shared.Ghost;
using Content.Shared.Popups;
using Content.Shared.Verbs;

namespace Content.Server._NF.SpiritBoard;

public sealed class SpiritBoardSystem : EntitySystem
{
    [Dependency] private PopupSystem _popupSystem = default!;
    [Dependency] private InteractionSystem _interactionSystem = default!;


    public override void Initialize()
    {
        SubscribeLocalEvent<SpiritBoardComponent, GetVerbsEvent<ActivationVerb>>(GetSeanceVerb);
        SubscribeLocalEvent<SpiritBoardComponent, GetVerbsEvent<ActivationVerb>>(GetChannelVerb);
    }

    /// <summary>
    /// Sends letters at a constant interval, and ends the channeling if the person doing the channeling walks to far away
    /// </summary>
    /// <param name="frameTime"></param>
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
    }

    /// <summary>
    /// Checks if the user can seance (as a ghost), and adds the verb
    /// </summary>
    public void GetSeanceVerb(EntityUid uid, SpiritBoardComponent comp, GetVerbsEvent<ActivationVerb> eventArgs)
    {
        //Only ghosts should ever see this verb
        if (!HasComp<GhostComponent>(eventArgs.User))
            return;
        //If nobody is summoning spirits, or a seance is already in progress, don't allow a ghost to seance
        if (!comp.ChannelingEntity.HasValue || comp.IsSeanceInProgress)
            return;

        ActivationVerb verb = new()
        {
            Act = () =>
            {
                _popupSystem.PopupEntity(Loc.GetString("spirit-board-channeling-started-placeholder"), uid);
            },

            Text = Loc.GetString("spirit-board-ghost-seance-verb"),
            Priority = 1

        };
        eventArgs.Verbs.Add(verb);
    }

    /// <summary>
    /// Checks if the user can channel (as a living) and adds the verb.
    /// </summary>
    public void GetChannelVerb(EntityUid uid, SpiritBoardComponent comp, GetVerbsEvent<ActivationVerb> eventArgs)
    {
        //They have to be able to interact, must be able to reach it, and nobody can already be channeling
        //TODO: Consider adding check for damageable, or upgrading to complex interaction, though Hammy channeling ghosts would be funny
        if (!eventArgs.CanInteract || !eventArgs.CanAccess || comp.ChannelingEntity.HasValue)
            return;

        ActivationVerb verb = new()
        {
            Act = () =>
            {
                //TODO: Add a condition for aghosts with no initial popup (So you don't see "the UristMcAdmin lays their hands on the board")
                _popupSystem.PopupEntity(Loc.GetString("spirit-board-channeling-started-placeholder"), eventArgs.User);
                comp.ChannelingEntity = eventArgs.User;
            },
            Text = Loc.GetString("spirit-board-living-channel-verb"),
            Priority = 1,
        };
        eventArgs.Verbs.Add(verb);

    }

    //Unknown if I am going to use the ghost's entity, but better safe than sorry for now /shrug
    public void OnGhostMessage(EntityUid board, EntityUid ghost, string message)
    {

    }
}

