using Content.Shared._starcup.GameEvents;
using Content.Shared.EntityTable;
using Content.Shared.EntityTable.Conditions;
using Content.Shared.EntityTable.EntitySelectors;
using Content.Shared.Mind;
using Content.Shared.Players;
using Content.Shared.Roles;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Shared._starcup.EntityTable.Conditions;

/// <summary>
/// Condition that passes only if the number of players in a department falls within the configured range. Only
/// counts players who are currently connected and in-game.
/// </summary>
public sealed partial class DepartmentalPlayerCountCondition : EntityTableCondition, IStationEventCondition
{
    [DataField(required: true)]
    public ProtoId<DepartmentPrototype> Department;

    [DataField]
    public int Min;

    [DataField]
    public int Max = int.MaxValue;

    private static ISharedPlayerManager? _playerManager;

    private static readonly Dictionary<ProtoId<JobPrototype>, ProtoId<DepartmentPrototype>> JobDepartmentMap = new();

    protected override bool EvaluateImplementation(EntityTableSelector root,
        IEntityManager entityManager,
        IPrototypeManager prototypeManager,
        EntityTableContext ctx)
    {
        return Evaluate(entityManager, prototypeManager);
    }

    public bool Evaluate(IEntityManager entityManager, IPrototypeManager prototypeManager)
    {
        _playerManager ??= IoCManager.Resolve<ISharedPlayerManager>();

        if (JobDepartmentMap.Count == 0)
        {
            foreach (var departmentProto in prototypeManager.EnumeratePrototypes<DepartmentPrototype>())
            {
                departmentProto.Roles.ForEach(x => JobDepartmentMap[x] = departmentProto.ID);
            }
        }

        var tally = 0;
        foreach (var session in _playerManager.Sessions)
        {
            if (session.Status != SessionStatus.InGame)
            {
                continue;
            }

            var mind = session.GetMind();
            if (mind == null || mind == EntityUid.Invalid)
            {
                continue;
            }

            if (!entityManager.TryGetComponent<MindComponent>(mind, out var mindComponent))
            {
                continue;
            }

            if (IsMindEmployedInDepartment(entityManager, mindComponent, Department))
            {
                tally++;
            }
        }

        return tally >= Min && tally <= Max;
    }

    private static bool IsMindEmployedInDepartment(IEntityManager entityManager,
        MindComponent mindComponent,
        ProtoId<DepartmentPrototype> department)
    {
        foreach (var mindRole in mindComponent.MindRoles)
        {
            if (!entityManager.TryGetComponent<MindRoleComponent>(mindRole, out var mindRoleComponent))
            {
                continue;
            }

            if (mindRoleComponent.JobPrototype is not {} jobPrototype)
            {
                continue;
            }

            if (JobDepartmentMap[jobPrototype] == department)
            {
                return true;
            }
        }

        return false;
    }
}
