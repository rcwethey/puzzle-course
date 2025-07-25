using Game.Component;
using Godot;

namespace Game.AutoLoad;


public partial class GameEvents : Node2D
{
	public static GameEvents Instance { get; private set; }


	[Signal]
	public delegate void BuildingPlacedEventHandler(BuildingComponent buildingComponent);

	public override void _Notification(int what)
	{
		if (what == NotificationSceneInstantiated)
		{
			Instance = this;
		}
	}
	
	
	public static void EmitBuildingPlaced(BuildingComponent buildingComponent)
	{
		Instance?.EmitSignal(SignalName.BuildingPlaced, buildingComponent);
	}
}
