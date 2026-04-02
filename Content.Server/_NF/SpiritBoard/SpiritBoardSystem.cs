using Content.Server.EUI;
using Content.Server.Interaction;
using Content.Server.Popups;
using Content.Shared._NF.SpiritBoard;
using Content.Shared.Ghost;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Server.Player;
using Robust.Shared.Timing;

namespace Content.Server._NF.SpiritBoard;

public sealed class SpiritBoardSystem : EntitySystem
{
    [Dependency] private PopupSystem _popupSystem = default!;
    [Dependency] private InteractionSystem _interactionSystem = default!;
    [Dependency] private EuiManager _uiManager = default!;
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private IGameTiming _gameTiming = default!;


    public override void Initialize()
    {
        SubscribeLocalEvent<SpiritBoardComponent, GetVerbsEvent<ActivationVerb>>(GetVerbs);
    }

    /// <summary>
    /// Sends letters at a constant interval, and ends the channeling if the person doing the channeling walks to far away
    /// </summary>
    /// <param name="frameTime"></param>
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        //TODO: Use entity querry enumerator to loop over all spirit boards and process them (use heath analuzer as a reference)
        //Going to cheat and write code assuming we get the entities and the comp through the power of ignoring the 500 errors
        //The errors have upgraded to 38000 dear god

        var boardQuery = EntityQueryEnumerator<SpiritBoardComponent, TransformComponent>();
        while (boardQuery.MoveNext(out var entityUid, out var boardComponent, out var transform))
        {
            //TODO: Remove this placeholder and replace it with a actual distance check to make sure the channeling person hasn't moved too far away.
            var distanceCheckPlaceholder = true;
            if (!boardComponent.ChannelingEntity.HasValue)
                //Board inactive, do nothing.
                continue;
            if (!distanceCheckPlaceholder)
            {
                boardComponent.ChannelingEntity = null;
                boardComponent.Message = "";
                boardComponent.CurrentLetterIndex = 0;
                boardComponent.IsSeanceInProgress = false;
                continue;
            }

            if (boardComponent.IsSeanceInProgress)
            {
                if (boardComponent.NextLetterTime < _gameTiming.CurTime)
                {
                    if (boardComponent.CurrentLetterIndex >= boardComponent.Message.Length)
                    {
                        //TODO: end the message process
                        boardComponent.Message = "";
                        boardComponent.CurrentLetterIndex = 0;
                        boardComponent.IsSeanceInProgress = false;
                    }
                    else
                    {
                        boardComponent.NextLetterTime = _gameTiming.CurTime.Add(boardComponent.LetterSendInterval);
                        var nextLetter = boardComponent.Message.Substring(boardComponent.CurrentLetterIndex, 1);
                        _popupSystem.PopupEntity(nextLetter, entityUid);
                        boardComponent.CurrentLetterIndex++;
                    }
                }
            }
        }
    }

    //This is needed because you can't have multiple subscriptions to the same event
    public void GetVerbs(EntityUid ent, SpiritBoardComponent comp, GetVerbsEvent<ActivationVerb> eventArgs)
    {
        GetChannelVerb(ent, comp, eventArgs);
        GetSeanceVerb(ent, comp, eventArgs);
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
                //TODO: Remove this
                _popupSystem.PopupEntity(Loc.GetString("spirit-board-placeholder-ghost-opened-ui"), uid);
                if (_playerManager.TryGetSessionByEntity(eventArgs.User, out var session))
                {
                    var ghostUi = new SpiritBoardGhostEui(this, uid);
                    _uiManager.OpenEui(ghostUi, session);
                }


            },

            Text = Loc.GetString("spirit-board-ghost-seance-verb"),
            Priority = 0

        };
        eventArgs.Verbs.Add(verb);
    }

    /// <summary>
    /// Checks if the user can channel (as a living) and adds the verb.
    /// TODO: Add a way to stop channeling (apart from just walking away)
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
            Priority = 0,
        };
        eventArgs.Verbs.Add(verb);

    }

    //Unknown if I am going to use the ghost's entity, but better safe than sorry for now /shrug
    //Hey look we did a smart
    public void OnGhostMessage(EntityUid board, EntityUid ghost, string message)
    {
        if (!TryComp<SpiritBoardComponent>(board, out var boardComponent))
        {
            return;
        }

        //TODO: Improve the messages for this
        //TODO: Test and ensure there isn't a global popup
        if (boardComponent.IsSeanceInProgress)
        {
            _popupSystem.PopupEntity(Loc.GetString("spirit-board-message-in-progress"), ghost, ghost, PopupType.Small);
            return;
        }

        if (!boardComponent.ChannelingEntity.HasValue)
        {
            _popupSystem.PopupEntity(Loc.GetString("spirit-board-nobody-channeling"), ghost, ghost, PopupType.Small);
            return;
        }
        //All checks passed, lets start the message

        //Longer delay for the first letter to give it a better chance of not being missed
        //TODO: Maybe change popup style
        var firstLetterTime = _gameTiming.CurTime + boardComponent.LetterSendInterval.Multiply(2);
        boardComponent.NextLetterTime = firstLetterTime;
        boardComponent.Message = message;
        boardComponent.IsSeanceInProgress = true;
        //TODO: Popup over the board itself
    }
}

