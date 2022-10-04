using System.Collections.Generic;
using UnityEngine;

namespace Return
{
    public class GetCoordinateInertia
    {
        public GetCoordinateInertia(ICoordinate coordinate)
        {
            Coordinate = coordinate;
        }

        readonly ICoordinate Coordinate;

        struct RecordData
        {
            public float Time;
            public PR PR;
        }

        List<RecordData> Records = new List<RecordData>();

        public PR Inertia
        {
            get
            {
                var pr = new PR();
                var length = Records.Count;

                if (length == 0)
                    return pr;

                for (int i = 1; i < length; i++)
                {
                    var deltaPos = Records[i].PR.Position - Records[i - 1].PR.Position;
                    pr.Position += deltaPos;

                    var deltaQuat = Quaternion.Inverse(Records[i - 1].PR.Rotation) * Records[i].PR.Rotation;
                    pr.Rotation *= deltaQuat;
                }

                var p = 1f / length;
                pr.Position *= p;
                pr.Rotation = Quaternion.Lerp(Quaternion.identity, pr.Rotation, p);
                return pr;
            }
        }


        public void Record()
        {
            var newRecord = new RecordData()
            {
                Time = Time.time,
                PR = new PR() { Position = Coordinate.position, Rotation = Coordinate.rotation }
            };

            Records.Add(newRecord);
        }
    }
}
