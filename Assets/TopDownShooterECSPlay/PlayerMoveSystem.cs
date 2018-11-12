using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Playground
{
	public class PlayerMoveSystem : ComponentSystem
	{
		public struct Data
		{
			public ComponentDataArray<Position> Positions;
			public ComponentDataArray<Rotation> Rotations;
			public ComponentDataArray<PlayerInput> Inputs;
		}

		[Inject] private Data _data;

		protected override void OnUpdate()
		{
			float dt = Time.deltaTime;

			for(int i=0;i<_data.Positions.Length;i++)
			{
				float3 pos =  _data.Positions[i].Value;
				quaternion rot = _data.Rotations[i].Value;

				PlayerInput pi = _data.Inputs[0];

				pos += dt * pi.Move * GameSettings.PLAYER_SPEED;
				Quaternion q = Quaternion.LookRotation(pi.FacingDir, Vector3.up);
				rot = (quaternion)q;

				if(pi.Fire)
				{
					pi.FireCooldown = GameSettings.FIRE_COOLDOWN;

					PostUpdateCommands.CreateEntity(Bootloader.BulletSpawnerType);
					PostUpdateCommands.SetComponent(new Bullet
					{
						Shot = new Shot
						{
							TimeToLive = GameSettings.BULLET_TIME,
							Energy = GameSettings.BULLET_DMG
						},
						Position = new Position{Value = pos},
						Rotation = new Rotation{Value = rot},
						// Scale = new Scale{Value = new float3(1.0f, 1.0f, 1.0f)},
						Scale = new Scale{Value = new float3(0.4f, 0.4f, 0.4f)},
						Faction = Factions.FAC_PLAYER
					});
				}

				_data.Positions[i] = new Position{Value = pos};
				_data.Rotations[i] = new Rotation{Value = rot};
				_data.Inputs[i] = pi;
			}
		}
	}
}