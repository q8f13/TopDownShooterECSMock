using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Playground
{
	[UpdateAfter(typeof(DestroyUnitSystem))]
	class EnemyMoveSystem : JobComponentSystem
	{
		public struct PlayerData
		{
			[ReadOnly]public ComponentDataArray<Position> Positions;
			[ReadOnly]public ComponentDataArray<PlayerInput> Input;
		}

		public struct EnemyMoveData
		{
			public ComponentDataArray<Enemy> Enemies;
			public ComponentDataArray<Position> Position;
			// public ComponentDataArray<Rotation> Rotation;
			public ComponentDataArray<MoveSpeed> Speed;
		}

		[Inject] PlayerData _player;
		[Inject] EnemyMoveData _enemy;

		struct ControlEnemy : IJobProcessComponentData<Enemy, MoveSpeed, Position, Rotation>
		{
			// public float3 Pos;
			// public quaternion Rotation;
			// public float3 UnitPos;
			public float Dt;
			public float3 PlayerPos;

			public void Execute(ref Enemy enemy, ref MoveSpeed speed, ref Position pos, ref Rotation rot)
			{
				float3 dir = math.normalize(PlayerPos - pos.Value);
				rot.Value = quaternion.LookRotation(dir, math.up());

				pos.Value += dir * Dt * speed.Speed;
			}
		}

		protected override JobHandle OnUpdate(Unity.Jobs.JobHandle inputDeps)
		{
			if(_enemy.Position.Length == 0 || _player.Positions.Length == 0)
				return inputDeps;

			float3 player_pos = _player.Positions[0].Value;

			return new ControlEnemy
			{
				PlayerPos = player_pos,
				// UnitPos = _enemy.Position[0].Value,
				Dt = Time.deltaTime
			}.Schedule(this, inputDeps);
		}
	}
}