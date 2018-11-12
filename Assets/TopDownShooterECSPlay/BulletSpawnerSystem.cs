using Unity.Collections;
using Unity.Entities;

namespace Playground
{
	public class BulletSpawnerSystem : ComponentSystem
	{

		public struct Data
		{
			public EntityArray BulletSpawned;
			[ReadOnly]public ComponentDataArray<Bullet> Bullets;
		}

		[Inject] private Data _data;

		protected override void OnUpdate()
		{
			var em = PostUpdateCommands;

			for(int i=0;i<_data.Bullets.Length; ++i)
			{
				var b = _data.Bullets[i];
				var bulletEntity = _data.BulletSpawned[i];

				em.RemoveComponent<Bullet>(bulletEntity);
				em.AddComponent(bulletEntity, new MoveSpeed{Speed = GameSettings.BULLET_SPEED});
				em.AddComponent(bulletEntity, new MoveForward{});
				em.AddComponent(bulletEntity, b.Position);
				em.AddComponent(bulletEntity, b.Rotation);
				em.AddComponent(bulletEntity, b.Scale);
				em.AddComponent(bulletEntity, b.Shot);

				em.AddSharedComponent(bulletEntity, Bootloader.BulletPrefab);
			}
		}
	}
}