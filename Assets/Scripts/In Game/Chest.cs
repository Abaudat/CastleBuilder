public class Chest : CollisionBlock
{
    protected override void OnPlayerCollision()
    {
        playManager.Success();
    }
}
