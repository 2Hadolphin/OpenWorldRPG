using UnityEngine;
using KinematicCharacterController;

namespace Return.Framework.PhysicController
{
    public interface IControllerMotor 
    {
        bool enabled { get; set; }

        #region Cache

        Transform Transform { get; }
        ICharacterController CharacterController { get; set; }

        #endregion

        #region Physics

        LayerMask CollidableLayers { get; set; }

        #endregion

        Vector3 TransientPosition { get; }
        Quaternion TransientRotation { get; }

        Vector3 InitialTickPosition { get; set; }
        Quaternion InitialTickRotation { get; set; }

        void SetPosition(Vector3 position, bool bypassInterpolation = true);
        void SetRotation(Quaternion rotation, bool bypassInterpolation = true);
        void SetPositionAndRotation(Vector3 position, Quaternion rotation, bool bypassInterpolation = true);
        void MoveCharacter(Vector3 toPosition);
        void RotateCharacter(Quaternion toRotation);


        void SetTransientPosition(Vector3 newPos);


        /// <summary>
        /// Update phase 1 is meant to be called after physics movers have calculated their velocities, but
        /// before they have simulated their goal positions/rotations. It is responsible for:
        /// - Initializing all values for update
        /// - Handling MovePosition calls
        /// - Solving initial collision overlaps
        /// - Ground probing
        /// - Handle detecting potential interactable rigidbodies
        /// </summary>
        void UpdatePhase1(float deltaTime);


        /// <summary>
        /// Update phase 2 is meant to be called after physics movers have simulated their goal positions/rotations. 
        /// At the end of this, the TransientPosition/Rotation values will be up-to-date with where the motor should be at the end of its move. 
        /// It is responsible for:
        /// - Solving Rotation
        /// - Handle MoveRotation calls
        /// - Solving potential attached rigidbody overlaps
        /// - Solving ModuleVelocity
        /// - Applying planar constraint
        /// </summary>
        void UpdatePhase2(float deltaTime);


        /// <summary>
        /// Returns the direction adjusted to be tangent to a specified surface normal relatively to the character's up direction.
        /// Useful for reorienting a direction on a slope without any lateral deviation in trajectory
        /// </summary>
        Vector3 GetDirectionTangentToSurface(Vector3 direction, Vector3 surfaceNormal);

        Rigidbody AttachedRigidbody { get; }

        Vector3 BaseVelocity { get; set; }
        Vector3 Velocity { get; }


        /// <summary>
        /// Sets whether or not the motor will solve collisions when moving (or moved onto)
        /// </summary>
        void SetMovementCollisionsSolvingActivation(bool movementCollisionsSolvingActive);
    }
}