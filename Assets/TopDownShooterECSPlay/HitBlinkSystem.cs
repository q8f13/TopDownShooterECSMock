using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Playground
{
	// [UpdateAfter(typeof(DestroyUnitSystem))]
	[UpdateAfter(typeof(SpawnerSystem))]
	public class HitBlinkSystem : ComponentSystem
	{
		// static Color DefaultColor = new Color(1.0f, 175.0f / 255.0f, 238.0f / 255.0f);

		public class BlinkBarrier : BarrierSystem{}

		struct EnemyUnit
		{
			public readonly int Length;
			[ReadOnly]public EntityArray Entities;
			public ComponentDataArray<Enemy> Enemies;
			public ComponentDataArray<BlinkTimer> Blinks;
			// [ReadOnly]public SharedComponentDataArray<MeshInstanceRenderer> MeshRenderer;
		}

		[Inject] private EnemyUnit _enemy;

		struct PlayerUnit
		{
			public readonly int Length;
			[ReadOnly]public EntityArray Entity;
			public ComponentDataArray<PlayerInput> Inputs;
			public ComponentDataArray<BlinkTimer> Blinks;
		}

		[Inject] private PlayerUnit _player;

		protected override void OnUpdate()
		{
			// EntityManager em = World.Active.GetOrCreateManager<EntityManager>();
			// NativeArray<Entity> ea = em.ent
			for(int i=0;i<_enemy.Length;++i)
			{
				var entity = _enemy.Entities[i];
				Enemy en = _enemy.Enemies[i];
				float blink_timer = _enemy.Blinks[i].Value;
				// float blink_timer = en.HitBlinkTimer;
				// BlinkTrigger bts = _enemy.Triggers[i];

				// MeshInstanceRenderer mr = _enemy.MeshRenderer[i];
				// MeshInstanceRenderer mr = manager.GetSharedComponentData<MeshInstanceRenderer>(entity);
				// Material mat = mr.material;
				if(blink_timer > 0.0f)
				{
					// mat.SetColor("_Color", Color.red);
					blink_timer -= Time.deltaTime;
					_enemy.Blinks[i] = new BlinkTimer{Value = blink_timer};
					// _enemy.Enemies[i] = en;
					// em.SetSharedComponentData<MeshInstanceRenderer>(entity, Bootloader.OnHitOne);
					PostUpdateCommands.SetSharedComponent(entity, Bootloader.OnHitOne);
					// nextMat = Bootloader.OnHitOne;
					// _enemy[i].MeshRenderer = Bootloader.OnHitOne;
				}
				else
				{
					// mat.SetColor("_Color", DefaultColor);
					// MeshInstanceRenderer nextMat = Bootloader.OnHitOne;
					// em.SetSharedComponentData<MeshInstanceRenderer>(entity, Bootloader.EnemyPrefab);
					PostUpdateCommands.SetSharedComponent(entity, Bootloader.EnemyPrefab);
				}
				// mr.material = mat;
				// _enemy.MeshRenderer[i] = mr;
			}

			for(int i=0;i<_player.Length;++i)
			{
				Entity pe = _player.Entity[i];
				BlinkTimer bt = _player.Blinks[i];
				if(bt.Value > 0.0f)
				{
					bt.Value -= Time.deltaTime;
					_player.Blinks[i] = bt;
					PostUpdateCommands.SetSharedComponent(pe, Bootloader.OnHitPlayer);
				}
				else
				{
					PostUpdateCommands.SetSharedComponent(pe, Bootloader.BounceBall);
				}
			}
		}
	}
}