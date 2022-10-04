using UnityEngine;

namespace Return
{
    public interface ITransform : ITransformConvert
    {
        public Vector3 position { get; set; }
        public Vector3 localPosition { get; set; }

        public Vector3 up { get; set; }
        public Vector3 forward { get; set; }
        public Vector3 right { get; set; }


        public Quaternion rotation { get; set; }
        public Quaternion localRotation { get; set; }
        
        public Vector3 localScale { get; set; }
    }
    

}
