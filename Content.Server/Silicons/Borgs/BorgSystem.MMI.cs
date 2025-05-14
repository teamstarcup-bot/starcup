using Content.Server.Roles;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Mind.Components;
using Content.Shared.Roles;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects.Components.Localization; // imp; for Grammar
using Robust.Shared.Enums; // imp; for Gender

namespace Content.Server.Silicons.Borgs;

/// <inheritdoc/>
public sealed partial class BorgSystem
{

    [Dependency] private readonly SharedRoleSystem _roles = default!;

    public void InitializeMMI()
    {
        SubscribeLocalEvent<MMIComponent, ComponentInit>(OnMMIInit);
        SubscribeLocalEvent<MMIComponent, EntInsertedIntoContainerMessage>(OnMMIEntityInserted);
        SubscribeLocalEvent<MMIComponent, MindAddedMessage>(OnMMIMindAdded);
        SubscribeLocalEvent<MMIComponent, MindRemovedMessage>(OnMMIMindRemoved);

        SubscribeLocalEvent<MMILinkedComponent, MindAddedMessage>(OnMMILinkedMindAdded);
        SubscribeLocalEvent<MMILinkedComponent, EntGotRemovedFromContainerMessage>(OnMMILinkedRemoved);
    }

    private void OnMMIInit(EntityUid uid, MMIComponent component, ComponentInit args)
    {
        if (!TryComp<ItemSlotsComponent>(uid, out var itemSlots))
            return;

        if (ItemSlots.TryGetSlot(uid, component.BrainSlotId, out var slot, itemSlots))
            component.BrainSlot = slot;
        else
            ItemSlots.AddItemSlot(uid, component.BrainSlotId, component.BrainSlot, itemSlots);
    }

    private void OnMMIEntityInserted(EntityUid uid, MMIComponent component, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != component.BrainSlotId)
            return;

        var ent = args.Entity;
        var linked = EnsureComp<MMILinkedComponent>(ent);
        linked.LinkedMMI = uid;
        Dirty(uid, component);

        //IMP EDIT: keep the pronouns of the brain inserted
        var grammar = EnsureComp<GrammarComponent>(uid);
        if (TryComp<GrammarComponent>(ent, out var formerSelf))
        {
            _grammar.SetGender((uid, grammar), formerSelf.Gender);
            //man-machine interface is not a proper noun, so i'm not setting proper here
        }
        //END IMP EDIT

        if (_mind.TryGetMind(ent, out var mindId, out var mind))
        {
            _mind.TransferTo(mindId, uid, true, mind: mind);

            if (!_roles.MindHasRole<SiliconBrainRoleComponent>(mindId))
                _roles.MindAddRole(mindId, "MindRoleSiliconBrain", silent: true);
        }

        _appearance.SetData(uid, MMIVisuals.BrainPresent, true);
    }

    private void OnMMIMindAdded(EntityUid uid, MMIComponent component, MindAddedMessage args)
    {
        _appearance.SetData(uid, MMIVisuals.HasMind, true);
    }

    private void OnMMIMindRemoved(EntityUid uid, MMIComponent component, MindRemovedMessage args)
    {
        //IMP EDIT: no brain, no gender, bucko
        if (TryComp<GrammarComponent>(uid, out var grammar))
        {
            _grammar.SetGender((uid, grammar), Gender.Neuter); // it/its
        }
        //END IMP EDIT

        _appearance.SetData(uid, MMIVisuals.HasMind, false);
    }

    private void OnMMILinkedMindAdded(EntityUid uid, MMILinkedComponent component, MindAddedMessage args)
    {
        if (!_mind.TryGetMind(uid, out var mindId, out var mind) ||
            component.LinkedMMI == null)
            return;

        _mind.TransferTo(mindId, component.LinkedMMI, true, mind: mind);
    }

    private void OnMMILinkedRemoved(EntityUid uid, MMILinkedComponent component, EntGotRemovedFromContainerMessage args)
    {
        if (Terminating(uid))
            return;

        if (component.LinkedMMI is not { } linked)
            return;
        RemComp(uid, component);

        if (_mind.TryGetMind(linked, out var mindId, out var mind))
        {
            if (_roles.MindHasRole<SiliconBrainRoleComponent>(mindId))
                _roles.MindRemoveRole<SiliconBrainRoleComponent>(mindId);

            _mind.TransferTo(mindId, uid, true, mind: mind);
        }

        _appearance.SetData(linked, MMIVisuals.BrainPresent, false);
    }
}
