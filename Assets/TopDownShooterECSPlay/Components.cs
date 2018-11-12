using Unity.Jobs;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Playground
{
	public struct Floor : IComponentData{}
	public struct BounceBall : IComponentData{}
	// public struct Position : IComponentData{}
	// public struct Rotation : IComponentData{}
	// public struct Scale : IComponentData{}
	public struct Health : IComponentData{
		public int Value;
	}

	public struct HealthDamageCooldown : IComponentData{
		public float Value;
		public bool CanBeHurt => Value <= 0.0f;
	}

	public struct Bullet : IComponentData{
		public Position Position;
		public Rotation Rotation;
		public Scale Scale;
		public Shot Shot;
		public int Faction;
	}

    public struct MoveForward : IComponentData { }

	public struct Factions
	{
		public const int FAC_PLAYER = 0;
		public const int FAC_ENEMY = 1;
	}

    public struct PlayerInput : IComponentData
    {
        public float3 Move;
        // public float3 Shoot;
		public float3 FacingDir; 
        public float FireCooldown;
		public int OpenFire;

        public bool Fire => OpenFire == 1 && FireCooldown <= 0.0;
    }

	public struct GameTimer : IComponentData
	{
		public float Value;
	}

    public struct Shot : IComponentData
    {
        public float TimeToLive;
        public int Energy;
    }
	
	public struct MoveSpeed : IComponentData
	{
		public float Speed;
	}

	public struct Enemy : IComponentData{
		// public float HitBlinkTimer;
	}

	public struct BlinkTimer : IComponentData{
		public float Value;
	}

	public struct SpawnerState : IComponentData
	{
		public int CurrentCount;
		public int KillCount;
		public float SpawnCooldown;
	}

	public struct DefaultRenderer : ISharedComponentData
	{
        public Mesh                 mesh;
        public Material             material;
	    public int                  subMesh;

        public ShadowCastingMode    castShadows;
        public bool                 receiveShadows;
	}
}
