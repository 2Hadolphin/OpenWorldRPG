using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Linq;

namespace Return
{
    public interface IStatus
    {

    }

    /// <summary>
    /// 
    /// </summary>
    public class Status : NullCheck,IStatus
    {
        #region Public Variables

        public int BaseAmount { get; private set; }
        public int CalculatedAmount { get; private set; }
        public List<IModifier> Modifiers { get; }
        public string DataId { get; }
        public string OwnerId { get; }

        #endregion

        #region Constructor

        public Status(string id, string ownerId, string statDataId, int baseAmount)// : base(id)
        {
            DataId = statDataId;
            BaseAmount = baseAmount;
            CalculatedAmount = baseAmount;
            OwnerId = ownerId;
            Modifiers = new List<IModifier>();
            
            //AddDomainEvent(new StatCreated(id, statDataId, ownerId, BaseAmount));
        }

        #endregion

        #region Public Methods

        public void AddBaseAmount(int amount)
        {
            SetBaseAmount(BaseAmount + amount);
        }

        public void AddModifiers(List<string> modifierIds, List<ModifierType> modifierTypes, List<int> amounts)
        {
            var wantToAddCount = modifierIds.Count;


            var modifiersCount = Modifiers.Count;
            for (var i = 0; i < wantToAddCount; i++)
            {
                var modifierId = modifierIds[i];
                var modifierType = modifierTypes[i];
                var amount = amounts[i];
                AddModifier(modifierId, modifierType, amount);
            }

            Calculate();
        }

        public IModifier GetModifier(string modifierId)
        {
            var modifier = Modifiers.Find(modifier => modifier.GetId().Equals(modifierId));
            return modifier;
        }

        public void RemoveModifiers(List<string> modifierIds)
        {
            var modifierCountShouldEqualIds = Modifiers.Count >= modifierIds.Count;
            var expectedCount = Modifiers.Count - modifierIds.Count;

            foreach (var modifierId in modifierIds)
            {
                RemoveModifier(modifierId);
            }


            Calculate();
        }

        public void SetBaseAmount(int amount)
        {
            BaseAmount = amount;
            //if (BaseAmount < 0) BaseAmount = 0;
            //AddDomainEvent(new BaseAmountModified(GetId(), OwnerId, DataId, BaseAmount));
            Calculate();
        }

        #endregion

        #region Private Methods

        private void AddModifier(string modifierId, ModifierType modifierType, int amount)
        {
            var modifier = new Modifier(modifierId, modifierType, amount);
            Modifiers.Add(modifier);
            //AddDomainEvent(new ModifierAdded(GetId(), modifierId));
        }

        private void Calculate()
        {
            var flatModifiers = Modifiers.FindAll(modifier => modifier.Type.Equals(ModifierType.Flat));
            var percentAddModifiers = Modifiers.FindAll(modifier => modifier.Type.Equals(ModifierType.PercentAdd));
            var percentMultiModifiers = Modifiers.FindAll(modifier => modifier.Type.Equals(ModifierType.PercentMulti));
            // calculate Flat
            var sumFlat = flatModifiers.Sum(modifier => modifier.Amount);
            var sumPercentAdd = percentAddModifiers.Sum(modifier => modifier.Amount);
            var calculateResult = BaseAmount + sumFlat;
            // calculate Percent add
            if (percentAddModifiers.Count > 0)
            {
                var multiplyAdd = 1f + sumPercentAdd / 100f;
                var multiplyResult = Math.Round(calculateResult * multiplyAdd, MidpointRounding.AwayFromZero);
                calculateResult = (int)multiplyResult;
            }

            // calculate Percent multi
            if (percentMultiModifiers.Count > 0)
            {
                var aggregatePercentMulti = percentMultiModifiers.Aggregate(1f, (x, y) =>
                {
                    var result = x * (1f + y.Amount / 100f);
                    return result;
                });
                if (aggregatePercentMulti != 0)
                    calculateResult = (int)Math.Round(calculateResult * aggregatePercentMulti);
            }

            if (calculateResult < 0) calculateResult = 0;
            SetCalculatedAmount(calculateResult);
        }

        private void RemoveModifier(string modifierId)
        {
            var modifier = GetModifier(modifierId);
            var removeSucceed = Modifiers.Remove(modifier);
            //if (removeSucceed) AddDomainEvent(new ModifierRemoved(GetId(), modifierId));
        }

        private void SetCalculatedAmount(int amount)
        {
            CalculatedAmount = amount;
            //AddDomainEvent(new CalculatedAmountModified(GetId(), DataId, OwnerId, CalculatedAmount));
        }

        #endregion


    }


}
