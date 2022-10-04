using DG.Tweening;

namespace Return.Cameras
{
    public class FirearmsPositionBob:Bob
    {
        public virtual void SetIncrease(float f,float duration=0.1f)
        {
            Increase = f;
            //DOTween.To(() => Increase, (width) => Increase = width, f, duration);
        }

        public override void UpdateTransform()
        {
            base.UpdateTransform();

            //Debug.Log(stepCount + " ** " + this);

        }
    }

}