using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._RMC14.Xenonids.Shield;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(XenoShieldSystem))]
public sealed partial class XenoShieldComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Shielded = false;

    [DataField, AutoNetworkedField]
    public bool DidPopup = false;

    [DataField, AutoNetworkedField]
    public TimeSpan Duration = TimeSpan.FromSeconds(7);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan ExpiresAt;

    [DataField, AutoNetworkedField]
    public FixedPoint2 PlasmaCost = FixedPoint2.New(50);

    [DataField, AutoNetworkedField]
    public int Armor = 20;

    [DataField, AutoNetworkedField]
    public string ImmuneToStatus = "Stun";

    [DataField, AutoNetworkedField]
    public EntProtoId Effect = "CMEffectShield";
}
