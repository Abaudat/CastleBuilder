public class SpikeTrap : CollisionBlock
{
    protected override void OnPlayerCollision()
    {
        playManager.Die();
    }
}
