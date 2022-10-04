using Return;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Return.Humanoid;
using Return.Cameras;

public class Camera_ThirdPerson : HumanoidCameraBase
{

    protected override void Awake()
    {
        base.Awake();
        YawTransform = transform.parent;

        //Settings.Push(DemoPreset.Instance.DefaultCameraSetting);
        setting = Settings.Peek();
    }


    public override void Activate()
    {
        base.Activate();

        if (_Beacon == null)
        {
            enabled = false;
            return;
        }

        _ViewportDistance_current = (_MaxDistance + _MinDistance) * 0.61f;
        _ViewportDistance_control = _ViewportDistance_current;
        var newpos = _Beacon.forward * -_ViewportDistance_current;
        newpos = Quaternion.AngleAxis(45f, _Beacon.right) * newpos;
        Overwrite_pos = _Beacon.forward * _ViewportDistance_current;
        CullingDistance = Camera.nearClipPlane * 2f;

        Subscribe();
    }


    protected void Subscribe()
    {
        var input = InputManager.Input;
        input.Humanoid.MouseRotate.performed += InputControl;
        input.Humanoid.Room.performed += Room;
    }

    public bool Freeze
    {
        set
        {
            if (value)
                Subscribe();
            else
                Unsubscribe();
        }
    }

    public override Transform HostObject => throw new System.NotImplementedException();



    #region Parameter
    [Range(float.Epsilon, 100f)]
    public float Sensitivity_Room = 0.1f;
    [SerializeField]
    protected LayerMask SeeThroughMask;
    public bool RoomOverwrite = true;
    #endregion

    #region Catch
    protected Vector2 InputRot;
    protected float Rotate_Vertical;
    protected float Rotate_Horizontal;
    protected Vector3 Offset;
    #endregion

    [Range(0.1f, 10f)]
    public float _MaxDistance;
    [Range(0.1f, 10f)]
    public float _MinDistance;
    public float _ViewportDistance_control;
    public float _ViewportDistance_current;

    public bool ForceOn;
    public Vector3[] TracePoint = new Vector3[5];

    protected virtual void Room(InputAction.CallbackContext ctx)
    {
        var delta = ctx.ReadValue<float>();
        if (delta == 0)
            return;

        delta *= ConstCache.deltaTime * -Sensitivity_Room;

        var distance = (_ViewportDistance_control + delta).Clamp_flow(_MinDistance, _MaxDistance, out var overflow);

        //print(delta);

        if (bool_Bouncing)
        {
            if (_ViewportDistance_control > _ViewportDistance_current)
            {
                if (delta > float.Epsilon)
                {
                    delta -= overflow;

                    if (RoomOverwrite)
                        _ViewportDistance_control += delta;
                    else
                        _ViewportDistance_control = distance;

                    _ViewportDistance_current += delta;
                }
            }
        }
        else
        {
            _ViewportDistance_control = distance;
            CalShotDistance(distance);
        }
    }

    protected virtual void CalShotDistance(float d)
    {
        //if(Physics.SphereCast(_Root.position,0.1f,-_Root.forward,out hit,_MaxDistance,SeeThroughMask))

        if (d != _ViewportDistance_control)
        {
            bool_Bouncing.SetAs(true);
        }

        d /= _ViewportDistance_current;

        Overwrite_pos.x *= d;
        Overwrite_pos.y *= d;
        Overwrite_pos.z *= d;

        _ViewportDistance_current *= d;
    }



    protected virtual void LoadSetting()
    {

    }


    protected override void LateUpdate()
    {
        base.LateUpdate();
        var deltaTime = ConstCache.deltaTime;

        //rotate
        Overwrite_pos = Quaternion.AngleAxis(Rotate_Horizontal * deltaTime, Vector3.up) * Overwrite_pos;
        var axis_v = Vector3.Cross(Vector3.up, Overwrite_pos);

        var Vertical_clamp = Rotate_Vertical * deltaTime;

        (Vertical_clamp + Vector3.Angle(Vector3.up, Overwrite_pos)).Flow(3, 173, out var flow);

        Overwrite_pos = Quaternion.AngleAxis(Vertical_clamp - flow, axis_v) * Overwrite_pos;

        Offset = _Beacon.TransformVector(setting.Offset_pos);
        YawTransform.position = Overwrite_pos + _Beacon.position + Offset;

        YawTransform.LookAt(_Beacon.position + Offset, Vector3.up);
    }

    protected Ray ray;
    protected RaycastHit hit;
    [SerializeField]
    protected float CullingDistance;

    [SerializeField]
    protected AnimationCurve AdjustWeight = null;
    [SerializeField]
    protected AnimationCurve RebounceWeight = null;
    [SerializeField]
    protected float AdjustViewportDistanceStrength = 1f;


    public float SensorDensity = 0.05f;

    /*
    protected float[] Distance=new float[360];
    public float StepLimit=0.07f;//distance/per degree
    public AnimationCurve LerpDistance;
    public virtual void FixedTick()
    {
        var origin = _Beacon.position + _Beacon.TransformVector(CurrentSetting.Offset_pos);
        var dir = (_Root.position - origin).normalized;
        var agl = Mathf.RoundToInt(Vector3.SignedAngle(_Beacon.forward, Vector3.ProjectOnPlane(dir, _Beacon.up),_Beacon.up));

        var debugdir = Quaternion.AngleAxis(Vector3.Angle(_Beacon.up,dir),_Beacon.right)*_Beacon.up;

        agl =agl.Deg2Pi();
        agl -= 25;
        agl = agl.Deg2Pi();
        dir = Quaternion.AngleAxis(-25f, _Beacon.up)*dir;


        Quaternion rot = Quaternion.AngleAxis(1, _Beacon.up);
        int sn=0;
        float min = _MaxDistance;

        var length = 50+agl;
        float d;
        for (int i = agl; i < length; i++)
        {
            if (Physics.OnSensorUpdate(origin, dir, out hit, _MaxDistance, SeeThroughMask))
            {
                d= hit.distance;
                if (d < min)
                {
                    sn = i.Deg2Pi();
                    min = d;
                    Distance[sn] = d;
                }
                else
                    Distance[i.Deg2Pi()] = d;
            }
            else
                Distance[i.Deg2Pi()] = _MaxDistance;

            //Debug.DrawRay(origin, dir * Distance[(i).Deg2Pi()], Color.red);
            dir = rot * dir;
        }

        //clockwise
        length = 25+sn;

        var value = Distance[sn];
        var minValue=value-StepLimit;
        var maxValue = value + StepLimit;
        var rate = 1f;
        
        for (int i = sn; i < length; i++)
        {
            value = Distance[i.Deg2Pi()].Clamp(minValue, maxValue);
            rate = 1+LerpDistance.Evaluate((i - sn)/(length-sn));
            minValue = value - StepLimit*rate;
            maxValue = value + StepLimit*rate;
            Distance[i.Deg2Pi()] = value;
            Debug.DrawRay(origin, Quaternion.AngleAxis(i,Vector3.up)*debugdir*value, Color.red);
        }

        //counterclockwise
        length = sn - 25;

        for (int i = sn; i > length; i--)
        {
            value = Distance[i.Deg2Pi()].Clamp(minValue, maxValue);
            rate = 1 + LerpDistance.Evaluate((i - sn) / (length - sn));
            minValue = value - StepLimit*rate;
            maxValue = value + StepLimit*rate;
            Distance[i.Deg2Pi()] = value;
            Debug.DrawRay(origin, Quaternion.AngleAxis(i, Vector3.up) * debugdir*value, Color.red);
        }



    }
    */
    protected Vector3 GetFocusPoint()
    {
        return _Beacon.position + Offset;
    }
    public virtual void CalViewDiatance()
    {

        var dir = YawTransform.position - GetFocusPoint();
        var agl = Vector3.SignedAngle(_Beacon.forward, Vector3.ProjectOnPlane(dir, _Beacon.up), _Beacon.up);

    }

    protected Collider[] obs = new Collider[4];

    protected float ApproachingDistance = 0.2f;
    protected float TurningRadius = 0.07f;
    protected virtual bool CalApproachingProgress(Vector3 dir, ref float progress)
    {
        var origin = YawTransform.position;
 
        var stepfwd = YawTransform.forward.Multiply(SensorDensity);
        int length = (int)(_ViewportDistance_current / SensorDensity) + 1;

        float sensorDistance = ApproachingDistance * _ViewportDistance_current+TurningRadius;
        float min = sensorDistance;
        float d = min;
        int sn = 1;
        for (int i = 1; i < length; i++)
        {
            origin += stepfwd;
            if (Physics.Raycast(origin, dir, out hit, sensorDistance, SeeThroughMask))
            {
                d = hit.distance;
                if (d < min)
                {
                    min = d;
                    sn = i;
                   // Debug.DrawRay(origin, dir * d, Color.yellow);
                }
               // else
                 //   Debug.DrawRay(origin, dir * d, Color.red);

            }
        }

        if (min.Equals(sensorDistance))
        {
            progress = 0;
            return false;
        }
        else
        {
            progress = 1 - Mathf.Clamp01((min - TurningRadius) / sensorDistance);
            return true;
        }
    }

    protected void CalSpeedDir()
    {
        var pos = YawTransform.position;
        speedDir = YawTransform.InverseTransformDirection(pos - lastPos);
        speedDir.z = 0;
        speedDir = YawTransform.TransformDirection(speedDir);
        lastPos = pos;
    }

    protected bool bool_Bouncing = false;
    protected virtual void Rebouncing()
    {
        if (MathfUtility.Similar(_ViewportDistance_current, _ViewportDistance_control, 0.01f))
        {
            var weight= AdjustViewportDistanceStrength * RebounceWeight.Evaluate(_ViewportDistance_current / _ViewportDistance_control) * ConstCache.deltaTime;
//            print(weight);
            CalShotDistance(weight.Lerp(_ViewportDistance_current, _ViewportDistance_control));
        }
        else
        {
            _ViewportDistance_control = _ViewportDistance_current;
            bool_Bouncing.SetAs(false);
        }
    }

    protected Vector3 lastPos;
    protected Vector3 speedDir;
    protected float radius;
    protected override void Update()
    {
        base.Update();
        CalSpeedDir();
        var rootP = YawTransform.position;
        ray.origin = GetFocusPoint();
        ray.direction = YawTransform.forward.ASign();
        radius = (TurningRadius + ApproachingDistance * _ViewportDistance_current).Clamp(CullingDistance, _ViewportDistance_current * 0.5f);//(_ViewportDistance_current * radiusMultiply).Clamp(_MinDistance / 2 + camCullingDistance, _MaxDistance);

        Vector3 hitPoint;
        float progress = 0;


        /*
             if (Physics.OnSensorUpdate(ray,out hit, _ViewportDistance_control, SeeThroughMask))//bouncing lock
             {
                 Physics.SphereCast(ray, radius, out hit, _ViewportDistance_control, SeeThroughMask);//bouncing lock

                 var distance = hit.distance;
                 distance = (ConstCache.deltaTime*AdjustViewportDistanceStrength).Lerp(distance, _ViewportDistance_current);
                 enableViewportDistance = distance.Clamp(_MinDistance, _MaxDistance);

                 print(_MinDistance+"***"+enableViewportDistance);
                 print(Overwrite_pos.magnitude);
                 Debug.DrawLine(ray.origin, hit.point, Color.green);
             }
         */

        hitPoint = (rootP + ray.origin).Multiply(0.5f);
        var obsnum = Physics.OverlapSphereNonAlloc(hitPoint, _ViewportDistance_current * 0.5f, obs, SeeThroughMask);
        var speeddir = speedDir.normalized;
        float enableViewportDistance;
        if (speedDir.Equals(Vector3.zero))
        {
            if (Physics.SphereCast(ray, radius, out hit, _ViewportDistance_control, SeeThroughMask)&& hit.distance < _ViewportDistance_current)
            {
                enableViewportDistance = hit.distance;
            }
            else 
            {
                if (bool_Bouncing)
                    Rebouncing();
                return;
            }
        }
        else
        {
            if (obsnum > 1)//multi collider
            {
                obs.ClosestPoint(hitPoint, radius, out _, out _, out _, out hitPoint);
                Debug.LogError("multy col");
            }
            else if (obsnum.Equals(1))
            {
                hitPoint = obs[0].bounds.ClosestPoint(rootP);
            }

            bool isstatic;
            Vector3 iteration_a;
            float distance;

            if (Physics.SphereCast(ray, radius, out hit, _ViewportDistance_control, SeeThroughMask))
            {
                isstatic = hit.collider.gameObject.isStatic;
                distance = hit.distance;
                hitPoint = hit.point;
                iteration_a = ray.GetPoint((distance + radius - CullingDistance - TurningRadius).Clamp(_MinDistance, _MaxDistance));
                ray.direction = hitPoint - ray.origin;

                Debug.DrawRay(ray.origin, iteration_a, Color.green);
                if (obsnum == 0)
                {
                    enableViewportDistance = distance + radius - CullingDistance-TurningRadius;
                }
                else// if (distance < _ViewportDistance_control)
                {
                    if (Vector3.Dot(hitPoint-rootP, speeddir) < 0)//away
                    {
                        Debug.DrawRay(rootP, hitPoint - rootP, Color.green);
                        Debug.DrawRay(rootP, speeddir, Color.red);
                        speeddir.ASign_ref();//need lerp
                        Debug.DrawRay(rootP, speeddir, Color.yellow);
                    }

                    if (!CalApproachingProgress(speeddir, ref progress))
                        return;

                    #region Cal Iteration Point
                    if (Physics.SphereCast(ray, radius, out hit, _MaxDistance, SeeThroughMask) && hit.distance < _ViewportDistance_control)
                    {
                        distance = hit.distance;
                        var iteration_b = ray.GetPoint((distance + radius - CullingDistance - TurningRadius).Clamp(_MinDistance, _MaxDistance));

                        var endP = iteration_a.Vector3_Bezier(rootP, iteration_b, progress);

                        Debug.DrawLine(rootP, endP, Color.yellow);
                        Debug.DrawLine(iteration_a, iteration_b, Color.red);

                        enableViewportDistance = (_ViewportDistance_current - Vector3.Dot((endP - rootP), YawTransform.forward)).Clamp(_MinDistance, _MaxDistance);
                        Debug.DrawLine(iteration_a, ray.GetPoint(enableViewportDistance), Color.cyan);
                    }
                    #endregion
                    else
                        return;
                }
            }
            else
            {
                if (bool_Bouncing)
                    Rebouncing();

                return;
            }
            Debug.DrawLine(rootP, iteration_a, Color.red);
        }
        //close shot?
        enableViewportDistance = (ConstCache.deltaTime*AdjustViewportDistanceStrength).Lerp(_ViewportDistance_current, enableViewportDistance);
        CalShotDistance(enableViewportDistance);
    }

    /*
    protected void Update()
    {
        var p = _Beacon.position+Offset;
        ray.origin= p;
        ray.direction = _Root.position - p;
        var radius = (_ViewportDistance_current * radiusMultiply).Clamp(_MinDistance/2+ camCullingDistance, _MaxDistance);


        if (Physics.SphereCast(ray, radius, out hit, _MaxDistance, SeeThroughMask) && hit.distance < _ViewportDistance_control)
        {
            p = ray.GetPoint(hit.distance);
            var v_cast = hit.point - p;
            var agl = Vector3.Angle(ray.direction, v_cast)/90;
            agl=Mathf.Clamp01(agl);
            var weight = AdjustViewportDistanceStrength * AdjustWeight.Evaluate(agl) * ConstCache.deltaTime;
            enableViewportDistance = hit.distance + radius;

            CalShotDistance(weight.Lerp(_ViewportDistance_current, enableViewportDistance));
            print(weight + "=weight-- bounce--agl= :" + agl);
            print(enableViewportDistance - camCullingDistance);
            bool_Bouncing.SetAs(true);
            Debug.DrawRay(p, ray.direction.normalized*radius, Color.green);
            Debug.DrawLine(ray.origin, p,Color.red);
        }
        else 
        {
            if (bool_Bouncing)
            {
                if (_ViewportDistance_current + 0.01f < _ViewportDistance_control)
                    CalShotDistance((AdjustViewportDistanceStrength * RebounceWeight.Evaluate(_ViewportDistance_current/_ViewportDistance_control) * ConstCache.deltaTime).Lerp(_ViewportDistance_current, _ViewportDistance_control));
                else
                    bool_Bouncing.SetAs(false);
                print("Rebounce");
            }
        }
        //close shot?
    }

    */
    protected void InputControl(InputAction.CallbackContext obj)
    {
        InputRot = obj.ReadValue<Vector2>();
        Rotate_Horizontal = InputRot.x * setting.Sensitivity_H;
        Rotate_Vertical = InputRot.y * setting.Sensitivity_V;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!UnityEditor.EditorApplication.isPlaying)
            return;



        if (!YawTransform || !_Beacon)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(YawTransform.position, _Beacon.position + _Beacon.InverseTransformVector(setting.Offset_pos));

        Gizmos.color = Color.green;
        Gizmos.DrawCube(hit.point, Vector3.one.Multiply(0.005f));

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(ray.GetPoint(hit.distance),radius);


        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere((YawTransform.position + GetFocusPoint()).Multiply(0.5f), Vector3.Distance(YawTransform.position, GetFocusPoint()) / 2);
    }
#endif

    protected override void OnDisable()
    {
        base.OnDisable();

        Unsubscribe();
    }

    void Unsubscribe()
    {
        var input = InputManager.Input;
        input.Humanoid.MouseRotate.performed -= InputControl;
        input.Humanoid.Room.performed -= Room;
    }

    public override void SetTarget(Transform tf)
    {
        _Beacon = tf;
    }
}


