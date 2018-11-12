using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Playground
{
	public class DmgSystem : JobComponentSystem
	{
		struct Player 
		{
			public ComponentDataArray<Health> Healths;
			public ComponentDataArray<HealthDamageCooldown> HurtCooldown;
			public ComponentDataArray<BlinkTimer> Blink;
			[ReadOnly]public ComponentDataArray<Position> Position;
			[ReadOnly]public ComponentDataArray<PlayerInput> PlayerMarker;
		}

		[Inject] Player _player;
		[Inject] HitBlinkSystem.BlinkBarrier _barrier;

		struct Enemies
		{
			public ComponentDataArray<Enemy> EnemyArr;
			public ComponentDataArray<Health> Healths;
			public ComponentDataArray<BlinkTimer> Blinks;
			// public NativeArray<Entity> EnemyEntities;
			[ReadOnly]public ComponentDataArray<Position> Positions;
		}

		[Inject] Enemies _enemies;

		struct PlayerShotsData
		{
			public ComponentDataArray<Shot> Shots;
			[ReadOnly]public ComponentDataArray<Position> ShotPositions;
			// [ReadOnly]public ComponentDataArray<Shot> Shots;
		}

		[Inject] PlayerShotsData _shotsData;

		[BurstCompile]
		struct HurtByEnemyJob : IJobParallelFor
		{
			public float PlayerHitboxRadiusSquared;

			public ComponentDataArray<Health> PlayerHealth;
			public ComponentDataArray<BlinkTimer> Blinks;
			public ComponentDataArray<HealthDamageCooldown> HurtCooldown;

			[ReadOnly]public ComponentDataArray<Position> EnemyPos;
			[ReadOnly]public ComponentDataArray<Position> PlayerPos;
			
			public void Execute(int index)
			{
				// dmg from enemies to player
				float3 playerPos = PlayerPos[index].Value;
				Health playerHealth = PlayerHealth[index];
				BlinkTimer playerBlink = Blinks[index];
				HealthDamageCooldown hcd = HurtCooldown[index];
				for(int i=0;i<EnemyPos.Length;++i)
				{
					float3 enemyPos = EnemyPos[i].Value;
					float3 distance = enemyPos - playerPos;
					float squard = math.dot(distance, distance);
					// player been touched
					if(squard <= PlayerHitboxRadiusSquared && hcd.CanBeHurt)
					{
						playerHealth.Value = math.max(playerHealth.Value - GameSettings.ENEMY_DMG, 0);
						hcd.Value = GameSettings.HURT_COOLDOWN;
						PlayerHealth[index] = playerHealth;
						HurtCooldown[index] = hcd;
						playerBlink.Value = GameSettings.BLINK_TIME * 5;
						Blinks[index] = playerBlink;
					}

/* 					Health enemy_health = EnemyHealth[i];
					for(int j=0;j<Shots.Length;++j)
					{
						// dmg from player shots to enemies
						float3 shotPos = ShotPos[j].Value;
						float3 delta = shotPos - enemyPos;
						float squard_to_bullet = math.dot(delta,delta);
						Shot s = Shots[j];
						if(squard_to_bullet <= EnemyHitboxRadiusSquared)
						{
							// destroy bullet
							s.TimeToLive = 0.0f;
							// -hp
							enemy_health.Value -= GameSettings.BULLET_DMG;
							Shots[j] = s;
							EnemyHealth[i] = enemy_health;
							break;
						}
					} */
				}

			}
		}

		[BurstCompile]
		public struct ShotEnemiesJob : IJobParallelFor
		// struct ShotEnemiesJob : IJobProcessComponentData<Shot, Position, Enemy>
		{
			public float EnemyHitboxRadiusSquared;
			[NativeDisableParallelForRestriction]
			public ComponentDataArray<Shot> Shots;
			[NativeDisableParallelForRestriction]
			[ReadOnly]public ComponentDataArray<Position> ShotPos;
			[ReadOnly]public ComponentDataArray<Position> EnemyPos;
			[NativeDisableParallelForRestriction]
			public ComponentDataArray<Enemy> Enemies;
			[NativeDisableParallelForRestriction]
			public ComponentDataArray<BlinkTimer> Blink;
			[NativeDisableParallelForRestriction]
			public ComponentDataArray<Health> EnemyHealth;

			public void Execute(int index)
			{
/* 				EnemyHitboxRadiusSquared = 1.0f;
				Shots = _shotsData.Shots;
				ShotPos =_shotsData.ShotPositions;
				EnemyPos = _enemies.Positions; */

				Shot s = Shots[index];
				float3 bullet_pos = ShotPos[index].Value;
				for(int j=0;j<EnemyHealth.Length;++j)
				{
					Health hp = EnemyHealth[j];
					float3 pos = EnemyPos[j].Value;
					Enemy en = Enemies[j];
					BlinkTimer bt = Blink[j];
					float blinkTimer = bt.Value;

					float distance = math.dot(bullet_pos - pos, bullet_pos - pos);
					if(distance <= EnemyHitboxRadiusSquared)
					{
						// hit
						hp.Value-=s.Energy;
						hp.Value = math.max(hp.Value, 0);
						// destroy bullet
						s.TimeToLive = 0.0f;
						blinkTimer = GameSettings.BLINK_TIME;

						Shots[index] = s;
						EnemyHealth[j] = hp;
						bt.Value = blinkTimer;
						Blink[j] = bt;
						Enemies[j] = en;
					}
				}
			}

/* 			public void Execute(ref Shot shot, ref Position shot_pos, ref Enemy enemy)
			{
				Shot s = shot;
				float3 bullet_pos = shot_pos.Value;
				for(int j=0;j<EnemyHealth.Length;++j)
				{
					Health hp = EnemyHealth[j];
					float3 pos = EnemyPos[j].Value;
					Enemy en = Enemies[j];
					float blinkTimer = en.HitBlinkTimer;

					float distance = math.dot(bullet_pos - pos, bullet_pos - pos);
					if(distance <= EnemyHitboxRadiusSquared)
					{
						// hit
						hp.Value-=s.Energy;
						hp.Value = math.max(hp.Value, 0);
						// destroy bullet
						s.TimeToLive = 0.0f;
						blinkTimer = GameSettings.BLINK_TIME;

						shot = s;
						EnemyHealth[j] = hp;
						en.HitBlinkTimer = blinkTimer;
						Enemies[j] = en;

						BlinkTrigger bt;
						CommandBuffer.SetComponent(EnEntities[j], bt);
					}
				}
			} */
		}

		struct HurtCooldownCountingJob : IJobParallelFor
		{
			public ComponentDataArray<HealthDamageCooldown> HurtCooldown;
			public float dt;

			public void Execute(int index)
			{
				HealthDamageCooldown hcd = HurtCooldown[index];
				if(hcd.Value > 0.0f)
				{
					hcd.Value -= dt;
					HurtCooldown[index] = hcd;
				}
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps) 
		{
			var enemies_to_player = new HurtByEnemyJob
			{
				PlayerHitboxRadiusSquared = 1.0f,
				PlayerHealth = _player.Healths,
				HurtCooldown = _player.HurtCooldown,
				EnemyPos = _enemies.Positions,
				PlayerPos = _player.Position,
				Blinks = _player.Blink
			}.Schedule(_player.Position.Length, 1, inputDeps);

			var hurt_cooldown_check = new HurtCooldownCountingJob
			{
				HurtCooldown = _player.HurtCooldown,
				dt = Time.deltaTime
			}.Schedule(_player.Position.Length, 1, enemies_to_player);

			var shot_enemies = new ShotEnemiesJob
			{
				EnemyHitboxRadiusSquared = 1.0f,
				EnemyHealth = _enemies.Healths,
				EnemyPos = _enemies.Positions,
				Enemies = _enemies.EnemyArr,
				Blink = _enemies.Blinks,
				ShotPos = _shotsData.ShotPositions,
				Shots = _shotsData.Shots,
			}.Schedule(_shotsData.Shots.Length, 1, hurt_cooldown_check);

			return shot_enemies;
			// return hurt_cooldown_check;
		}
	}
}