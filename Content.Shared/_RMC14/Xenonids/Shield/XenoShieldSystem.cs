using Content.Shared._RMC14.Armor;
using Content.Shared._RMC14.Xenonids.Leap;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared.Coordinates;
using Content.Shared.Actions;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Timing;
using Robust.Shared.Network;

namespace Content.Shared._RMC14.Xenonids.Shield;

public sealed class XenoShieldSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenoShieldComponent, XenoShieldActionEvent>(OnXenoTurnShieldAction);
        SubscribeLocalEvent<XenoShieldComponent, CMGetArmorEvent>(OnXenoShieldGetArmor);

        SubscribeLocalEvent<XenoShieldComponent, BeforeStatusEffectAddedEvent>(OnXenoShieldBeforeStatusAdded);
    }

    private void OnXenoTurnShieldAction(Entity<XenoShieldComponent> xeno, ref XenoShieldActionEvent args)
    {
        if (args.Handled)
            return;

        var attempt = new XenoShieldAttemptEvent();
        RaiseLocalEvent(xeno, ref attempt);

        if (attempt.Cancelled)
            return;

        args.Handled = true;

        xeno.Comp.Shielded = !xeno.Comp.Shielded;
        Dirty(xeno);

        if (!_xenoPlasma.TryRemovePlasmaPopup(xeno.Owner, xeno.Comp.PlasmaCost))
            return;

        xeno.Comp.ExpiresAt = _timing.CurTime + xeno.Comp.Duration;

        if (_net.IsServer)
            SpawnAttachedTo(xeno.Comp.Effect, xeno.Owner.ToCoordinates());
    }

    private void OnXenoShieldGetArmor(Entity<XenoShieldComponent> xeno, ref CMGetArmorEvent args)
    {
        if (xeno.Comp.Shielded)
            args.Armor += xeno.Comp.Armor;
    }

    private void OnXenoShieldBeforeStatusAdded(Entity<XenoShieldComponent> xeno, ref BeforeStatusEffectAddedEvent args)
    {
        if (xeno.Comp.Shielded && args.Key == xeno.Comp.ImmuneToStatus)
            args.Cancelled = true;
    }

    public override void Update(float frameTime)
    {
        var time = _timing.CurTime;
        var activeQuery = EntityQueryEnumerator<XenoShieldComponent>();
        while (activeQuery.MoveNext(out var uid, out var active))
        {
            if (active.ExpiresAt > time)
                continue;

            if (active.Shielded)
            {
                active.Shielded = false;
                active.DidPopup = false;

                var ev = new CMGetArmorEvent();
                RaiseLocalEvent(uid, ref ev);
            }

            if (!active.DidPopup)
            {
                _popup.PopupClient(Loc.GetString("cm-xeno-shield-expire"), uid, uid, PopupType.SmallCaution);
                active.DidPopup = true;
                Dirty(uid, active);
            }
        }
    }
}
