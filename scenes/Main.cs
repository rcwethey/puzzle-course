using System;
using Godot;
using Grid.Manager;

namespace Game;

public partial class Main : Node
{
	private Sprite2D cursor;
	private PackedScene buildingScene;
	private Button placeBuildingButton;
	private Vector2I? hoveredGridCell;
	private GridManager gridManager;

	public override void _Ready()
	{
		buildingScene = GD.Load<PackedScene>("res://scenes/Building/Building.tscn");
		gridManager = GetNode<GridManager>("GridManager");
		cursor = GetNode<Sprite2D>("Sprite2D");
		placeBuildingButton = GetNode<Button>("PlaceBuildingButton");
		cursor.Visible = false;
		placeBuildingButton.Pressed += OnButtonPressed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var gridPosition = gridManager.GetMouseGridCellPosition();
		cursor.GlobalPosition = gridPosition * 64;

		if (
			cursor.Visible &&
			(
				!hoveredGridCell.HasValue ||
				hoveredGridCell.Value != gridPosition
			)
		)
		{
			hoveredGridCell = gridPosition;
			gridManager.HighlightExpandedBuildableTiles(hoveredGridCell.Value, 3);
		}
	}

	public override void _UnhandledInput(InputEvent @evt)
	{
		if (
			hoveredGridCell.HasValue &&
			@evt.IsActionPressed("left_click") &&
			gridManager.IsTilePositionValid(hoveredGridCell.Value) &&
			gridManager.IsTilePositionBuildable(hoveredGridCell.Value)
		)
		{
			PlaceBuildingAtHoveredCellPosition();
			cursor.Visible = false;
		}
	}

	private void PlaceBuildingAtHoveredCellPosition()
	{
		if (!hoveredGridCell.HasValue) return;

		var building = buildingScene.Instantiate<Node2D>();
		AddChild(building);

		building.GlobalPosition = hoveredGridCell.Value * 64;
		hoveredGridCell = null;
		gridManager.ClearHighlightedTiles();
	}


	private void OnButtonPressed()
	{
		cursor.Visible = true;
		gridManager.HighlightBuildableTiles();
	}
}
