


namespace Return
{
    public class Entity<T>
    {
        #region Protected Variables

        protected readonly T id;

        #endregion

        #region Constructor

        protected Entity(T id)
        {
            this.id = id;
        }

        #endregion

        #region Public Methods

        public T GetId()
        {
            return id;
        }

        #endregion
    }
}