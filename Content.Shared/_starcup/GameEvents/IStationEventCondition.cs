using Robust.Shared.Prototypes;

namespace Content.Shared._starcup.GameEvents;

[ImplicitDataDefinitionForInheritors]
public partial interface IStationEventCondition
{
    public bool Evaluate(IEntityManager entityManager, IPrototypeManager prototypeManager);
}

