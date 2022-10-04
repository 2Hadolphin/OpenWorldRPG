using System.Collections.Generic;
using UnityEngine;
using System;
using Return.Items;
using Sirenix.OdinInspector;
using UnityEngine.Assertions;
using Return.Equipment.Slot;
using Return.Preference;
using Return.Agents;
using Return.InteractSystem;
using Cysharp.Threading.Tasks;
using System.Collections;
using Return.Cameras;
using System.Linq;
using Return;
using Return.Animations;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using Return.Inventory;
using Return.Modular;
using Return.Motions;

namespace Return
{

    /// Local user ? Nets version(ignore)? Agent?
    /// <summary>
    /// Control system of hands, manage IK motion (**ForwardTo ). **m_items <= Interactor <= Inventory & scene
    /// **Item **Vehicle **Agent **
    /// </summary>
    public partial class HumanoidInteractSystem : BaseInteractSystem, IHumanoidInteractSystem
    {


        #region Agent

        [Inject]
        protected IAgent Agent;

        #endregion

        #region IInteractSystem

        public override async UniTask<bool> Interact(object obj)
        {
            bool valid = true;

            try
            {
                if (obj is IPickup pickup)
                    valid = await PickUp(pickup);
                else if (obj is IInteractHandle interactable)
                    interactable.Interact(Agent,this);
                else
                    valid = false;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                valid = false;
            }

            return valid;
        }

        
        public override event Action<InteractWrapper> OnInteractMission;
        public override event Action<InteractWrapper> OnCancelMission;

        /// <summary>
        /// Interact with target use by internal handle.
        /// </summary>
        protected virtual void Interact(InteractWrapper wrapper)
        {
            OnInteractMission?.Invoke(wrapper);

            // pass interactable to handler(this)
            _ = Interact(wrapper.Interactable);
        }

        /// <summary>
        /// Cancel interact target.
        /// </summary>
        protected virtual void CancelInteract(InteractWrapper wrapper)
        {
            OnCancelMission?.Invoke(wrapper);
        }

        #endregion

        #region IInventory

        [Inject(Optional =true)]
        protected IInventoryManager InventoryHandler;

        #endregion

        #region Interact Targets

        /// <summary>
        /// Interacting objects.(include hands)
        /// </summary>
        [ShowInInspector]
        protected mStack<object> Interactables = new(2);

        #endregion


        #region Slot

        [ShowInInspector]
        [NonSerialized]
        protected Slot[] Hands;

        #endregion


        #region IModule

        public override void InstallResolver(IModularResolver resolver)
        {
            resolver.RegisterModule<IInteractHandler>(this);
            //Resolver.RegisterModule<IHumanoidInteractSystem>(this);
        }

        protected override void Register()
        {
            base.Register();

            //yield return UniTask.Delay(1).ToCoroutine();

            #region Preference

            //SettingsManager.GamePlayUpdate.performed += LoadGamePlay;
            LoadGamePlay(SettingsManager.Instance);

            #endregion

            if (Agent.Resolver.TryGetModule(out IHumanoidMotionSystem motions))
            {
                //Init();

                // obsolete
                //var preset = Resources.Load<InteractorPreset>(MotionModulePresetPath);
                //Assert.IsNotNull(preset);
                MotionHandle = MotionPreset.Create(gameObject) as Interactor;

                motions.Resolver.Inject(MotionHandle);
                //motionSystem.InstallModule(MotionHandle);
            }
        }

        protected override void Unregister()
        {
            base.Unregister();

            if (MotionHandle)
                MotionHandle.Dispose();
        }

        protected override void Activate()
        {
            base.Activate();

            try
            {
                if (!Resolver.TryGetModule<Animator>(out var animator))
                    throw new KeyNotFoundException($"Failure to get animator : {animator}.");

                #region Bind Interactor

                Hand.Clear();

                var list = new List<Slot>(2);

                // human hands
                {
                    var rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand).Find(HandleOption.HandleSlot_RightHand);
                    if (rightHand)
                    {
                        var slot = rightHand.InstanceIfNull<Slot>();
                        list.Add(slot);
                        var right = Symmetry.Right;
                        Hand.Add((int)right, new Schedules(slot, right));
                    }

                    var leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand).Find(HandleOption.HandleSlot_LeftHand);
                    if (leftHand)
                    {
                        var slot = leftHand.InstanceIfNull<Slot>();
                        list.Add(slot);
                        var left = Symmetry.Left;
                        Hand.Add((int)left, new Schedules(slot, left));
                    }
                }


                //Debug.Log(list.Count);

                Hands = list.ToArray();

                var length = Hands.Length;

                //if (length > 0)
                //    Hand.Add((int)Prime, new Schedules(Hands[0], Prime));

                //if (length > 1)
                //    Hand.Add((int)Secondary, new Schedules(Hands[1], Secondary));

                for (int i = 2; i < length; i++)
                {
                    Hand.Add(i, new Schedules(Hands[i], Symmetry.Deny));
                }

                #endregion

                if (Agent.IsLocalUser())
                {
                    // add pointer
                    var pointer = InstanceIfNull<UserSelectionPointer>();
                    // set selection target
                    pointer.TargetTypes.Add(typeof(IInteractHandle));
                    // set pointer ignore
                    pointer.IgnoreColliders.AddRange(Agent.transform.GetComponentsInChildren<Collider>());


                    // add vfx
                    var vfx = InstanceIfNull<SelectionVFX>();
                    vfx.SetHandler(pointer);


                    // add UGUI
                    var ugui = InstanceIfNull<SelectionUGUI>();
                    ugui.SetHandler(pointer);


                    // user input
                    if (!Agent.Resolver.TryGetModule<UserInputHandle>(out var input))
                    {
                        input = new UserInputHandle();
                        input.RegisterInput(InputManager.Input);
                        input.SetHandler(this);
                        input.enabled = true;
                        input.SetHandler(pointer);

                        //player.RegisterSystem(input);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                Routine.OnUpdateLast.Subscribe(Tick);
            }
        }

        protected override void Deactivate()
        {
            Routine.OnUpdateLast.Unsubscribe(Tick);

            base.Deactivate();
        }


        #endregion

        #region Tick

        public virtual void Tick()
        {
            // Analyze the character's target
            //SearchForWeapons();
            //SearchForAmmo();
            //SearchForAdrenaline();
            //SearchInteractiveObjects();

            var delta = ConstCache.deltaTime;

            Slove(Prime, delta);
            Slove(Secondary, delta);
        }

        #endregion


        #region Motions **IK

        [SerializeField]
        InteractorPreset MotionPreset;

        [ShowInInspector]
        protected Interactor MotionHandle;

        #endregion


        #region Controll Parameter

        /// <summary>
        /// Schedule pair cache.
        /// </summary>
        protected Dictionary<int, Schedules> Hand = new(2);

        [SerializeField, ReadOnly]
        protected Symmetry Prime;
        [SerializeField, ReadOnly]
        protected Symmetry Secondary;

        #endregion



        #region Main Fuc **Evalute handle(chose) => SetMission => 

        /// <summary>
        /// Get free interactor
        /// </summary>
        protected Schedules GetHand(Symmetry enableHand)
        {
            if (Hand.TryGetValue((int)enableHand, out var stroke))
                return stroke;
            else if (Hand.TryGetValue((int)enableHand.Mirror(), out stroke))
                return stroke;

            Debug.LogError(enableHand);
            throw new KeyNotFoundException();
        }


        public async UniTask<bool> CleanInteractor()
        {
            foreach (var hand in Hand.Values)
            {
                if (hand.IsFree)
                    continue;

                hand.Item.Deactivate();

                await UniTask.Delay(TimeSpan.FromSeconds(1f));
            }

            return true;
        }

        /// <summary>
        /// Evalute position and return closest handle.
        /// </summary>
        public virtual Symmetry Evalute(ICoordinate coordinates)
        {
            var pos = coordinates.position;

            var min = float.MaxValue;

            Schedules closest=null;

            foreach (var hand in Hand.Values)
            {
                 var distance=Vector3.SqrMagnitude(pos- hand.Slot.Transform.position);

                if(distance<min)
                {
                    closest = hand;
                    min = distance;
                }    
            }

            //var left = MotionSystem.GetBoneInterface(HumanBodyBones.RightHand).position;
            //var left = MotionSystem.GetBoneInterface(HumanBodyBones.LeftHand).position;

            //var side = (left - pos).sqrMagnitude < (left - pos).sqrMagnitude ? Symmetry.Right : Symmetry.Left;

            return closest==null?Symmetry.Deny:closest.Side;
        }

        public bool ValidMission(Symmetry requireHand,out Symmetry validHandle)
        {
            validHandle = MotionHandle.CheckHandle(requireHand);
            return requireHand==validHandle;
        }


        /// <summary>
        /// CheckAdd hand and enable handle.
        /// </summary>
        public InteractAdapter SetupMission(object sender, Symmetry requireHand)
        {
            var adapter = new InteractAdapter(sender, requireHand);

            MotionHandle.RegisterAdapter(adapter);

            return adapter;
        }



        #endregion


        #region Stroke Routine

        /// <param name="schedule"> sender </param>
        /// <param name="dependent"> targeted plan </param>
        private bool CheckDependent(Schedules sender, Plan dependent)
        {
            var plan = sender.Plans.Peek();
            switch (plan.DependType)
            {
                case Set.Intersection:
                    #region Intersection
                    {
                        foreach (var data in Hand)
                        {
                            var schedule = data.Value;
                            if (schedule == sender)
                                continue;

                            if (schedule.Plans.Contains(dependent))
                            {
                                if (schedule.Plans.Peek() == dependent)
                                    return true;

                                if (sender.Plans.Count > 1)
                                {
                                    dependent = sender.Plans.Dequeue();
                                    sender.Plans.Enqueue(dependent);
                                }
                            }
                            else
                            {
                                sender.Plans.Dequeue();
                            }
                        }
                        return false;
                    }
                #endregion

                case Set.Union:
                    return true;

                case Set.Subtraction:
                    #region Subtraction
                    {
                        foreach (var data in Hand)
                        {
                            var schedule = data.Value;
                            if (schedule == sender)
                                continue;

                            if (schedule.Plans.Contains(dependent))
                                return false;
                        }
                        return true;
                    }
                #endregion

                default:
                    throw new KeyNotFoundException();
            }
        }

        /// <summary>
        ///  Calculate stroke progress and apply ik motion.
        /// </summary>
        protected virtual void Slove(Symmetry actor, float delta)
        {
            var hand = GetHand(actor);

            if (hand.Slove(delta, out var ratio, out var dependent))
                FinishSchedule(hand);
            else if (ratio != default)
                if (dependent == null || CheckDependent(hand, dependent))
                    SloveMission(hand, ratio);
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void SloveMission(Schedules schedule, float ratio)
        {



        }

        #endregion


        #region Analyze Schedule  ** Main Action

        // local user camera handle
        /// <summary>
        /// Release 
        /// **Discard pickupable => deactivate item
        /// **Deactivate ridable => get off vehicle
        /// **Cancel interacting object => cancel interact
        /// ik anim Schedule_Drop
        /// </summary>
        public override void Release()
        {
            using var loop = Interactables.CacheLoop();


            foreach (var obj in loop)
            {
                _ = ReleaseTask(obj);
            }

            //foreach (var hand in Hand.Values)
            //{
            //    if (hand.IsFinalFree)
            //        continue;

            //    if (!hand.IsEmpty)
            //        hand.m_items?.Deactivate();
            //}
        }

        /// <summary>
        /// Discard interacting objects.
        /// </summary>
        protected virtual async UniTask ReleaseTask(object obj)
        {
            try
            {
                if (obj is IPickup pickup)
                {

                    Debug.Log(obj + nameof(pickup.Deactivate));
                    await pickup.Deactivate();


                    // check storage or drop
                    if (InventoryHandler != null)
                    {
                        //

                        if (InventoryHandler.CheckStore(pickup))
                        {
                            pickup.Unregister(Agent);

                            InventoryHandler.Store(pickup);
                        }
                        else // can't storage
                        {
                            DropPickup(pickup);
                        }
       
                    }
                    else
                    {
                        // drop
                        // push Schedule_Drop ik schedule
                        if (false)
                        {
                            Schedule_Drop(null);
                        }


                        DropPickup(pickup);
                    }


                    if (obj is Component c && c != null)
                    {
                        _ = GizmosUtil.DrawWireCube(c.transform.position, new Vector3(0.2f, 0.2f, 0.2f), Color.white).Wait(15);

                        var pos = c.transform.position;

                        if (TryGetComponent<RigBuilder>(out var rig))
                            rig.Clear();

                        var animator = GetComponent<Animator>();
                        animator.UnbindAllStreamHandles();

                        if (rig != null)
                            rig.Build();
            
                        c.transform.SetParent(null,true);

                        Debug.Log($"Ineractor {Time.frameCount} : {pos} to {transform.position}");
                    }


                }
                else if (obj is IInteractHandle interactable)
                {
                    interactable.Cancel(Agent, this);
                }
                else if (false)// rideable
                {

                }
                else if (obj is IDisposable disposable)
                {
                    disposable.Dispose();
                    Debug.LogWarning($"Not recommand using disposable to handle interactable object : {obj}");
                }
                else
                {
                    throw new NotImplementedException($"Missing stop interact function {obj}");
                }
            }
            catch (Exception e)
            {

                Debug.LogException(e);
            }
            finally
            {
                Interactables.Remove(obj);
            }


            // valid interable remove from hands
            try
            {
                if (obj is IItemModule item)
                    item.UnRegister();

                if (obj is Component c)
                    Assert.IsTrue(true);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Pick up object and set operation. 
        /// **Valid selector handler =>(empty hands)
        /// **Valid inventory storage =>(storage preference and space limit)
        /// **Other system (Quest???)
        /// </summary>
        public async UniTask<bool> PickUp(IPickup pickup, object plan = null)
        {
            // valid interacting targets contain this new object
            if (Interactables.Contains(pickup))
            {
                Debug.LogErrorFormat("{0} already exist in interact system.", pickup);
                return false;
            }

            var actor = TakeOver();

            // no hand
            if (actor==Symmetry.Deny)
            {
                Debug.LogError($"Failure to pick up because interactor {actor}.");
                return false;
            }

            // check play performer (in user view)??
            await UniTask.NextFrame();

 
            // valid inventory
            if (InventoryHandler != null)
            {
                // valid preference storage or use
                if (false)
                {

                }

                if(InventoryHandler.CheckStore(pickup))
                {
                    // go storage?
                }
            }

            // Pickup
            await UniTask.Delay(TimeSpan.FromSeconds(0.3f));

            // do animation and sensor target place
            if(false)//valid ik
            {

            }

            try
            {
                Debug.LogFormat("Pick up {0}.", pickup);

                pickup.Register(Agent);

                await pickup.Activate();

                Assert.IsFalse(Interactables.Contains(pickup));

                var add = Interactables.CheckAdd(pickup);

            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }


            return true;

            //var handles =obj.GetHandles(out var prime, out var second);

            //if (handles == 0)
            //{
            //    var plan = new Plan() { StrokeType = StrokeType.Pickup, Target = obj.Preset, TargetCoordinate = obj.Grab() };
            //    GetHand(actor).Plans.Enqueue(plan);
            //}
            //else
            //{
            //    var plan = new Plan() { StrokeType = StrokeType.Pickup, Target = obj.Preset, TargetCoordinate = prime };
            //    if (prime != null)
            //    {

            //        GetHand(actor).Plans.Enqueue(plan);
            //    }

            //    if (second != null)
            //    {
            //        var supPlan = new Plan() { StrokeType = StrokeType.Support, Target = obj.Preset, TargetCoordinate = second,Dependent=plan };
            //        GetHand(Mirror(actor)).Plans.Enqueue(supPlan);
            //    }
            //}
        }



        /// <summary>
        /// Takeout obj from container
        /// </summary>
        /// <param name="item">Preset from obj preset.</param>
        /// <param name="handles">UnityHandles from container. **Bag **Vending **Pocket **Port</param>
        public void Drawout(IPickup item, params ICoordinate[] handles)
        {
            var actor = TakeOver();

            if (handles.Length > 0)
            {
                var plan = new Plan() { StrokeType = StrokeType.Drawout, Item = item, TargetCoordinate = handles[0] };
                GetHand(actor).Plans.Enqueue(plan);
            }

            if (handles.Length > 1)
            {
                var plan = new Plan() { StrokeType = StrokeType.Support, Item = item, TargetCoordinate = handles[1] };
                GetHand(actor.Mirror()).Plans.Enqueue(plan);
            }
        }

        /// <summary>
        /// ???????
        /// </summary>
        public void Unequip(bool unequipAll = false) //Input         // =>store/Schedule_Drop
        {
            var actor = CheckEmpty();

            switch (actor)
            {
                case Symmetry.Deny:
                    if (!GetHand(Prime).IsFinalFree)
                        actor = Prime;
                    else if (!GetHand(Secondary).IsFinalFree)
                        actor = Secondary;
                    else

                        return;
                    break;

                case Symmetry.Right:
                    actor = actor.Mirror();
                    break;


                case Symmetry.Left:
                    actor = actor.Mirror();
                    break;


                case Symmetry.Both:

                    if (!GetHand(Prime).IsFinalFree)
                        actor = Prime;
                    else if (!GetHand(Secondary).IsFinalFree)
                        actor = Secondary;
                    else
                        return;
                    break;
            }

            var hand = GetHand(actor);

            if (unequipAll)
            {

            }


            // if has place to store

            //if (InventoryHandler.CheckStore(hand.m_items, out var coordinate))
            //{
            //    hand.Plans.Enqueue(new Plan() { StrokeType = StrokeType.Store, TargetCoordinate = coordinate, Time = 0.3f });
            //}
            //else
            //{
            //    return;
            //}
        }

        [Obsolete]
        /// <summary>
        /// ???
        /// </summary>
        public void Discard()
        {
            //ICoordinate prime, second;
            //var pri = GetHand(Prime);
            //var sec = GetHand(Secondary);

            //var actor = CheckEmpty();

            //if (actor == EnableHand.Both)
            //    return;

            //if (actor == EnableHand.Deny)
            //{
            //    if (pri.m_items == sec.m_items)
            //        pri.m_items.GetHandle(out prime, out second);
            //    else
            //    {
            //        pri.m_items.GetHandles(out prime, out _);
            //        sec.m_items.GetHandles(out second, out _);
            //    }
            //}
            //else
            //{
            //    actor = Mirror(actor);
            //    if (actor == Prime)
            //        pri.m_items.GetHandles(out prime, out second);
            //    else
            //        pri.m_items.GetHandles(out second, out prime);
            //}

            //var time = 0.4f;

            //if (prime != null)
            //{
            //    var plan = new Plan() { StrokeType = StrokeType.Release, TargetCoordinate = prime, Time = time };
            //    if (!pri.IsSupport)
            //        plan.m_items = pri.m_items.Preset;

            //    pri.Plans.Enqueue(plan);
            //}


            //if (second != null)
            //{
            //    var plan = new Plan() { StrokeType = StrokeType.Release, TargetCoordinate = second, Time = time };
            //    if (!sec.IsSupport)
            //        plan.m_items = sec.m_items.Preset;

            //    sec.Plans.Enqueue(plan);
            //}



        }


        #endregion


        #region Schedule

        /// <summary>
        /// obj into hand => CheckFreeHand(whether arrange stroke) =>_checkCache preference hand(equip/replace) =>  
        /// </summary>
        [Obsolete]
        protected void Equip(Schedules hand, IPickup item, ICoordinate coordinate = null)
        {
            //var handles=obj.GetHandles(out var pri, out var sec);
            //var handle = obj.Grab(coordinate);
            //var mirror = GetHand(Mirror(hand.Side));

            //if (handles > 0) // both hand
            //{
            //    if (handles == 1)
            //    {
            //        handle.FillNullField(ref pri);
            //        handle.FillNullField(ref sec);
            //    }

            //    if (hand.Side.Equals(Prime)) //call assit
            //    {
            //        if (mirror.IsFinalFree)
            //        {
            //            var assistPlan = new Plan() { StrokeType = StrokeType.Support, TargetCoordinate = sec, Time = 0.3f };
            //            mirror.Plans.Enqueue(assistPlan);
            //            hand.Plans.Enqueue(new Plan() { StrokeType = StrokeType.Equip, TargetCoordinate = pri, Time = 0.3f, Target = obj.Preset, Dependent = assistPlan, DependType = Set.Subtraction });
            //        }
            //    }
            //    else
            //    {
            //        if (mirror.IsFinalFree)
            //        {
            //            var plan = new Plan() { StrokeType = StrokeType.Equip, TargetCoordinate = pri, Time = 0.3f };
            //            mirror.Plans.Enqueue(plan);
            //            hand.Plans.Enqueue(new Plan() { StrokeType = StrokeType.Support, TargetCoordinate = sec, Time = 0.3f, Target = obj.Preset, Dependent = plan, DependType = Set.Subtraction });
            //        }
            //    }
            //}
            //else // none handle
            //{
            //    hand.Plans.Enqueue(new Plan() { StrokeType = StrokeType.Equip, TargetCoordinate = handle,Time=0.3f });
            //}
        }


        private void Store(Schedules hand, ItemInfo item)
        {

        }
         
        #endregion



        #region FinishSchedule

        protected void FinishSchedule(Schedules hand)
        {
            var plan = hand.Plans.Dequeue();

            Debug.LogFormat("Finish schedule : {0}.", plan.Item);

            switch (plan.StrokeType)
            {
                case StrokeType.Support:
                    hand.IsSupport = true;
                    return;

                case StrokeType.Pickup:
                    pickup(hand, plan);
                    break;

                case StrokeType.Drop:
                    Schedule_Drop(hand);
                    break;

                case StrokeType.Equip:
                    equip(hand);
                    break;

                case StrokeType.Drawout:
                    drawout(hand, plan);
                    break;

                case StrokeType.Store:
                    store(hand);
                    break;
            }
        }


        protected virtual void pickup(Schedules hand, Plan plan)
        {
            Debug.Log(nameof(pickup));

            var queue = hand.Plans;

            if (plan.Item is not ItemShowcase showcase)
                return;

            //var showcase = plan.m_items.GetComponentInParent<ItemShowcase>();
            //hand.Slot.Grab(showcase);

            // equip or store
            //if (showcase.Preset.m_category == m_category.Weapon && hand.IsFree)
            //    Equip(hand, showcase, plan.TargetCoordinate);
            //else
            //    Store(hand, showcase.Preset);

            Equip(hand, showcase, plan.TargetCoordinate);
        }


        protected virtual void equip(Schedules hand) // assist hold => main hold => assist hold
        {
            Debug.Log(nameof(equip));




        }

        protected virtual void store(Schedules hand)       // To Repay or Release
        {
            Debug.Log(nameof(store));
            //var target = hand.m_items;

            //if (target == null)
            //    if (!hand.hand.Target.GetChild(0).TryGetComponent(out target))
            //        throw new KeyNotFoundException();

            //if (IM.CheckStore(hand.m_items, out var coordinate))
            //{
            //    IM.Store(hand.m_items);
            //    hand.m_items = null;
            //    hand.hand.Remove();
            //    Destroy(hand.hand.Target);
            //}
            //else
            //{
            //    hand.m_items.GetHandles(out var pri, out var sec);

            //    var support = hand.IsSupport;

            //    if (Prime == hand.Side)
            //        coordinate = support ? sec : pri;
            //    else
            //        coordinate = support ? pri : sec;

            //    hand.Plans.Enqueue(new Plan() { StrokeType = StrokeType.Equip, TargetCoordinate = coordinate, Time = 0.3f });
            //}
        }

        /// <summary>
        /// Inovoke
        /// </summary>
        protected virtual void Schedule_Drop(Schedules hand)
        {
            Debug.Log(nameof(Schedule_Drop));
            // null??
            var target = hand.Item;

            //var obj = hand.Slot.SlotItem;

            //if (obj != null)
            //    hand.Slot.Remove();
            //else
            //{
            //    //obj = Instantiate(target.Preset.Model, hand.hand.tf);
            //}

            //var showcase = hand.hand.CharacterRoot.InstanceIfNull<ItemShowcase>();
            //StartCoroutine(showcase.Motion(new Coordinate(hand.hand.tf), new PR { GUIDs = transform.forward }));
        }


        protected virtual void drawout(Schedules hand, Plan plan)  // drawout from inv
        {
            Debug.Log(nameof(drawout));       
            
            //if (InventoryHandler.ExtractStock(hand.m_items)) // 
            //{
            //    var showcase = plan.TargetCoordinate.ReadOnlyTransform.GetComponentInParent<ItemShowcase>();
            //    hand.Slot.Grab(showcase);
            //    Equip(hand, showcase, plan.TargetCoordinate);
            //}
            //else // delete
            //{
            //    Destroy(plan.TargetCoordinate.ReadOnlyTransform.CharacterRoot);
            //}

        }

        #endregion


        #region Fun **Drop

  


        /// <summary>
        /// Drop item to scene.
        /// </summary>
        protected virtual void DropPickup(IPickup pickup)
        {
            Debug.LogError(nameof(DropPickup));

            // item pool release item.
            pickup.Unregister(Agent);

            pickup.Drop(transform);


            // pickup pool control
            //ItemPool.Instance.Return(pickup);
        }

        #endregion


        #region ConfigExcution

        /// <summary>
        ///  usually using on take over obj
        /// </summary>
        protected Symmetry CheckUnionStroke(StrokeType newMission)
        {
            var hand = Symmetry.Deny;

            switch (newMission)
            {
                case StrokeType.Drop:
                    return hand;
                case StrokeType.Equip:
                    return hand;
                case StrokeType.Store:
                    return hand;
            }

            var pri = GetHand(Prime);
            var sec = GetHand(Secondary);

            var priFree = pri.IsFinalFree;
            var secFree = sec.IsFinalFree;

            if (priFree && secFree)
            {
                hand = pri.Plans.Count < sec.Plans.Count ? pri.Side : sec.Side;
            }
            else if (priFree)
            {
                hand = pri.Side;
            }
            else if (secFree)
            {
                hand = sec.Side;
            }

            Debug.Log("CheckUnionStroke : " + hand);

            return hand;
        }

        protected Symmetry CheckShortStroke()
        {
            var pri = GetHand(Prime);
            var sec = GetHand(Secondary);

            return pri.Plans.Count < sec.Plans.Count ? pri.Side : sec.Side;
        }

        protected Symmetry CheckEmpty()
        {
            var check = Symmetry.Deny;

            Debug.Log(Hand.Count);

            if (Hand.TryGetValue((int)Symmetry.Right, out var schedule) && schedule.IsFree)
                check |= schedule.Side;

            if (Hand.TryGetValue((int)Symmetry.Left, out schedule) && schedule.IsFree)
                check |= schedule.Side;


            Debug.Log("CheckAdd empty : " + check);

            return check;
        }

        /// <summary>
        /// Valid which hand to accept new mission.
        /// </summary>
        protected Symmetry TakeOver()
        {
            Symmetry actor;
            try
            {
                actor = CheckEmpty();

                if (actor == Symmetry.Both)
                    actor = Prime;
                else if (actor == Symmetry.Deny)
                {
                    actor = CheckUnionStroke(StrokeType.Pickup);

                    if (actor == Symmetry.Deny)
                        actor = CheckShortStroke();
                }

                Debug.Log("TakeOver : " + actor);
            }
            catch (Exception e)
            {
                actor = Symmetry.Deny;
                throw e;
            }


            return actor;
        }


        protected bool isHandEmpty(Symmetry actor)
        {
            switch (actor)
            {
                case Symmetry.Both:
                    return GetHand(Symmetry.Right).IsFree && GetHand(Symmetry.Left).IsFree;

                case Symmetry.Right:
                    return GetHand(Symmetry.Right).IsFree;

                case Symmetry.Left:
                    return GetHand(Symmetry.Left).IsFree;
            }

            Debug.LogError("Error check hand type : "+actor);

            throw new KeyNotFoundException();
        }


        protected bool Ratherquality(IItem newitem, out Vector2Int ReplaceTarget)   // only quickSlot and hand
        {
            Category type = newitem.Preset.Category;
            ReplaceTarget = new Vector2Int(0, 0);

            //go search IM

            return false;

        }

        protected void FinishTargetSensor(Schedules hand)
        {
            //Agent.I.SelectAS.StopMonitor(stroke.curStroke.obj);
        }

        #endregion


        #region Preference 

        void LoadGamePlay(SettingsManager manager)
        {
            var side = manager.PrimeHand;

            Prime = side == Side.Both ? Symmetry.Both :
                side == Side.Left ? Symmetry.Left : Symmetry.Right;

            if (Prime == Symmetry.Right)
                Secondary = Symmetry.Left;
            else
                Secondary = Symmetry.Right;
        }

        #endregion




        #region delete


        //public IInventoryManager InventoryHandler { get; protected set; }
        #region Equip



        //public virtual bool _Drawout(IPickup pickup, params ICoordinate[] handles)
        //{
        //    var length = handles.Length;
        //    for (int i = 0; i < length; i++)
        //    {
        //        Schedules hand;
        //        switch (i)
        //        {
        //            case 0:
        //                hand = GetHand(Prime);

        //                break;

        //            case 1:
        //                hand = GetHand(Secondary);
        //                break;

        //            default:
        //                if (Hand.TryGetValue(i, out hand))
        //                    break;
        //                else
        //                    return false;
        //        }


        //    }

        //    return true;
        //}



        #endregion

        /// <summary>
        /// Defines the reference to the character’s Main Camera transform.
        /// </summary>
        //[SerializeField]
        //[Required]
        //[Tooltip("Defines the reference to the character’s Main Camera transform.")]
        //private ReadOnlyTransform m_CameraTransformReference;

        /// <summary>
        /// Defines how far the character can search for interactive objects.
        /// </summary>
        //[SerializeField]
        //[Distance(0, Mathf.Infinity)]
        //[Tooltip("Defines how far the character can search for interactive objects.")]
        //public float m_InteractionRadius = 2;

        //public event Action Interact;

        //public Camera m_Camera;


        //public AudioEmitter m_PlayerBodySource { get; protected set; }

        //private void _Start()
        //{
        //    m_PlayerBodySource = mAudioManager.m_Instance.RegisterSource("[AudioEmitter] CharacterBody", transform.root, spatialBlend: 0);
        //}

        /*
/// <summary>
/// Casts a ray forward trying to find any targetable object in front of the character. 
/// </summary>
private void Search()
{
    Ray ray = new Ray(m_CameraTransformReference.position, m_CameraTransformReference.TransformDirection(Vector3.forward));

    RaycastHit[] results = new RaycastHit[4];
    int amount = Physics.SphereCastNonAlloc(ray, m_Controller.Controller.radius, results, m_InteractionRadius,
        Physics.AllLayers, QueryTriggerInteraction.Collide);

    float dist = m_InteractionRadius;
    GameObject temp = null;

    for (int i = 0, l = results.Length; i < l; i++)
    {
        if (!results[i].collider)
            continue;

        GameObject go = results[i].collider.CharacterRoot;

        if (go.transform.root == transform.root)
            continue;

        // Is the object visible?
        if (Physics.Linecast(m_CameraTransformReference.position, results[i].point, out RaycastHit hitInfo, Physics.AllLayers, QueryTriggerInteraction.Collide))
        {
            if (hitInfo.collider.CharacterRoot != go)
                continue;
        }

        // Discard unnecessary objects.
        if (!go.CompareTag(TagRefer.Selected) && go.GetComponent<IActionable>() == null && go.GetComponent<FirearmsShowcase>() == null)
            continue;

        if (results[i].distance > dist)
            continue;

        temp = go;
        dist = results[i].distance;
    }

    Target = temp;
}

private IEnumerator ExecuteInteract(IActionable target)
{
    if (m_CurrentWeapon != null && m_CurrentWeapon.CanUseEquipment)
    {
        m_ItemCoolDown = true;

        if (target.RequiresAnimation)
        {
            m_CurrentWeapon.Interact();
            yield return new WaitForSeconds(m_CurrentWeapon.InteractDelay);
        }

        target.Interact();

        yield return new WaitForSeconds(Mathf.Max(m_CurrentWeapon.InteractAnimationLength - m_CurrentWeapon.InteractDelay, 0));
        m_ItemCoolDown = false;
    }
    else if (m_CurrentWeapon == null)
    {
        target.Interact();
    }
}

private IEnumerator RefillItem(Equipment[] items)
{
    m_ItemCoolDown = true;

    Interact?.Invoke();

    // wait ius delegate time finish
    //yield return new WaitForSeconds(m_CurrentWeapon.InteractDelay);

    for (int i = 0; i < items.Length; i++)
    {
        items[i].Refill();
    }

    m_PlayerBodySource.ForcePlay(m_ItemPickupSound, m_ItemPickupVolume);

    yield return new WaitForSeconds(Mathf.Max(m_CurrentWeapon.InteractAnimationLength - m_CurrentWeapon.InteractDelay, 0));
    m_ItemCoolDown = false;
}



/// <summary>
/// Defines the reference to the Adrenaline obj.
/// </summary>
[SerializeField]
[Tooltip("Defines the reference to the Adrenaline obj.")]
private FirstAidKit m_Adrenaline;

  // Throw a grenade
    if (!IUS.IUS.m_ItemCoolDown)
    {
        if (m_LethalEquipmentAction.triggered && m_CurrentWeapon != null && m_CurrentWeapon.CanUseEquipment && m_FragGrenade && m_FragGrenade.Amount > 0)
        {
            StartCoroutine(ThrowGrenade());
        }
    }
  private IEnumerator ThrowGrenade()
{
    IUS.IUS.m_ItemCoolDown = true;

    m_CurrentWeapon.Deselect();
    yield return new WaitForSeconds(m_CurrentWeapon.HideAnimationLength);
    m_CurrentWeapon.Viewmodel.SetActive(false);

    m_FragGrenade.CharacterRoot.SetActive(true);
    m_FragGrenade.Init();
    m_FragGrenade.Use();

    yield return new WaitForSeconds(m_FragGrenade.UsageDuration);
    m_FragGrenade.CharacterRoot.SetActive(false);

    m_CurrentWeapon.Viewmodel.SetActive(true);
    m_CurrentWeapon.Select();
    IUS.IUS.m_ItemCoolDown = false;
}

private IEnumerator AdrenalineShot()
{
    IUS.m_ItemCoolDown = true;

    m_CurrentWeapon.Deselect();
    yield return new WaitForSeconds(m_CurrentWeapon.HideAnimationLength);
    m_CurrentWeapon.Viewmodel.SetActive(false);

    m_Adrenaline.CharacterRoot.SetActive(true);
    m_Adrenaline.Init();
    m_Adrenaline.Use();

    yield return new WaitForSeconds(m_Adrenaline.UsageDuration);
    m_Adrenaline.CharacterRoot.SetActive(false);

    m_CurrentWeapon.Viewmodel.SetActive(true);
    m_CurrentWeapon.Select();
    IUS.m_ItemCoolDown = false;
}

    /// <summary>
/// Checks the target object to analyze if it is a adrenaline pack.
/// </summary>
private void SearchForAdrenaline()
{
    if (!IUS.IUS.m_ItemCoolDown && m_EquippedWeaponsList.Count > 0 && m_CurrentWeapon != null && m_CurrentWeapon.CanUseEquipment)
    {
        if (Target)
        {
            // If the target has the Adrenaline Tags
            if (Target.CompareTag(m_AdrenalinePackTag) && m_Adrenaline.CanRefill)
            {
                if (m_InteractAction.triggered)
                {
                    StartCoroutine(RefillItem(new Equipment.Equipment[] { m_Adrenaline }));
                }
            }
        }
    }
}

   /// <summary>
/// Checks the target object to analyze if it is a weapon.
/// </summary>
private void SearchForWeapons()
{
    if (Target)
    {
        // Try to convert the target for a gun pickup.
        GunPickup target = Target.GetComponent<GunPickup>();

        // If the gun pickup is not null means that the target is actually a weapon.
        if (target)
        {
            IWeapon weapon = GetWeaponByID(target.Identifier);

            if (weapon == null)
                return;

            if (m_CurrentWeapon != null)
            {
                if (!m_CurrentWeapon.CanSwitch)
                    return;

                if (IsEquipped(weapon))
                    return;

                if (HasFreeSlot)
                {
                    if (m_InteractAction.triggered)
                    {
                        EquipWeapon(GetWeaponIndexOnList(weapon.Identifier));
                        Destroy(target.transform.CharacterRoot);
                        StartCoroutine(Change(m_CurrentWeapon, weapon, target.CurrentRounds));

                        IUS.m_PlayerBodySource.ForcePlay(m_ItemPickupSound, m_ItemPickupVolume);
                        CalculateWeight();
                    }
                }
                else
                {
                    if (m_InteractAction.triggered)
                    {
                        UnequipWeapon(GetEquippedWeaponIndexOnList(m_CurrentWeapon.Identifier));
                        EquipWeapon(GetWeaponIndexOnList(weapon.Identifier));
                        StartCoroutine(DropAndChange(m_CurrentWeapon, weapon, target, target.CurrentRounds));

                        if (m_FastChangeWeapons)
                            IUS.m_PlayerBodySource.ForcePlay(m_ItemPickupSound, m_ItemPickupVolume);

                        CalculateWeight();
                    }
                }
            }
            else
            {
                if (HasFreeSlot)
                {
                    if (m_InteractAction.triggered)
                    {
                        EquipWeapon(GetWeaponIndexOnList(weapon.Identifier));
                        Select(weapon, target.CurrentRounds);
                        Destroy(target.transform.CharacterRoot);
                        CalculateWeight();
                    }
                }
            }
        }
    }
}

/// <summary>
/// Checks the target object to analyze if it is a ammo box.
/// </summary>
private void SearchForAmmo()
{
    if (!IUS.IUS.m_ItemCoolDown && m_EquippedWeaponsList.Count > 0 && m_CurrentWeapon != null
        && m_CurrentWeapon.CanUseEquipment && CanRefillAmmo())
    {
        if (Target)
        {
            // If the target has the Ammo Tags
            if (Target.CompareTag(m_AmmoTag))
            {
                if (m_InteractAction.triggered)
                {
                    StartCoroutine(RefillAmmo());
                }
            }
        }
    }
}



/// <summary>
/// Checks the target object to analyze if it is a interactive object.
/// </summary>
private void SearchInteractiveObjects()
{
    if (!IUS.m_ItemCoolDown)
    {
        if (Target)
        {
            IActionable target = Target.GetComponent<IActionable>();

            if (target != null)
            {
                if (m_InteractAction.triggered)
                {
                    StartCoroutine(Interact(target));
                }
            }
        }
    }
}


*/
        #endregion



    }


}

namespace TestTransform
{
}
