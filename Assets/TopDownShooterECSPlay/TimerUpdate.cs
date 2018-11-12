using System.Collections;
using Unity.Entities;
using Unity.Collections;
using System;
using UnityEngine.UI;
using UnityEngine;
using Unity.Jobs;

namespace Playground{
	public class TimerUpdate : JobComponentSystem 
	{
		public struct TimeData
		{
			public ComponentDataArray<GameTimer> Timer;
		}

		[Inject] TimeData _timerData;

		struct UpdateTimer : IJobProcessComponentData<GameTimer>
		{
			public float DeltaTime;
			public void Execute(ref GameTimer timer)
			{
				// timer.Value += DeltaTime;
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)	
		{
			return new UpdateTimer{
				DeltaTime = Time.deltaTime
			}.ScheduleSingle(this, inputDeps);
		}
	}
}
