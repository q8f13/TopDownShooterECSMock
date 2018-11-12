using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Playground
{
	public class SpawnerSystem : ComponentSystem
	{
		struct State
		{
			[ReadOnly]public ComponentDataArray<GameTimer> Timer;
			public ComponentDataArray<SpawnerState> CurrentState;
		}

		[Inject] State _state;

		private static float _lastSpawnTime;
		private static Entity _gameTimer;

		public static void Setup(EntityManager manager)
		{
			Debug.Assert(_gameTimer == Entity.Null, "should only run 1 time");

			var arch = manager.CreateArchetype(typeof(GameTimer),typeof(SpawnerState));
			var entity = manager.CreateEntity(arch);

			manager.SetComponentData(entity, new GameTimer{Value = 0.0f});
			manager.SetComponentData(entity, new SpawnerState{
				CurrentCount = 0,
				SpawnCooldown = 1.0f,
				KillCount = 0
			});

			_gameTimer = entity;
		}

		public static void Reset(EntityManager em)
		{
			em.SetComponentData(_gameTimer, new GameTimer{
				Value = 0.0f
			});
			em.SetComponentData(_gameTimer, new SpawnerState{
				CurrentCount = 0,
				SpawnCooldown = 1.0f
			});

			_lastSpawnTime = 0.0f;
		}

		protected override void OnUpdate()
		{
			float cooldown = _state.CurrentState[0].SpawnCooldown;
			float curr_time = _state.Timer[0].Value;
			bool spawn = curr_time - _lastSpawnTime > cooldown;
			// bool spawn = Time.time < cooldown;

			if(spawn)
			{
				_lastSpawnTime = _state.Timer[0].Value;
				SpawnOne();
			}
		}

		void SpawnOne()
		{
			SpawnerState state = _state.CurrentState[0];

			float3 spawnPosition = GetRandomPosition();
			state.CurrentCount++;

			Debug.Log($"spawn new enemy at {spawnPosition}");

			PostUpdateCommands.CreateEntity(Bootloader.EnemyArchveType);
			PostUpdateCommands.SetComponent(new Position{Value = spawnPosition});
			PostUpdateCommands.SetComponent(new Rotation{Value = quaternion.LookRotation(new float3(0.0f, 0.0f, -1.0f), math.up())});
			PostUpdateCommands.SetComponent(new MoveSpeed{Speed = GameSettings.ENEMY_SPEED});
			PostUpdateCommands.SetComponent(new Health{Value = GameSettings.ENEMY_HP});

			PostUpdateCommands.AddSharedComponent(Bootloader.EnemyPrefab);
			// PostUpdateCommands.AddSharedComponent(Bootloader.EnemyPrefab);

			_state.CurrentState[0] = state;
		}

		float3 GetRandomPosition()
		{
			const float SIDE = 15.0f;
			float r = UnityEngine.Random.value;
			float r_dimension = UnityEngine.Random.value;
			int r_side = UnityEngine.Random.value > 0.5f ? 1 : -1;
			float3 result = new float3(0,0,0);
			result.y = 0.5f;

			if(r_dimension < 0.5f)
			{
				result.x = SIDE / 2.0f * r;
				result.z = r_side * SIDE / 2.0f;
			}
			else
			{
				result.z = SIDE / 2.0f * r;
				result.x = r_side * SIDE / 2.0f;
			}

			return result;
		}
	}
}