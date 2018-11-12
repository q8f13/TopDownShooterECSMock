using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Rendering;
using Unity.Transforms;

namespace Playground
{
	public class Bootloader {
		public static EntityArchetype FloorArcheType;
		public static EntityArchetype BulletSpawnerType;
		public static EntityArchetype BounceBallArcheType;
		public static EntityArchetype EnemyArchveType;

		public static MeshInstanceRenderer Floor;
		public static MeshInstanceRenderer BounceBall;
		public static MeshInstanceRenderer EnemyPrefab;
		public static MeshInstanceRenderer BulletPrefab;

		public static MeshInstanceRenderer OnHitOne;
		public static MeshInstanceRenderer OnHitPlayer;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		public static void Init()
		{

			var manager = World.Active.GetOrCreateManager<EntityManager>();
			// Debug.Log("bootLoader init()");

			FloorArcheType = manager.CreateArchetype(
				typeof(Position), typeof(Rotation), typeof(Scale)
			);
			BounceBallArcheType = manager.CreateArchetype(
				typeof(Position), typeof(Rotation), typeof(PlayerInput), typeof(Health), typeof(HealthDamageCooldown), typeof(BlinkTimer)
			);
			EnemyArchveType = manager.CreateArchetype(
				typeof(Position), typeof(Rotation), typeof(Enemy), typeof(MoveSpeed), typeof(Health), typeof(BlinkTimer)
			);
			BulletSpawnerType = manager.CreateArchetype(
				typeof(Bullet)
			);

			// Entity ett_floor = manager.CreateEntity(null);
			// Entity BounceBall = manager.CreateEntity(null);
			Floor = GetLookFromPrototype("plane");
			BounceBall = GetLookFromPrototype("ball");
			EnemyPrefab = GetLookFromPrototype("enemy");
			OnHitOne = GetLookFromPrototype("onhit_enemy");
			OnHitPlayer = GetLookFromPrototype("onhit_player");
			BulletPrefab = GetLookFromPrototype("BulletPrototype");

			Entity ett_floor = manager.CreateEntity(FloorArcheType);
			manager.SetComponentData(ett_floor, new Position{Value = new float3(0.0f, 0.0f, 0.0f)});
			manager.SetComponentData(ett_floor, new Scale{Value = new float3(1.5f, 1.5f, 1.5f)});

			// World.Active.GetOrCreateManager<GameTimer>();

			manager.AddSharedComponentData(ett_floor, Floor);

			World.Active.GetOrCreateManager<UIUpdater>().GetElementRefs();


			
			SpawnerSystem.Setup(World.Active.GetOrCreateManager<EntityManager>());

			Debug.Log("bootloader init done");
		}

		public static void StartNew()
		{
			EntityManager em = World.Active.GetOrCreateManager<EntityManager>();

			Entity ett_ball = em.CreateEntity(BounceBallArcheType);
			em.SetComponentData(ett_ball, new Position{Value = new float3(0.0f, 0.5f, 0.0f)});
			em.SetComponentData(ett_ball, new Health{Value = GameSettings.PLAYER_HP});
			em.AddSharedComponentData(ett_ball, BounceBall);

			SpawnerSystem.Reset(em);
		}

		private static MeshInstanceRenderer GetLookFromPrototype(string go_name) 
		{
			var proto = GameObject.Find(go_name);
			// meshinstanren
			var result = proto.GetComponent<MeshInstanceRendererComponent>().Value;
			Object.Destroy(proto);
			return result;
		}
	}

}

