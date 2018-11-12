using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Playground
{
	public class PlayerInputSystem : ComponentSystem
	{
		public const float FIRE_COOLDOWN = 0.3f;

		struct PlayerData
		{
			[ReadOnly]public ComponentDataArray<Position> Positions;
			public ComponentDataArray<PlayerInput> Inputs;
		}

		[Inject]private PlayerData _players;

		private RaycastHit _hit;

		protected override void OnUpdate()
		{
			float dt = Time.deltaTime;
			// Ray r;
			for(int i=0;i<_players.Positions.Length;++i)
			{
				float3 curr_pos = _players.Positions[0].Value;

				PlayerInput pi;
				pi.Move.x = Input.GetAxis("Horizontal");
				pi.Move.y = 0.0f;
				pi.Move.z = Input.GetAxis("Vertical");
				// pi.OpenFire = Input.GetMouseButton(0);
				pi.FireCooldown = Mathf.Max(0.0f, _players.Inputs[i].FireCooldown - dt);
				// pi.FireCooldown = Mathf.Max(0.0f, _players.Inputs[i].FireCooldown - dt);

				// float3 dir = pi.FacingDir;
				float3 dir = new float3(0,0,0);

/* 				r = Camera.main.ScreenPointToRay(Input.mousePosition);
				if(Physics.Raycast(Camera.main.transform.position, r.direction, out _hit, 50.0f))
				{
					Vector3 vec = (_hit.point - new Vector3(curr_pos.x, curr_pos.y, curr_pos.z)).normalized;
					dir.x = vec.x;
					dir.y = vec.y;
					dir.z = vec.z;

					Debug.Log($"update dir to {dir}");
				} */

				Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				point.y = curr_pos.y;
				Vector3 vec = (point - new Vector3(curr_pos.x, curr_pos.y, curr_pos.z)).normalized;
				dir.x = vec.x;
				dir.y = vec.y;
				dir.z = vec.z;

				pi.FacingDir = dir;
				pi.OpenFire = Input.GetMouseButton(0) ? 1 : 0;

				_players.Inputs[i] = pi;
			}
		}
	}
}