using System.Collections;
using UnityEngine;

namespace Return
{

    public interface ICoordinate: IVirtualCoordinate
    {
        Space OutputPositionSpace { get; set; }
        Space OutputRotationSpace { get; set; }

        //Transform Transform { get; }
        //bool Equals(ICoordinate coordinate);
    }

    public interface IVirtualCoordinate
    {
        Vector3 position { get; }
        Quaternion rotation { get; }
        
        Vector3 forward { get; }
        Vector3 up { get; }
    }

    public class VirtualCoordinate : IVirtualCoordinate
    {
        public Vector3 position { get; set; }
        public Quaternion rotation { get; set; }

        public virtual Vector3 forward => rotation * Vector3.forward;
        public virtual Vector3 up => rotation * Vector3.up;
    }


}
