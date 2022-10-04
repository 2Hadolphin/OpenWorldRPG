#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
	public struct PhysicsFilter
	{
		public static class LayerIndex
		{
			// Unity layers
			public const int Default = 0;
			public const int TransparentFX = 1;
			public const int IgnoreRaycast = 2;
			public const int Water = 4;
            public const int UI = 5;

            // NeoFPS layers
            public const int PostProcessingVolumes = 8; // Volumes used for camera post processing profiles
            public const int EnvironmentRough = 9;	// Used for rough terrain shapes for character traversal
			public const int EnvironmentDetail = 10; // Used for bullet hits, etc, requiring high detail physics (usually render mesh as mesh collider)
            public const int MovingPlatforms = 11; // Used for bullet hits, etc, requiring high detail physics (usually render mesh as mesh collider)
            public const int DynamicProps = 12; // Dynamic rigidbody props
            public const int CharacterControllers = 13; // Character controller objects (root of a character heirarchy)
            public const int CharacterFirstPerson = 14; // Any character geo viewed from first person camera
            public const int CharacterExternal = 15; // Render geometry for characters not in first person
            public const int CharacterPhysics = 16; // Detailed character collision volumes
			public const int CharacterRagdoll = 17; // Character ragdoll colliders
			public const int CharacterNonColliding = 18; // Objects in character heirarchy that might be needed for checks but not collisions
            public const int WieldablesFirstPerson = 19; // Items such as weapons viewed from first person camera
            public const int WieldablesExternal = 20; // Items such as weapons viewed from first person camera
            public const int TriggerZones = 21; // Any trigger zone
            public const int InteractiveObjects = 22; // Used for interactive objects such as switches
            public const int Doors = 23; // Door colliders
            public const int SmallDynamicObjects = 24; // Used for smaller items that should not affect characters, etc
			public const int Effects = 25; // Visual effects such as particles and debris
            public const int AiVisibility = 26; // Used for AI detection and sight checks
		}

		public static class LayerFilter
        {
            // Unity layers
            public static readonly PhysicsFilter Default = new PhysicsFilter (1);
			public static readonly PhysicsFilter TransparentFX = new PhysicsFilter (1 << LayerIndex.TransparentFX);
			public static readonly PhysicsFilter IgnoreRaycast = new PhysicsFilter (1 << LayerIndex.IgnoreRaycast);
			public static readonly PhysicsFilter Water = new PhysicsFilter (1 << LayerIndex.Water);
            public static readonly PhysicsFilter UI = new PhysicsFilter(1 << LayerIndex.UI);

            // NeoFPS layers
            public static readonly PhysicsFilter PostProcessingVolumes = new PhysicsFilter(1 << LayerIndex.PostProcessingVolumes);
            public static readonly PhysicsFilter EnvironmentRough = new PhysicsFilter(1 << LayerIndex.EnvironmentRough);
            public static readonly PhysicsFilter EnvironmentDetail = new PhysicsFilter(1 << LayerIndex.EnvironmentDetail);
            public static readonly PhysicsFilter MovingPlatforms = new PhysicsFilter(1 << LayerIndex.MovingPlatforms);            
            public static readonly PhysicsFilter DynamicProps = new PhysicsFilter(1 << LayerIndex.DynamicProps);
            public static readonly PhysicsFilter CharacterControllers = new PhysicsFilter(1 << LayerIndex.CharacterControllers);
            public static readonly PhysicsFilter CharacterFirstPerson = new PhysicsFilter(1 << LayerIndex.CharacterFirstPerson);
            public static readonly PhysicsFilter CharacterExternal = new PhysicsFilter(1 << LayerIndex.CharacterExternal);
            public static readonly PhysicsFilter CharacterPhysics = new PhysicsFilter(1 << LayerIndex.CharacterPhysics);
            public static readonly PhysicsFilter CharacterRagdoll = new PhysicsFilter(1 << LayerIndex.CharacterRagdoll);
            public static readonly PhysicsFilter CharacterNonColliding = new PhysicsFilter(1 << LayerIndex.CharacterNonColliding);
            public static readonly PhysicsFilter WieldablesFirstPerson = new PhysicsFilter(1 << LayerIndex.WieldablesFirstPerson);
            public static readonly PhysicsFilter WieldablesExternal = new PhysicsFilter(1 << LayerIndex.WieldablesExternal);
            public static readonly PhysicsFilter TriggerZones = new PhysicsFilter(1 << LayerIndex.TriggerZones);
            public static readonly PhysicsFilter InteractiveObjects = new PhysicsFilter(1 << LayerIndex.InteractiveObjects);
            public static readonly PhysicsFilter Doors = new PhysicsFilter(1 << LayerIndex.Doors);
            public static readonly PhysicsFilter SmallDynamicObjects = new PhysicsFilter(1 << LayerIndex.SmallDynamicObjects);
            public static readonly PhysicsFilter Effects = new PhysicsFilter(1 << LayerIndex.Effects);
            public static readonly PhysicsFilter AiVisibility = new PhysicsFilter(1 << LayerIndex.AiVisibility);
        }

		public static class Masks
		{

#if NEOFPS_LIGHTWEIGHT

            // Layer mask for gun-shots. Used both for detecting hits and placing decals. Uses high detail physics meshes
            public static readonly PhysicsFilter BulletBlockers = new PhysicsFilter(LayerFilter.Default | LayerFilter.EnvironmentRough | LayerFilter.Doors | LayerFilter.DynamicProps | LayerFilter.SmallDynamicObjects | LayerFilter.CharacterPhysics | LayerFilter.CharacterRagdoll | LayerFilter.WieldablesExternal);

            // Layer mask for interactable objects and blockers (prevents interacting through scene objects)
            public static readonly PhysicsFilter Interactable = new PhysicsFilter(LayerFilter.InteractiveObjects | LayerFilter.EnvironmentRough | LayerFilter.CharacterPhysics);

            // Layer mask for geometry that can accept decals
            public static readonly PhysicsFilter ShowDecals = new PhysicsFilter(); // No decals in lightweight

#else
            
            // Layer mask for gun-shots. Used both for detecting hits and placing decals. Uses high detail physics meshes
            public static readonly PhysicsFilter BulletBlockers = new PhysicsFilter (LayerFilter.Default | LayerFilter.EnvironmentDetail | LayerFilter.Doors | LayerFilter.DynamicProps | LayerFilter.SmallDynamicObjects | LayerFilter.CharacterPhysics | LayerFilter.CharacterRagdoll | LayerFilter.WieldablesExternal);
            
            // Layer mask for interactable objects and blockers (prevents interacting through scene objects)
            public static readonly PhysicsFilter Interactable = new PhysicsFilter(LayerFilter.Default | LayerFilter.InteractiveObjects | LayerFilter.EnvironmentDetail | LayerFilter.CharacterPhysics);// | LayerFilter.Doors );

#endif

            // layer mask for character collisions with the environment
            public static readonly PhysicsFilter CharacterBlockers = new PhysicsFilter (LayerFilter.Default | LayerFilter.EnvironmentRough | LayerFilter.MovingPlatforms | LayerFilter.Doors | LayerFilter.DynamicProps);

            // layer mask for character collisions with the environment
            public static readonly PhysicsFilter StaticCharacterBlockers = new PhysicsFilter(LayerFilter.Default | LayerFilter.EnvironmentRough);

			// Layer mask for dynamic blockers that can interfere with spawn points
			public static readonly PhysicsFilter SpawnBlockers = new PhysicsFilter (LayerFilter.Default | LayerFilter.CharacterControllers | LayerFilter.EnvironmentRough | LayerFilter.MovingPlatforms | LayerFilter.Doors | LayerFilter.DynamicProps);

            // Layer mask for checking for visible objects
            public static readonly PhysicsFilter AiVisibilityCheck = new PhysicsFilter (LayerFilter.Default | LayerFilter.AiVisibility | LayerFilter.EnvironmentRough | LayerFilter.MovingPlatforms | LayerFilter.Doors | LayerFilter.DynamicProps);

            // Layer mask for character colliders (used for overlaps for detection)
            public static readonly PhysicsFilter Characters = new PhysicsFilter(LayerFilter.CharacterControllers | LayerFilter.AiVisibility);
        }

		private readonly int m_Value;

		public PhysicsFilter (int filter)
		{
			m_Value = filter;
		}

        public static PhysicsFilter GetMatrixMaskFromLayerIndex(int index)
        {
            int result = 0;
            for (int i = 0; i < 32; ++i)
            {
                if (!Physics.GetIgnoreLayerCollision(index, i))
                    result |= 1 << i;
            }
            return result;
        }

        // Checks
        public static bool operator ==(PhysicsFilter x, PhysicsFilter y)
		{
			return (x.m_Value == y.m_Value);
		}

		public static bool operator !=(PhysicsFilter x, PhysicsFilter y)
		{
			return (x.m_Value != y.m_Value);
		}

		public override bool Equals (object obj)
		{
			if (obj is PhysicsFilter)
				return m_Value == ((PhysicsFilter)obj).m_Value;
			if (obj is int)
				return m_Value == (int)obj;
			return false;
		}

		public override int GetHashCode ()
		{
			return m_Value;
		}

		// Implicit conversions
		public static implicit operator PhysicsFilter (int value)
		{
			return new PhysicsFilter (value);
		}

		public static implicit operator int (PhysicsFilter dam)
		{
			return dam.m_Value;
		}

        public static implicit operator PhysicsFilter(LayerMask value)
        {
            return new PhysicsFilter(value);
        }

        public static implicit operator LayerMask(PhysicsFilter dam)
        {
            return dam.m_Value;
        }

        public PhysicsFilter AddLayer (int layerIndex)
		{
			return new PhysicsFilter (m_Value | (1 << layerIndex));
		}

		public PhysicsFilter RemoveLayer (int layerIndex)
		{
			return new PhysicsFilter (m_Value & ~(1 << layerIndex));
		}

		public PhysicsFilter AddFilter (PhysicsFilter filter)
		{
			return new PhysicsFilter (m_Value | filter.m_Value);
		}

		public PhysicsFilter RemoveFilter (PhysicsFilter filter)
		{
			return new PhysicsFilter (m_Value & ~filter.m_Value);
		}


		public bool ContainsLayer (int layer)
		{
			return (m_Value & (1 << layer)) != 0;
		}
	}
}