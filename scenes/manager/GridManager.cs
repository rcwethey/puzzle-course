using System;
using System.Collections.Generic;
using System.Linq;
using Game.AutoLoad;
using Game.Component;
using Godot;

namespace Grid.Manager;

public partial class GridManager : Node
{
    //private HashSet<Vector2I> occupiedCells = new();

    private HashSet<Vector2I> validBuildableTiles = new();
    private List<TileMapLayer> allTileMapLayers = new();

    [Export]
    private TileMapLayer highlightTileMapLayer;

    [Export]
    private TileMapLayer baseTerrainTileMapLayer;

    public override void _Ready()
    {
        GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
        allTileMapLayers = GetAllTileMapLayers(baseTerrainTileMapLayer);
        Console.WriteLine($"Found {allTileMapLayers.Count} tile map layers in the scene.");
        Console.WriteLine($"Tile Map Layers Found: {string.Join(", ", allTileMapLayers.Select(x => x.Name))}");
    }


    public void ClearHighlightedTiles()
    {
        highlightTileMapLayer.Clear();
    }
    

    public Vector2I GetMouseGridCellPosition()
    {
        var mousePosition = highlightTileMapLayer.GetGlobalMousePosition();
        var gridPosition = (mousePosition / 64).Floor();
        return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
    }


    public bool IsTilePositionValid(Vector2I tilePosition)
    {
        foreach (var layer in allTileMapLayers)
        {
            var customData = layer.GetCellTileData(tilePosition);
            if (customData == null) continue;
            return (bool)customData.GetCustomData("buildable");
        }
        return false;
    }


    public bool IsTilePositionBuildable(Vector2I tilePosition)
    {
        return validBuildableTiles.Contains(tilePosition);
    }


    public void HighlightBuildableTiles()
    {
        foreach (var tiles in validBuildableTiles)
        {
            highlightTileMapLayer.SetCell(tiles, 0, Vector2I.Zero);
        }
    }


    public void HighlightExpandedBuildableTiles(Vector2I rootCell, int radius)
    {
        ClearHighlightedTiles();
        HighlightBuildableTiles();
        var validTiles = GetValidTilesInRadius(rootCell, radius).ToHashSet();
        var occupiedTiles = GetOccupiedTiles().ToHashSet();
        var expandedTiles = validTiles.Except(validBuildableTiles).Except(occupiedTiles).ToList();
        var atlasCoord = new Vector2I(1, 0);
        foreach (var tiles in expandedTiles)
        {
            highlightTileMapLayer.SetCell(tiles, 0, atlasCoord);
        }

    }

    private List<Vector2I> GetValidTilesInRadius(Vector2I rootCell, int radius)
    {
        var result = new List<Vector2I>();
        for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
        {
            for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
            {
                var tilePosition = new Vector2I(x, y);
                if (!IsTilePositionValid(tilePosition))
                {
                    continue;
                }
                result.Add(tilePosition);
            }
        }
        return result;
    }


    private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
    {
        var rootCell = buildingComponent.GetGridCellPosition();
        var buildableRadius = buildingComponent.BuildableRadius;
        var validTiles = GetValidTilesInRadius(rootCell, buildableRadius);
        validBuildableTiles.UnionWith(validTiles);

        var occupiedTiles = GetOccupiedTiles().ToHashSet();
        validBuildableTiles.ExceptWith(occupiedTiles);
    }


    private IEnumerable<Vector2I> GetOccupiedTiles()
    {
        var buildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>();
        var occupiedTiles = buildingComponents.Select(x => x.GetGridCellPosition());
        return occupiedTiles;
    }


    private void OnBuildingPlaced(BuildingComponent buildingComponent)
    {
        UpdateValidBuildableTiles(buildingComponent);
    }


    private static List<TileMapLayer> GetAllTileMapLayers(TileMapLayer rootTileMapLayer)
    {
        var result = new List<TileMapLayer>();
        var children = rootTileMapLayer.GetChildren();
        children.Reverse();
        foreach (var child in children)
        {
            if (child is TileMapLayer childTileMapLayer)
            {
                result.AddRange(GetAllTileMapLayers(childTileMapLayer));
            }
        }
        result.Add(rootTileMapLayer);
        return result;
    }
}