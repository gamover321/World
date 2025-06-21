namespace World.Engine.My2dWorld;

public class PhysicsContext
{
	public bool UsePredicted { get; }

	public PhysicsContext(bool usePredicted){
		UsePredicted = usePredicted;
	}

	public static readonly PhysicsContext Actual = new(false);
	public static readonly PhysicsContext Predicted = new(true);
}