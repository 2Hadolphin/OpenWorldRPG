using Return.Physical;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using Return.Humanoid.Animation;
using Return.Modular;

namespace Return.Humanoid.Character
{
    /// <summary>
    /// Humanoid ragdoll builder
    /// </summary>
    public class RagdollSystem : ModuleHandler, IInjectable
    {
        #region Resolver

        public override void InstallResolver(IModularResolver resolver)
        {

        }


        #endregion


        [Inject]
        protected Rigidbody RB;

        [Inject(Optional =true)]
        protected virtual void OnAnimatorInject(Animator animator)
        {
            PrepareRagdoll(animator);
        }

        private void Start()
        {
            test = RegisterCollisionWrapper(DefaultRagdollBones);
            test.RegisterCollisionEvent(CollisionThrede.Enter, 5);
            test.SensorMask = LayerMask.GetMask("Character");
            test.OnCollisionEnter += Test_OnCollisionEnter;
        }

        private void Test_OnCollisionEnter(object sender, ContactsArg e)
        {
            Debug.Log(e.Contacts[0].otherCollider);
        }

        CollisionWrapper test;
        #region Collision

        public Dictionary<HumanBodyBones, Collider> BodyColliders = new Dictionary<HumanBodyBones, Collider>();

        /// <summary>
        /// Childs collider layer as ignore each while default mode, change to ragdoll layer while require ragdoll function
        /// </summary>
        public HashSet<Collider> ColliderGroup = new HashSet<Collider>();

        public List<CollisionWrapper> CollisionAdapter = new List<CollisionWrapper>();

        /// <summary>
        /// RegisterHandler body bones collision event
        /// </summary>
        public virtual CollisionWrapper RegisterCollisionWrapper(params HumanBodyBones[] limbs)
        {
            var cols = new List<Collider>(limbs.Length);
            foreach (var limb in limbs)
            {
                if (BodyColliders.TryGetValue(limb, out var col))
                    cols.Add(col);
                else
                    Debug.LogWarning(string.Format("Can't found any collider attach on {0}. ", limb));
            }
            var wrapper = new CollisionWrapper(cols.ToArray());
            CollisionAdapter.Add(wrapper);
            return wrapper;
        }

     
        private void OnCollisionEnter(Collision collision)
        {
            foreach (var adapter in CollisionAdapter)
            {
                if (!adapter.Enter)
                    continue;

                adapter.Contact(collision, CollisionThrede.Enter);
            }
        }
        private void OnCollisionStay(Collision collision)
        {
            foreach (var adapter in CollisionAdapter)
            {
                if (!adapter.Enter)
                    continue;

                adapter.Contact(collision, CollisionThrede.Enter);
            }
        }
        private void OnCollisionExit(Collision collision)
        {
            foreach (var adapter in CollisionAdapter)
            {
                if (!adapter.Enter)
                    continue;

                adapter.Contact(collision, CollisionThrede.Enter);
            }
        }



        #endregion



        #region Ragdoll

        Dictionary<HumanBodyBones, mJoint> Joints;

        #region Parameter
        /// <summary>
        /// Time to destory rigidbody and joints
        /// </summary>
        public float DestoryTime;

        public float totalMass = 20;
        public float strength = 0.0F;

        public float HeadRadius = 0.15f;
        public float ArmRadius = 0.065f;
        public float LegRadius = 0.085f;
        public bool flipForward = false;
        #endregion


        #region RagdollCollider

        protected virtual void BuildCollider()
        {
            //var layer = ConstCache.PhysicSetting.CharacterBody;

            Vector3 worldUp = Joints[HumanBodyBones.Hips].tf.up;
            #region Run Time Build Body Collider

            #region Build Limb Colliders <Capsule>
            foreach (var mjoint in Joints)
            {
                var joint = mjoint.Value;

                if (joint.data is CapsuleJoint capsule)
                {
                    PrepareCapsuleJointData(joint, capsule);

                    #region build col
                    var col = joint.tf.gameObject.AddComponent<CapsuleCollider>();
                    
                    col.direction = capsule.Direction.Parse();
                    col.center = capsule.Center;
                    col.height = capsule.Height;
                    col.radius = capsule.Radius;
                    BodyColliders.Add(joint.data.bone, col);
                    #endregion
                }
            }
            #endregion

            #region Build Spine Collider <Box>

            {
                if (Joints.TryGetValue(HumanBodyBones.Hips, out var joint) && joint.data is BoxJoint box)
                {
                    var col = joint.tf.gameObject.AddComponent<BoxCollider>();
                    col.center = box.Bounds.center;
                    col.size = box.Bounds.size;
                    BodyColliders.Add(joint.data.bone, col);
                }
            }

            var middle = HumanBodyBones.UpperChest;
            while (!Joints.ContainsKey(middle) && middle.Parse() > 0)
                middle = middle.GetParent();

            {
                if (Joints.TryGetValue(middle, out var joint) && joint.data is BoxJoint box)
                {
                    var col = joint.tf.gameObject.AddComponent<BoxCollider>();
                    col.center = box.Bounds.center;
                    col.size = box.Bounds.size;
                    BodyColliders.Add(joint.data.bone, col);
                }
            }
            #endregion 

            #region Build Head Colliders <Sphere>
            {
                if (Joints.TryGetValue(HumanBodyBones.Head, out var joint) && joint.data is SphereJoint sphere)
                {
                    var col = joint.tf.gameObject.AddComponent<SphereCollider>();
                    col.center = sphere.Center;
                    col.radius = sphere.Radius;
                    BodyColliders.Add(joint.data.bone, col);
                }
            }
            #endregion
            #endregion
        }

        void PrepareCapsuleJointData(mJoint joint, CapsuleJoint data)
        {
            int direction;
            float distance;
            if (joint.children.Count == 1)
            {
                mJoint childBone = joint.children[0];
                Vector3 endPoint = childBone.tf.position;
                joint.tf.InverseTransformPoint(endPoint).CalculateDirection(out direction, out distance);
            }
            else
            {
                Vector3 endPoint = (joint.tf.position - Joints[joint.data.parent].tf.position) + joint.tf.position;
                joint.tf.InverseTransformPoint(endPoint).CalculateDirection(out direction, out distance);

                if (joint.tf.GetComponentsInChildren<Transform>().Length > 1)
                {
                    Bounds bounds = new Bounds();
                    foreach (Transform child in joint.tf.GetComponentsInChildren<Transform>())
                    {
                        bounds.Encapsulate(joint.tf.InverseTransformPoint(child.position));
                    }

                    if (distance > 0)
                        distance = bounds.max[direction];
                    else
                        distance = bounds.min[direction];
                }
            }

            data.Direction = (Axis)direction;

            Vector3 center = Vector3.zero;
            center[direction] = distance * 0.5F;
            data.Center = center;

            data.Height = Mathf.Abs(distance);

            if (4 < (int)(joint.data.bone))
                data.Radius = ArmRadius;
            else
                data.Radius = LegRadius;
        }

        #endregion

        #region Rigidbody

        public float Drag;
        Dictionary<mJoint, Rigidbody> RagdollRig ;

        protected virtual void BuildRigidbodies()
        {
            if (RagdollRig == null)
                RagdollRig = new Dictionary<mJoint, Rigidbody>(Joints.Count);
            else
                RagdollRig.Clear();

            foreach (var pair in Joints)
            {
                var joint = pair.Value;
                var rig = joint.tf.gameObject.InstanceIfNull<Rigidbody>();
                rig.mass = joint.data.density;
                rig.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rig.drag = Drag;
                rig.solverIterations = 10;
                rig.solverVelocityIterations = 10;
                RagdollRig.Add(joint, rig);
            }
            RagdollRig.TrimExcess();

            if(Joints.TryGetValue(HumanBodyBones.Hips, out var rootJoint))

            #region CalculateMass
            {
                // Calculate allChildMass by summing all bodies
                CalculateMassRecurse(rootJoint);

                // Rescale the mass so that the whole character weights totalMass
                float massScale = totalMass / rootJoint.data.summedMass;
                foreach (var bone in RagdollRig)
                {
                    bone.Value.mass *= massScale;
                }

                // Recalculate allChildMass by summing all bodies
                CalculateMassRecurse(rootJoint);
            }
            #endregion
        }

        void CalculateMassRecurse(mJoint bone)
        {
            float mass = RagdollRig[bone].mass;
            foreach (mJoint child in bone.children)
            {
                CalculateMassRecurse(child);
                mass += child.data.summedMass;
            }
            bone.data.summedMass = mass;
        }

        protected virtual void CleanRigidbodies()
        {
            if (RagdollRig != null)
            {
                foreach (var pair in RagdollRig)
                {
                    Destroy(pair.Value, DestoryTime);
                }

                RagdollRig.Clear();
            }
        }

        #endregion

        #region Joints
        Dictionary<mJoint, CharacterJoint> RagdollJoint;

        protected virtual void BuildJoints()
        {
            if (RagdollJoint == null)
                RagdollJoint = new Dictionary<mJoint, CharacterJoint>(Joints.Count);
            else
                RagdollJoint.Clear();

            foreach (var pair in Joints)
            {
                var mjoint = pair.Value;

                if(!Joints.ContainsKey(mjoint.data.parent))
                    continue;

                var joint = mjoint.tf.gameObject.InstanceIfNull<CharacterJoint>();
                RagdollJoint.Add(mjoint, joint);


                var data = mjoint.data;

                // RegisterHandler connection and axis
                joint.axis = mjoint.tf.InverseTransformDirection(data.axis).CalculateDirectionAxis();
                joint.swingAxis = mjoint.tf.InverseTransformDirection(data.normalAxis).CalculateDirectionAxis();
                joint.anchor = Vector3.zero;
                joint.connectedBody = RagdollRig[Joints[mjoint.data.parent]];
                joint.enablePreprocessing = false; // turn off to handle degenerated scenarios, like spawning inside geometry.

                // RegisterHandler limits
                var limit = new SoftJointLimit
                {
                    contactDistance = 0, // default to zero, which automatically sets contact distance.
                    limit = data.minLimit
                };

                joint.lowTwistLimit = limit;

                limit.limit = data.maxLimit;
                joint.highTwistLimit = limit;

                limit.limit = data.swingLimit;
                joint.swing1Limit = limit;

                limit.limit = 0;
                joint.swing2Limit = limit;
            }
            RagdollJoint.TrimExcess();
        }

        protected virtual void CleanJoints()
        {
            if (RagdollJoint != null)
            {
                foreach (var pair in RagdollJoint)
                {
                    Destroy(pair.Value, DestoryTime);
                }

                RagdollJoint.Clear();
            }
        }
        #endregion

        [Button("TestEvent")]
        void TestBuild(Animator animator)
        {
            test = RegisterCollisionWrapper(DefaultRagdollBones);
            test.RegisterCollisionEvent(CollisionThrede.Enter, 5);
            test.SensorMask = LayerMask.GetMask("Character");
            test.OnCollisionEnter += Test_OnCollisionEnter;
        }

        [Button("CleanRagdoll")]
        public void CleanRagdoll()
        {
            CleanJoints();

            CleanRigidbodies();
        }



        [Button("PrepareRagdoll")]
        protected virtual void PrepareRagdoll(Animator animator)
        {
            _BuildRagdollData(animator, flipForward, out Joints);

            BuildCollider();
        }

        [Button("ActivateRagdoll")]
        public void BuildRagdoll()
        {
            BuildRigidbodies();

            BuildJoints();
        }

        /// <summary>
        /// Generate ragdoll preset ->bone & detail data
        /// </summary>
        public static void BuildRagdollData(Animator animator, bool flipForward, out HumanJointInfo[] joints)
        {
            _BuildRagdollData(animator, flipForward, out var mjoints);
            joints = mjoints.Select(x => x.Value.data).ToArray();
        }

        static void _BuildRagdollData(Animator animator, bool flipForward, out Dictionary<HumanBodyBones, mJoint> Joints)
        {
            var ragdollBones = DefaultRagdollBones;
            Joints = new Dictionary<HumanBodyBones, mJoint>(ragdollBones.Length);

            #region BindBone


            #region Create mJoint
            foreach (var bone in ragdollBones)
            {
                var transform = animator.GetBoneTransform(bone);
                if (transform)
                {
                    var newJoint = new mJoint() { tf = transform };
                    Joints.Add(bone, newJoint);
                }
            }

            #endregion

            var hips = Joints[HumanBodyBones.Hips].tf;
            var head = Joints[HumanBodyBones.Head].tf;
            var rightElbow = Joints[HumanBodyBones.RightLowerArm].tf;

            #endregion


            #region CalculateAxes(Adjust)

            var right = Vector3.right;
            var up = Vector3.up;
            var forward = Vector3.forward;

            var worldRight = Vector3.right;
            var worldUp = Vector3.up;
            var worldForward = Vector3.forward;

            if (hips)
            {
                worldRight = hips.TransformDirection(right);
                worldUp = hips.TransformDirection(up);
                worldForward = hips.TransformDirection(forward);
            }

            if (head != null && hips != null)
                up = hips.InverseTransformPoint(head.position).CalculateDirectionAxis();

            if (rightElbow != null && hips != null)
            {
                Vector3 temp;
                mVectorUtility.DecomposeVector(out temp, out Vector3 removed, hips.InverseTransformPoint(rightElbow.position), up);
                right = removed.CalculateDirectionAxis();
            }

            forward = Vector3.Cross(right, up);
            if (flipForward)
                forward = -forward;

            #endregion


            #region BindJoint
            if(Joints.TryGetValue(HumanBodyBones.Hips, out var rootJoint))
            {
                rootJoint.data = new BoxJoint
                {
                    parent = HumanBodyBones.LastBone,
                    density = 2.5F
                };

            }

            var middle = //mjoints.ContainsKey(HumanBodyBones.UpperChest) ? HumanBodyBones.UpperChest:
                Joints.ContainsKey(HumanBodyBones.Chest) ? HumanBodyBones.Chest : HumanBodyBones.Spine;

            Assert.IsTrue(Joints.ContainsKey(middle)) ;



            AddRagdollJoint(Joints, ColliderType.Capsule, HumanBodyBones.LeftUpperLeg, HumanBodyBones.Hips, worldRight, worldForward, -20, 70, 30, 0.3F, 1.5F);
            AddRagdollJoint(Joints, ColliderType.Capsule, HumanBodyBones.RightUpperLeg, HumanBodyBones.Hips, worldRight, worldForward, -20, 70, 30, 0.3F, 1.5F);

            AddRagdollJoint(Joints, ColliderType.Capsule, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftUpperLeg, worldRight, worldForward, -80, 0, 0, 0.25F, 1.5F);
            AddRagdollJoint(Joints, ColliderType.Capsule, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightUpperLeg, worldRight, worldForward, -80, 0, 0, 0.25F, 1.5F);

            AddRagdollJoint(Joints, ColliderType.Box, middle, HumanBodyBones.Hips, worldRight, worldForward, -20, 20, 10, 1, 2.5F);


            AddRagdollJoint(Joints, ColliderType.Capsule, HumanBodyBones.LeftUpperArm, middle, worldUp, worldForward, -70, 10, 50, 0.25F, 1.0F);
            AddRagdollJoint(Joints, ColliderType.Capsule, HumanBodyBones.RightUpperArm, middle, worldUp, worldForward, -70, 10, 50, 0.25F, 1.0F);

            AddRagdollJoint(Joints, ColliderType.Capsule, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftUpperArm, worldUp, worldForward, -90, 0, 0, 0.20F, 1.0F);
            AddRagdollJoint(Joints, ColliderType.Capsule, HumanBodyBones.RightLowerArm, HumanBodyBones.RightUpperArm, worldUp, worldForward, -90, 0, 0, 0.20F, 1.0F);

            AddRagdollJoint(Joints, ColliderType.Sphere, HumanBodyBones.Head, middle, worldRight, worldForward, -40, 25, 25, 1, 1.0F);
            #endregion


            #region Build Spine Colliders <Box>

            var _hips = Joints[HumanBodyBones.Hips].tf;

 
            var spine = Joints[middle].tf;

            var _leftUpperLeg = Joints[HumanBodyBones.LeftUpperLeg].tf;
            var _rightUpperLeg = Joints[HumanBodyBones.RightUpperLeg].tf;
            var _leftArm = Joints[HumanBodyBones.LeftUpperArm].tf;
            var _rightArm = Joints[HumanBodyBones.RightUpperArm].tf;
            var _neck = Joints[HumanBodyBones.Neck].tf;

            // Middle spine and hips
            if (spine != null && _hips != null)
            {
                {
                    // Middle spine bounds
                    var bounds = Clip(GetBreastBounds(_hips, _leftUpperLeg, _rightUpperLeg, _leftArm, _rightArm), worldUp, _hips, spine, false);

                    if (Joints.TryGetValue(HumanBodyBones.Hips, out var joint) && joint.data is BoxJoint box)
                    {
                        box.Bounds = bounds;
                        box.Center = bounds.center;
                    }
                }

                {
                    var bounds = Clip(GetBreastBounds(spine,_neck ,_leftUpperLeg, _rightUpperLeg, _leftArm, _rightArm), worldUp, spine, spine, true);

                    if (Joints.TryGetValue(middle, out var joint) && joint.data is BoxJoint box)
                    {
                        box.Bounds = bounds;
                        box.Center = bounds.center;
                    }
                }

            }
            else            // Only hips
            {
                var bounds = new Bounds();
                bounds.Encapsulate(_hips.InverseTransformPoint(_leftUpperLeg.position));
                bounds.Encapsulate(_hips.InverseTransformPoint(_rightUpperLeg.position));
                bounds.Encapsulate(_hips.InverseTransformPoint(_leftArm.position));
                bounds.Encapsulate(_hips.InverseTransformPoint(_rightArm.position));
                bounds.Encapsulate(_hips.InverseTransformPoint(_neck.position));

                Vector3 size = bounds.size;
                size[bounds.size.SmallestComponent()] = size[bounds.size.LargestComponent()] / 2.0F;

                if (Joints.TryGetValue(HumanBodyBones.Hips, out var joint) && joint.data is BoxJoint box)
                {
                    box.Bounds = bounds;
                    box.Center = bounds.center;
                }
            }

            #endregion

            #region Build Head Colliders <Sphere>
            {
                if (Joints.TryGetValue(HumanBodyBones.Head, out var joint) && joint.data is SphereJoint sphere)
                {
                    float radius = Vector3.Distance(_leftArm.position, _rightArm.position);
                    radius *= 0.35f;
                    var center = Vector3.zero;


                    //joint.tf.InverseTransformPoint(_hips.position).CalculateDirection(out var direction, out var distance);

                    //if (distance > 0)
                    //    center[direction] = -radius;
                    //else
                    //    center[direction] = radius;

                    sphere.Radius = radius;
                    sphere.Center = joint.tf.InverseTransformPoint(_neck.position).Multiply(-0.25f);//=center
                }
            }
            #endregion

            var empty = Joints.Where(x => x.Value.data == null).Select(x => x.Key).ToList();
            foreach (var key in empty)
            {
                Joints.Remove(key);
                Debug.Log(key + " is not match ragdoll system.");
            }

            foreach (var joint in Joints)
            {
                if (joint.Value.data != null)
                    BindParent(Joints, joint.Value);
            }
        }



        /// <summary>
        /// Self collider
        /// </summary>
        public HashSet<Collider> IgnoreGroup = new HashSet<Collider>();


        /// <summary>
        /// Basic ragdoll element during runtime
        /// </summary>
        class mJoint
        {
            public HumanJointInfo data;
            public Transform tf;
            public List<mJoint> children = new();
        }


        static void AddRagdollJoint(IDictionary<HumanBodyBones, mJoint> dic, ColliderType type, HumanBodyBones joint, HumanBodyBones parent, Vector3 worldTwistAxis, Vector3 worldSwingAxis, float minLimit, float maxLimit, float swingLimit, float scale, float density)
        {
            var mJoint = dic[joint];
            HumanJointInfo data = type switch
            {
                ColliderType.Sphere => new SphereJoint(),
                ColliderType.Capsule => new CapsuleJoint(),
                ColliderType.Box => new BoxJoint(),
                _ => throw new NotImplementedException(type.ToString())
            };
            data.bone = joint;
            data.axis = worldTwistAxis;
            data.normalAxis = worldSwingAxis;
            data.minLimit = minLimit;
            data.maxLimit = maxLimit;
            data.swingLimit = swingLimit;
            data.density = density;
            data.BoundsScale = scale;
            data.parent = parent;

            mJoint.data = data;
        }

        static void BindParent(IDictionary<HumanBodyBones, mJoint> dic,mJoint joint)
        {
            if(dic.TryGetValue(joint.data.parent,out var parentJoint))
                parentJoint.children.Add(joint);
        }

        public static Bounds Clip(Bounds bounds, Vector3 worldUp, Transform relativeTo, Transform clipTransform, bool below)
        {
            int axis = bounds.size.LargestComponent();

            if (Vector3.Dot(worldUp, relativeTo.TransformPoint(bounds.max)) > Vector3.Dot(worldUp, relativeTo.TransformPoint(bounds.min)) == below)
            {
                Vector3 min = bounds.min;
                min[axis] = relativeTo.InverseTransformPoint(clipTransform.position)[axis];
                bounds.min = min;
            }
            else
            {
                Vector3 max = bounds.max;
                max[axis] = relativeTo.InverseTransformPoint(clipTransform.position)[axis];
                bounds.max = max;
            }
            return bounds;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points">leftUpperLeg/rightUpperLeg/leftArm/rightArm</param>
        /// <returns></returns>
        public static Bounds GetBreastBounds(Transform relativeTo, params Transform[] points)
        {
            // Pelvis bounds
            Bounds bounds = new Bounds();

            foreach (var point in points)
                bounds.Encapsulate(relativeTo.InverseTransformPoint(point.position));

            Vector3 size = bounds.size;
            size[bounds.size.SmallestComponent()] = size[bounds.size.LargestComponent()] / 2.5f;// / points.Length;//4.0F;
            bounds.size = size;
            return bounds;
        }

        public static HumanBodyBones[] DefaultRagdollBones
        {
            get => new[]
            {
                    HumanBodyBones.Hips,

                    HumanBodyBones.LeftUpperLeg,
                    HumanBodyBones.LeftLowerLeg,
                    HumanBodyBones.RightUpperLeg,
                    HumanBodyBones.RightLowerLeg,

                    HumanBodyBones.Neck,
                    //HumanBodyBones.UpperChest,
                    HumanBodyBones.Chest,
                    HumanBodyBones.Spine,

                    HumanBodyBones.RightUpperArm,
                    HumanBodyBones.RightLowerArm,
                    HumanBodyBones.LeftUpperArm,
                    HumanBodyBones.LeftLowerArm,

                    HumanBodyBones.Head,

            };
        }
        #endregion
    }
}
