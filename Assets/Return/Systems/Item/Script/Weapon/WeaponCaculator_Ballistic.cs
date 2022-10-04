using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Items.Weapons;
using Return;
using Return.CentreModule;

[RequireComponent(typeof(WeaponCentral_Arm))]
public class WeaponCaculator_Ballistic : WeaponCaculator
{

    private LayerMask mask;
    private LayerMask quickMask;

    //Usual Element
    private RaycastHit lastHit;
    private RaycastHit[] hits = new RaycastHit[7];

    private float KE;
    private float BH;
    private int CasttingCount = 0;
    private int IterationStep;
    private float IterationRange;
    private Ray cast;
    private Dictionary<Collider, IDamageState> LastTarget;
    public override bool Initialization(IWeaponBase _central)
    {
        if (_central == null)
        {
            gameObject.SetActive(false);
            Debug.LogError("Weapon System Error ! : " + this.name);
            return false;
        }

        central = _central;

        FireRate = 1 / (central.data.CurrentData.FireRate / 60);
        KE = central.data.CurrentData.KineticEnergy;
        BH = central.data.CurrentData.Hardness;
        IterationStep = central.data.CurrentData.IterationStep;
        IterationRange = central.data.CurrentData.Range / IterationStep;
        mask = central.data.CurrentData.CaculateMask;
        quickMask = GDR.QuickPhysicMask;
        LastTarget = new Dictionary<Collider, IDamageState>();

        if (central.io.ElementRef.Barrel != null)
        {
            chamber = central.io.ElementRef.Barrel.transform;
        }
        else
        {
            Debug.LogError("Weapon Missing Chamer Slot !");

        }

        waitRate = new WaitForSeconds(FireRate * 1); //?? get another type of cooldown bursttimes
        return true;
    }

    public override void Single()
    {
        HonestCast();
    }
    public override void Multi()
    {
        StartCoroutine(CoolDownByTime());
        CasttingCount++;
        cast = new Ray(chamber.position, chamber.forward);
        float BeenShotRange = IterationRange * 5; //??
        float CurKe = KE; //Quality*Speed
        float CurBH = BH; //Bullet Hardness
        bool Targetable = true;

        Vector3 ImpactPoint = cast.GetPoint(100);

        if (Physics.Raycast(cast, out lastHit, IterationRange * 5, quickMask, QueryTriggerInteraction.Collide))
        {
            BeenShotRange = lastHit.distance;
            ImpactPoint = lastHit.point;
        }



        IDamageState op;
        DamageState.Package package = new DamageState.Package();
        DamageState.Result ob;

        int obstacleLN = Physics.RaycastNonAlloc(cast, hits, BeenShotRange, mask, QueryTriggerInteraction.Collide);

        if (obstacleLN >= hits.Length)  //Too many target in rows
        {
            Targetable = false;
            Debug.LogWarning("PhysicCast too much target! Please CheckAdd the Game Design !");
        }

        float ObstacleScale;
        for (int i = 0; i < obstacleLN; i++)
        {
            if (hits[i].distance < BeenShotRange) // in Distance
            {
                print(" Hit Distance : "+hits[i].distance);
                if (hits[i].collider.CompareTag("DamageTag"))
                {
                    if (LastTarget.TryGetValue(hits[i].collider, out op))
                    {

                    }
                    else if (hits[i].transform.TryGetComponent<IDamageState>(out op))
                    {
                        LastTarget.Add(hits[i].collider, op);
                    }
                    else
                    {
                        Debug.LogError("Target has none IDamagerState but tag as damageable or layer on damageable !");
                        break;
                    }

                    ob = op.Sampling(1);
                    CurBH = HardnessCaculator(CurBH, ob.HardnessValue, out package.HardnessGap);
                    package.IsReflacted = false;    //??

                    if (CurBH == -1)
                    {
                        Targetable = false;
                        package.ReceiveKineticEnergy = CurKe;
                    }

                    if (ob.IsTrans)
                    {
                        ObstacleScale = hits[i].collider.GetColliderTransLength(cast);
                        package.ReceiveKineticEnergy = ob.ObstacleValue * ObstacleScale;
                        CurKe -= package.ReceiveKineticEnergy;
                    }
                    else
                    {
                        package.ReceiveKineticEnergy = ob.ObstacleValue > CurKe ? CurKe : ob.ObstacleValue;
                        CurKe -= ob.ObstacleValue;
                    }


                    if (CurKe <= 0)
                    {
                        Targetable = false;
                    }

                    central.io.PostDamageEvent(op, package);

                    if (!Targetable)
                    {
                        ImpactPoint = hits[i].point;
                        break;
                    }
                }

            }
        }
        StartCoroutine(DrawCast(cast, ImpactPoint));
        CasttingCount--;
    }

    protected void HonestCast()
    {

        StartCoroutine(CaculateTrajectory());
    }


    #region StaticFunction
    protected static float HardnessCaculator(float bullet, float target, out float gap)
    {
        gap = 1;
        return 0;
    }
    public override void MathfOperate()
    {
        throw new System.NotImplementedException();
    }


    #endregion

    protected IEnumerator DrawCast(Ray ray, Vector3 target)
    {
        Vector3 nextstep = ray.origin;
        central.io.ElementRef.rayline.SetPosition(0, nextstep);
        central.io.ElementRef.rayline.SetPosition(1, target);
        yield return new WaitForSeconds(0.01f);
        central.io.ElementRef.rayline.SetPosition(0, Vector3.zero);
        central.io.ElementRef.rayline.SetPosition(1, Vector3.zero);

        /*
        for (float t = 0; t < 0.25f; t+=GDR.deltaTime)
        {
            central.IO.ElementRef.rayline.SetPosition(0, nextstep);
            nextstep += ray.GetPoint(GDR.deltaTime*300);
            central.IO.ElementRef.rayline.SetPosition(1, nextstep);
            yield return null;
        }
        */
        yield break;
    }

    private WaitForSeconds waitRate;

    protected IEnumerator CaculateTrajectory() //?long shot
    {
        StartCoroutine(CoolDownByTime());
        CasttingCount++;

        Ray ray = new Ray(chamber.position, chamber.forward);
        float BeenShotRange;
        float CurKe = KE; //Quality*Speed
        float CurBH = BH; //Bullet Hardness


        print("Shoot!");



        Vector3 ImpactPoint = new Vector3();
        List<Vector3?> ImpactPoints = new List<Vector3?>();
        List<Vector3?> PenetractionPoint = new List<Vector3?>();

        ImpactPoints.Add(chamber.position);
        for (int i = 0; i < IterationStep; i++)
        {
            central.io.ElementRef.rayline.SetPosition(0, ray.origin);
            if (Physics.Raycast(ray, out lastHit, IterationRange, mask, QueryTriggerInteraction.Collide))
            {
                ImpactPoint = lastHit.point;
                ImpactPoints.Add(ImpactPoint);
                if (lastHit.collider.CompareTag("DamageTag"))
                {
                    IDamageState op;
                    DamageState.Package package = new DamageState.Package();
                    if (lastHit.transform.TryGetComponent<IDamageState>(out op))
                    {
                        DamageState.Result ob = op.Sampling(1);
                        float HardnessGap = CurBH - ob.HardnessValue;
                        float CastAngle = 90 - Vector3.Angle(ray.direction, lastHit.normal);


                        if (CastAngle < ob.ReflactAngle || HardnessGap < -1)  //Caculate Bounce cause  bullet too soft or direction
                        {
                            if (ob.Bounceable) //very HardRelfact ray 
                            {
                                ray.direction = Vector3.Reflect(ray.direction, lastHit.normal) * UnityEngine.Random.Range(0.5f, 1.5f);
                                CurKe = (CurKe - ob.ObstacleValue) * 0.35f; // ??? jectValue ref?
                            }
                            else // HardRelfact ray
                            {
                                ray.direction = Vector3.Reflect(ray.direction, lastHit.normal) * UnityEngine.Random.Range(0, 0.2f);
                                CurKe = (CurKe - ob.ObstacleValue) * 0.05f; // ??? jectValue ref?
                            }
                            package.ReceiveKineticEnergy = CurKe;
                            package.IsReflacted = true;
                        }
                        else // none-Relfact ray
                        {
                            float ObstacleScale = 1;

                            ray.direction += -lastHit.normal * Random.Range(0f, 0.35f);
                            if (ob.IsTrans)
                            {
                                ObstacleScale = lastHit.collider.GetColliderTransLength( ray);
                                PenetractionPoint.Add(ray.GetPoint(ObstacleScale));
                            }
                            float offKE = ob.ObstacleValue * ObstacleScale;
                            CurKe = (CurKe - offKE);
                            package.ReceiveKineticEnergy = offKE;
                            package.IsReflacted = false;
                        }
                        package.HardnessGap = HardnessGap;

                        ray.origin = ImpactPoint;
                        central.io.ElementRef.rayline.SetPosition(1, ImpactPoint);
                    }
                    central.io.PostDamageEvent(op, package);

                    if (CurKe <= 0)
                    {
                        break;
                    }
                }
                else // Wrong Layer debug
                {
                    print("LayerMask Warnning! : " + lastHit.collider);
                }
            }
            else //non cast target
            {
                ray.origin = ray.GetPoint(IterationRange);
                central.io.ElementRef.rayline.SetPosition(1, ray.GetPoint(IterationRange));
            }
            //Wait for next frame


            ray.direction = ray.direction + Vector3.down * 0.001f;// set??
            yield return null;
        }
        central.io.ElementRef.rayline.SetPosition(0, Vector3.zero);
        central.io.ElementRef.rayline.SetPosition(1, Vector3.zero);
        CasttingCount--;
        yield break;
    }



    protected IEnumerator CoolDownByTime()
    {
        yield return waitRate;
        central._cooldown = true;
    }

    public override void CheckMotion()
    {
        throw new System.NotImplementedException();
    }



}
