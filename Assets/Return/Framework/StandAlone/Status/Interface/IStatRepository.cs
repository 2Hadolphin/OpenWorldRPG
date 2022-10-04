using System.Collections.Generic;


namespace Return
{
    public interface IStatRepository //: IRepository<IStatReadModel>
    {
    #region Public Methods

        IModifier FindModifer(string statId , string modifierId);

        public IStatReadModel FindStat(string statId);
        //{
        //    return FindById(statId);
        //}

        IStatReadModel FindStat(string ownerId , string dataId);

        public List<IStatReadModel> FindStatsByOwnerId(string ownerId);

    #endregion
    }
}