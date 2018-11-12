using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Playground
{
	public class MoveForwardSystem : JobComponentSystem
	{
//		[BurstCompile]
		struct MoveForwardUnit : IJobParallelFor
		{
			public ComponentDataArray<Position> Positions;
			[ReadOnly]public ComponentDataArray<Rotation> Rotations;
			[ReadOnly]public ComponentDataArray<MoveSpeed> Speeds;
			[ReadOnly]public ComponentDataArray<Shot> Shot;
			public float dt;

			public void Execute(int i)
			{
				Positions[i] = new Position
				{
					Value = Positions[i].Value + (dt * Speeds[i].Speed * math.forward(Rotations[i].Value))
				};
			}
		}

		ComponentGroup _moveForwardGroup;

		protected override void OnCreateManager()
		{
			_moveForwardGroup = GetComponentGroup(
				ComponentType.ReadOnly(typeof(MoveForward)),
				ComponentType.ReadOnly(typeof(MoveSpeed)),
				ComponentType.ReadOnly(typeof(Rotation)),
				ComponentType.ReadOnly(typeof(Shot)),
				typeof(Position)
			);
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			MoveForwardUnit moveForwardJob = new MoveForwardUnit
			{
				Positions = _moveForwardGroup.GetComponentDataArray<Position>(),
				Rotations = _moveForwardGroup.GetComponentDataArray<Rotation>(),
				Speeds = _moveForwardGroup.GetComponentDataArray<MoveSpeed>(),
				Shot = _moveForwardGroup.GetComponentDataArray<Shot>(),
				dt = Time.deltaTime
			};

			var moveForwardJobHandle = moveForwardJob.Schedule(_moveForwardGroup.CalculateLength(), 64, inputDeps);
			return moveForwardJobHandle;
		}
	}
}