using System.Collections;
using UnityEngine;
using Return.Humanoid;
using Return.CentreModule;
using Return;
public class ProjectTileManager : MonoBehaviour
{
    protected RocketLocator loactor;
    protected RocketMotor motor;

    [System.Serializable]
    public struct ProjectileData 
    {
        public bool waitForIgnite;
        public int MaxSpeed;
        public int MaxHp;
        public float Fuel;
        public float MaxRotSpeed;
        public int mass;
        public int[] reachTime;
        public int ExplosionRange;
    }
    [SerializeField]
    protected ProjectileData data;

    public ProjectileData getData { get { return data; } }
    [SerializeField]
    private ParticleSystem flame=null;
    [SerializeField]
    private ParticleSystem explosion=null;


    public string UIDSu = null;
    public GameObject WeaponIndicator = null;
    public GameObject TargetIndicator = null;
    public GameObject GammingUI = null;



    public Transform CDN { get; protected set; }
    public GameObject uiA = null;
    public GameObject uiB = null;

    public struct ProjectileTargetData 
    {
        public bool Targeted;
        public Transform[] targets;
        //public IPlayerFroegin user;
    }
    private ProjectileTargetData Targetdata;
    public void Activate()
    {


        // DM
        data.mass = 3;
        data.MaxRotSpeed = 35;
        data.MaxSpeed = 300;
        data.waitForIgnite = true;
        data.ExplosionRange = 50;
        data.Fuel = 10;

        //................
        loactor = gameObject.GetComponent<RocketLocator>();
        motor = gameObject.GetComponent<RocketMotor>();

        loactor.Initializate();
        motor.Initializate();

        motor.rb.drag = 1f;
        motor.rb.angularDrag = 3f;
        motor.rb.mass = data.mass;
        motor.Active = true;

        data.reachTime = new int[5] { 2, 6, 7, 0, 0 };




    }

    public void Launch()
    {


        transform.parent = null;
        motor.rb.AddForce(transform.forward * 1000, ForceMode.Impulse);
        //loactor.SetGuid(Targetdata.targets[0]);
        StartCoroutine(Stroke());

    }

    IEnumerator Stroke()
    {
        yield return ConstCache.WaitForFixedUpdate;

        if (Targetdata.Targeted)
        {
            CDN = Targetdata.targets[0];
        }
        else
        {
            Collider[] enemys;
            
            enemys = Physics.OverlapSphere(transform.position, 1000, GDR.WeaponPhysicMask);
            CDN = enemys[Random.Range(0, enemys.Length)].transform;
            print("AutoAssigningEnemy");
        }

        loactor.SetGuid(CDN);
        yield return new WaitForSeconds(0.8f);
        motor.Active = false;
        //--------caculate coordinate



        yield return new WaitForSeconds(0.2f);
        print( "Ignite Now");
        Ignite();
        loactor.Active = true;
        DrawTargetUI(true);
    }

    private void Ignite()
    {
        flame.Play();
        motor.Active = true;
        motor.rb.drag = 1f;
        motor.rb.angularDrag = 1f;
    }

    public void Explode()
    {

        Collider[] targets = new Collider[50];
        int length=Physics.OverlapSphereNonAlloc
            (
            transform.position,
            data.ExplosionRange, 
            targets, 
            GDR.WeaponPhysicMask, 
            QueryTriggerInteraction.Collide
            );

        IDamageState target;

        for(int i = 0; i < length; i++)
        {
            if(targets[i].TryGetComponent<IDamageState>(out target))
            {
                target.ReceiveDamage(DamageState.DamageType.Overlap,50);
            }
        }
        GameObject.Destroy(this.gameObject);
        //overlap zoom pass damage
    }
    public void  DrawTargetUI(bool z)
    {
        /*
        if (z)
        {
            uiA = Instantiate(WeaponIndicator, GammingUI.transform);
            uiA.GetComponent<WeaponIndicator>().Target = this.CharacterRoot;
            uiB = Instantiate(TargetIndicator, GammingUI.transform);
            uiB.GetComponent<WeaponIndicator>().Target = CDN;
        }
        else
        {
            uiB.GetComponent<WeaponIndicator>().Target = CDN;
        }
        */

    }

}
