using Content.Server.Atmos.EntitySystems;
using Content.Server.Light.EntitySystems;
using Content.Server.Parallax;
using Content.Shared._DeltaV.Planet;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Content.Shared.Parallax.Biomes;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._DeltaV.Planet;

public sealed class PlanetSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly BiomeSystem _biome = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedSunShadowSystem _sunShadow = default!;

    private readonly List<(Vector2i, Tile)> _setTiles = new();

    /// <summary>
    /// Spawn a planet map from a planet prototype.
    /// </summary>
    public EntityUid SpawnPlanet(ProtoId<PlanetPrototype> id, bool runMapInit = true)
    {
        var planet = _proto.Index(id);

        var map = _map.CreateMap(out _, runMapInit: runMapInit);
        _biome.EnsurePlanet(map, _proto.Index(planet.Biome), mapLight: planet.MapLight);

        // add each marker layer
        var biome = Comp<BiomeComponent>(map);
        foreach (var layer in planet.BiomeMarkerLayers)
        {
            _biome.AddMarkerLayer(map, biome, layer);
        }

        if (planet.AddedComponents is {} added)
            EntityManager.AddComponents(map, added);

        _atmos.SetMapAtmosphere(map, false, planet.Atmosphere);

        _meta.SetEntityName(map, Loc.GetString(planet.MapName));

        return map;
    }

    /// <summary>
    /// Spawns an initialized planet map from a planet prototype and loads a grid onto it.
    /// Returns the map entity if loading succeeded.
    /// </summary>
    public EntityUid? LoadPlanet(ProtoId<PlanetPrototype> id, ResPath path)
    {
        var map = SpawnPlanet(id, runMapInit: false);
        var mapId = Comp<MapComponent>(map).MapId;
        if (!_mapLoader.TryLoadGrid(mapId, path, out var grid))
        {
            Log.Error($"Failed to load planet grid {path} for planet {id}!");
            Del(map);
            return null;
        }

        // don't want rocks spawning inside the base
        _setTiles.Clear();
        var aabb = Comp<MapGridComponent>(grid.Value).LocalAABB;
        _biome.ReserveTiles(map, aabb.Enlarged(0.2f), _setTiles);

        _map.InitializeMap(map);
        return map;
    }


    public void MakePlanet(Entity<MapGridComponent> grid, ProtoId<PlanetPrototype> id, EntityUid map)
    {
        var planet = _proto.Index(id);

        // var map = _map.CreateMap(out _, false);
        _biome.EnsurePlanet(map, _proto.Index(planet.Biome), mapLight: planet.MapLight);

        // add each marker layer
        var biome = Comp<BiomeComponent>(map);
        foreach (var layer in planet.BiomeMarkerLayers)
        {
            _biome.AddMarkerLayer(map, biome, layer);
        }

        if (planet.AddedComponents is {} added)
            EntityManager.AddComponents(map, added);

        _atmos.SetMapAtmosphere(map, false, planet.Atmosphere);

        if (EntityManager.GetComponent<LightCycleComponent>(map) is { } lightCycle)
        {
            lightCycle.MaxLevel = planet.Sunlight;
            lightCycle.MinLevel = planet.Moonlight;
            lightCycle.ClipLight = planet.ClipLight;
            lightCycle.MinLightLevel = planet.MinimumLightLevel;
            lightCycle.MaxLightLevel = planet.MaximumLightLevel;
            return;
        }

        _meta.SetEntityName(map, Loc.GetString(planet.MapName));

        // var mapId = Comp<MapComponent>(map).MapId;
        // if (!_mapLoader.TryLoadGrid(mapId, path, out var grid))
        // {
        //     Log.Error($"Failed to load planet grid {path} for planet {id}!");
        //     Del(map);
        //     return;
        // }

        // don't want rocks spawning inside the base
        _setTiles.Clear();
        var aabb = grid.Comp.LocalAABB;
        _biome.ReserveTiles(map, aabb.Enlarged(0.2f), _setTiles);

        // _map.InitializeMap(map);
    }
}
