using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System;
using UnityEngine.Assertions;
using UnityEngine;


namespace Return
{
    /// <summary>
    /// ???? hash?
    /// </summary>
    public interface IStatusWrapper:IEquatable<IStatusWrapper>
    {
        int GetHashCode();

    }

    /// <summary>
    /// ??????
    /// </summary>
    public abstract class BlackBoard : PresetDatabase, IEquatable<BlackBoard>, IStatusWrapper
    {
        [ShowInInspector]
        [Obsolete]
        public string EventID = string.Empty;

        [Obsolete]
        public virtual string GetID { get => EventID; }
        
        public virtual bool Equals(BlackBoard other)
        {
            if (!other || string.IsNullOrEmpty(other.EventID))
                return false;

            return EventID == other.EventID;
        }

        public bool Equals(IStatusWrapper other)
        {
            return GetHashCode().Equals(other.GetHashCode());
        }

        /// <summary>
        /// HashCode mixed with blackboard type and state id
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return GetType().GetHashCode() + EventID.GetHashCode();
        }

    }

    public static class BlackBoardExtension
    {
        /// <summary>
        /// ????????
        /// </summary>
        /// <typeparam name="TSet"></typeparam>
        /// <typeparam name="TBoard"></typeparam>
        /// <param name="set"></param>
        /// <param name="blackBoard"></param>
        /// <returns></returns>
        public static TSet RegisterState<TSet,TBoard>(this HashSet<TSet> set, TBoard blackBoard) where TSet : BlackBoard where TBoard:TSet
        {
            if (!set.TryGetValue(blackBoard, out var exist))
            {
                exist = blackBoard.UnityJsonCloneSelf<TBoard>();
                set.Add(exist);
            }
            return exist;
        }

        /// <summary>
        /// ???
        /// </summary>
        /// <typeparam name="TBoard"></typeparam>
        /// <param name="set"></param>
        /// <param name="blackBoard"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static TokenWrapper<TBoard> mRegisterState<TBoard>(this HashSet<IStatusWrapper> set, TBoard blackBoard)  where TBoard : BlackBoard,IStatusWrapper
        {
            if (set.TryGetValue(blackBoard,out var exist))
            {
                if (exist is TokenWrapper<TBoard> wrapper)
                    return wrapper;
                else
                    throw new NotImplementedException(exist.ToString());
            }
            else
            {
                blackBoard = blackBoard.UnityJsonCloneSelf<TBoard>();
                var wrapper = new TokenWrapper<TBoard>(blackBoard);
                set.Add(wrapper);
                return wrapper;
            }

        }
    }

    [Obsolete]
    public class TokenWrapper<U>:IStatusWrapper where U : BlackBoard
    {
        public TokenWrapper(U blackBoard)
        {
            Board = blackBoard;
        }
        public readonly U Board;

        public virtual bool LoadAs<T>(out T blackboard) where T : BlackBoard
        {
            if (Board is T)
                blackboard = Board as T;
            else
                blackboard = null;

            return blackboard;
        }

        /// <summary>
        /// Who can edit this blackboard
        /// </summary>
        public event Action<Token,bool> QualifyToken;

        public Token Current { get; protected set; }
        public readonly HashSet<Token> Tokens=new ();

        public virtual bool VerifyToken(Token token)
        {
            Tokens.Add(token);
            int order = 0;
            foreach (var cToken in Tokens)
            {
                if(cToken)
                {
                    order = token.CompareTo(cToken);
                    if (order > 1)
                        return false;
                }
            }

            return order > 0;
        }

        

        public override int GetHashCode()
        {
            return Board.GetHashCode();
        }

        public bool Equals(TokenWrapper<U> other)
        {
            return Board.Equals(other.Board);
        }

        public bool Equals(IStatusWrapper other)
        {
            if (other is null)
                return false;

            return GetHashCode().Equals(other.GetHashCode());
        }

        public static explicit operator U(TokenWrapper<U> wrapper)
        {
            return wrapper.Board;
        }
    }

    /// <summary>
    /// ????????
    /// </summary>
    [Obsolete]
    public abstract class NamedBlackBoard : BlackBoard,IDisposable
    {
        protected HashSet<object> Subscriber = new HashSet<object>();

        public virtual void Register(object user)
        {
            Subscriber.Add(user);
        }

        public virtual void Unregister(object user)
        {
            Subscriber.Remove(user);
            Dispose();
        }

        public void Dispose()
        {
            if (Subscriber.Count == 0)
#if UNITY_EDITOR
                DestroyImmediate(this);
#else               
                Destroy(this);
#endif

        }
    }
}