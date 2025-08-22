using Content.Shared.EntityTable;
using Content.Shared.EntityTable.Conditions;
using Content.Shared.EntityTable.EntitySelectors;
using Content.Shared.Mind;
using Content.Shared.Players;
using Content.Shared.Roles;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._starcup.EntityTable.Conditions;

/// <summary>
/// Condition that passes only if the number of players in a department falls within the configured range.
/// </summary>
public sealed partial class DepartmentalPlayerCountCondition : EntityTableCondition
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
        IEntityManager entMan,
        IPrototypeManager proto,
        EntityTableContext ctx)
    {

        _playerManager ??= IoCManager.Resolve<ISharedPlayerManager>();

        if (JobDepartmentMap.Count == 0)
        {
            foreach (var departmentProto in proto.EnumeratePrototypes<DepartmentPrototype>())
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

            if (!entMan.TryGetComponent<MindComponent>(mind, out var mindComponent))
            {
                continue;
            }

            if (IsMindEmployedInDepartment(entMan, mindComponent, Department))
            {
                tally++;
            }
        }

        return tally >= Min && tally <= Max;
    }

    private static bool IsMindEmployedInDepartment(IEntityManager entMan,
        MindComponent mindComponent,
        ProtoId<DepartmentPrototype> department)
    {
        foreach (var mindRole in mindComponent.MindRoles)
        {
            if (!entMan.TryGetComponent<MindRoleComponent>(mindRole, out var mindRoleComponent))
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
