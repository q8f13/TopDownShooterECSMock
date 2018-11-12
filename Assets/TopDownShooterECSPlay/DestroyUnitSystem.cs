using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Playground
{
	public class RemoveUnitBuffer : BarrierSystem{}

	// [UpdateAfter(typeof(BulletSpawnerSystem))]
	[UpdateAfter(typeof(DmgSystem))]
	public class DestroyUnitSystem : ComponentSystem {
	// public class DestroyUnitSystem : JobComponentSystem {
		public struct BulletData
		{
			public EntityArray Entities;
			public ComponentDataArray<Shot> Shots;
		}

		[Inject] private BulletData _data;

		private struct PlayerCheck
		{
			public EntityArray Entities;
			[ReadOnly]public ComponentDataArray<PlayerInput> PlayerInput;
			[ReadOnly]public ComponentDataArray<Health> Health;
		}
		[Inject] private PlayerCheck _playerCheck;

		struct EnemyData
		{
			public EntityArray Entities;
			[ReadOnly]public ComponentDataArray<Enemy> Enemies;
			public ComponentDataArray<Health> Healths;
		}
		[Inject] private EnemyData _enemyCheckData;

		[Inject] private RemoveUnitBuffer _removeBuffer;

		struct RemoveBulletsJob : IJobProcessComponentDataWithEntity<Shot>
		{
			public bool PlayerDead;
			public EntityCommandBuffer Commands;

			public void Execute(Entity entity, int index, ref Shot shot)
			{
				if(shot.TimeToLive <= 0 || PlayerDead)
				{
					Commands.DestroyEntity(entity);
				}
			}
		}

		struct RemoveUnitJob : IJobProcessComponentDataWithEntity<Health>
		{
			public EntityCommandBuffer Commands;
			public bool PlayerDead;

			public void Execute(Entity entity, int index, ref Health health)
			{
				if(health.Value <= 0)
				{
					Commands.DestroyEntity(entity);
				}
			}
		}

/* 		protected override JobHandle OnUpdate(JobHandle inputDeps)  
		{
			EntityCommandBuffer cmd = _removeBuffer.CreateCommandBuffer();

			var remove_bullet = new RemoveBulletsJob
			{
				PlayerDead = _playerCheck.PlayerInput.Length == 0,
				Commands = cmd
			}.ScheduleSingle(this, inputDeps);

			var remove_unit = new RemoveUnitJob
			{
				Commands = cmd
			}.ScheduleSingle(this,remove_bullet);

			return remove_unit;
		} */

		protected override void OnUpdate()
		{
			bool playerAlive = _playerCheck.PlayerInput.Length > 0;
			float dt = Time.deltaTime;

			for(int i=0;i<_data.Entities.Length;++i)
			{
				Shot s = _data.Shots[i];
				if(s.TimeToLive > 0)
					s.TimeToLive -= dt;
				if(s.TimeToLive <= 0.0f || !playerAlive)
				{
					PostUpdateCommands.DestroyEntity(_data.Entities[i]);
				}

				_data.Shots[i] = s;
			}

			for(int i=0;i<_enemyCheckData.Enemies.Length;++i)
			{
				Health hp = _enemyCheckData.Healths[i];
				if(hp.Value <= 0 || !playerAlive)
				{
					PostUpdateCommands.DestroyEntity(_enemyCheckData.Entities[i]);
				}
			}

			for(int i=0;i<_playerCheck.PlayerInput.Length;i++)
			{
				if(_playerCheck.Health[i].Value <= 0)
				{
					PostUpdateCommands.DestroyEntity(_playerCheck.Entities[i]);
				}
			}
		}
	}
}

