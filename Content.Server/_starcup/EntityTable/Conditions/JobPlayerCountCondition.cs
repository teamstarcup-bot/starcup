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
/// Condition that passes only if the number of players with a specific job falls within the configured range. Only
/// counts players who are currently connected and in-game.
/// </summary>
public sealed partial class JobPlayerCountCondition : EntityTableCondition
{
    [DataField(required: true)]
    public ProtoId<JobPrototype> Job;

    [DataField(required: true)]
    public int Min;

    [DataField(required: true)]
    public int Max = int.MaxValue;

    private static ISharedPlayerManager? _playerManager;

    protected override bool EvaluateImplementation(EntityTableSelector root,
        IEntityManager entMan,
        IPrototypeManager proto,
        EntityTableContext ctx)
    {
        _playerManager ??= IoCManager.Resolve<ISharedPlayerManager>();

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

            foreach (var mindRole in mindComponent.MindRoles)
            {
                if (!entMan.TryGetComponent<MindRoleComponent>(mindRole, out var mindRoleComponent))
                {
                    continue;
                }

                if (mindRoleComponent.JobPrototype == Job)
                {
                    tally++;
                }
            }
        }

        return tally >= Min && tally <= Max;
    }
}
