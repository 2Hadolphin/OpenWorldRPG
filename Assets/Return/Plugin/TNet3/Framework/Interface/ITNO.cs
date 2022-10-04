namespace TNet
{
    /// <summary>
    /// Intetface of network handler(unit).
    /// </summary>
    public interface ITNO
    {
        #region Native

        #region Property

        /// <summary>
        /// Whether this object belongs to the local player.
        /// </summary>
        bool isMine { get; set; }

        /// <summary>
        /// Whether sending messages through this object is possible or not.
        /// </summary>
        bool canSend { get; }

        #endregion

        void Send(byte rfcID, Target target, params object[] objs);
        void Send(byte rfcID, Target target);
        void Send(byte rfcID, Target target, object obj0);
        void Send(byte rfcID, Target target, object obj0, object obj1);
        void Send(byte rfcID, Target target, object obj0, object obj1, object obj2);
        void Send(byte rfcID, Target target, object obj0, object obj1, object obj2, object obj3);
        void Send(byte rfcID, Target target, object obj0, object obj1, object obj2, object obj3, object obj4);
        void Send(byte rfcID, string targetName, params object[] objs);
        void Send(byte rfcID, string targetName);
        void Send(byte rfcID, string targetName, object obj0);
        void Send(byte rfcID, string targetName, object obj0, object obj1);
        void Send(byte rfcID, string targetName, object obj0, object obj1, object obj2);
        void Send(byte rfcID, string targetName, object obj0, object obj1, object obj2, object obj3);
        void Send(string rfcName, Target target, params object[] objs);
        void Send(string rfcName, Target target);
        void Send(string rfcName, Target target, object obj0);
        void Send(string rfcName, Target target, object obj0, object obj1);
        void Send(string rfcName, Target target, object obj0, object obj1, object obj2);
        void Send(string rfcName, Target target, object obj0, object obj1, object obj2, object obj3);
        void Send(string rfcName, Target target, object obj0, object obj1, object obj2, object obj3, object obj4);
        void Send(string rfcName, Target target, object obj0, object obj1, object obj2, object obj3, object obj4, object obj5);
        void Send(string rfcName, Target target, object obj0, object obj1, object obj2, object obj3, object obj4, object obj5, object obj6);
        void Send(string rfcName, string targetName, params object[] objs);
        void Send(string rfcName, string targetName);
        void Send(string rfcName, string targetName, object obj0);
        void Send(string rfcName, string targetName, object obj0, object obj1);
        void Send(string rfcName, string targetName, object obj0, object obj1, object obj2);
        void Send(string rfcName, string targetName, object obj0, object obj1, object obj2, object obj3);
        void Send(byte rfcID, Player target, params object[] objs);
        void Send(byte rfcID, Player target);

        void Send(byte rfcID, Player target, object obj0);
        void Send(byte rfcID, Player target, object obj0, object obj1);
        void Send(byte rfcID, Player target, object obj0, object obj1, object obj2);
        void Send(byte rfcID, Player target, object obj0, object obj1, object obj2, object obj3);


        void Send(string rfcName, Player target, params object[] objs);
        void Send(string rfcName, Player target);
        void Send(string rfcName, Player target, object obj0);
        void Send(string rfcName, Player target, object obj0, object obj1);
        void Send(string rfcName, Player target, object obj0, object obj1, object obj2);
        void Send(string rfcName, Player target, object obj0, object obj1, object obj2, object obj3);
        void Send(string rfcName, Player target, object obj0, object obj1, object obj2, object obj3, object obj4);
        void Send(string rfcName, Player target, object obj0, object obj1, object obj2, object obj3, object obj4, object obj5);



        void Send(byte rfcID, int playerID, params object[] objs);
        void Send(byte rfcID, int playerID);
        void Send(byte rfcID, int playerID, object obj0);
        void Send(byte rfcID, int playerID, object obj0, object obj1);
        void Send(byte rfcID, int playerID, object obj0, object obj1, object obj2);
        void Send(byte rfcID, int playerID, object obj0, object obj1, object obj2, object obj3);


        #region Send a remote function call.

        void Send(string rfcName, int playerID, params object[] objs);
        void Send(string rfcName, int playerID);
        void Send(string rfcName, int playerID, object obj0);
        void Send(string rfcName, int playerID, object obj0, object obj1);
        void Send(string rfcName, int playerID, object obj0, object obj1, object obj2);
        void Send(string rfcName, int playerID, object obj0, object obj1, object obj2, object obj3);
        void Send(string rfcName, int playerID, object obj0, object obj1, object obj2, object obj3, object obj4);
        void Send(string rfcName, int playerID, object obj0, object obj1, object obj2, object obj3, object obj4, object obj5);
        void Send(string rfcName, List<int> playerIDs, params object[] objs);

        void Send(string rfcName, List<int> playerIDs, object obj0);
        void Send(string rfcName, List<int> playerIDs, object obj0, object obj1);
        void Send(string rfcName, List<int> playerIDs, object obj0, object obj1, object obj2);
        void Send(string rfcName, List<int> playerIDs, object obj0, object obj1, object obj2, object obj3);
        void Send(string rfcName, List<int> playerIDs, object obj0, object obj1, object obj2, object obj3, object obj4);
        void Send(string rfcName, List<int> playerIDs, object obj0, object obj1, object obj2, object obj3, object obj4, object obj5);

        #endregion


        #region Send a remote function call via UDP (if possible).

        void SendQuickly(byte rfcID, Target target, params object[] objs);
        void SendQuickly(byte rfcID, Target target);
        void SendQuickly(byte rfcID, Target target, object obj0);
        void SendQuickly(byte rfcID, Target target, object obj0, object obj1);
        void SendQuickly(byte rfcID, Target target, object obj0, object obj1, object obj2);
        void SendQuickly(byte rfcID, Target target, object obj0, object obj1, object obj2, object obj3);

        #endregion

        #region Qucick Player

        void SendQuickly(byte rfcID, int playerID, params object[] objs);
        void SendQuickly(byte rfcID, int playerID);
        void SendQuickly(byte rfcID, int playerID, object obj0);
        void SendQuickly(byte rfcID, int playerID, object obj0, object obj1);
        void SendQuickly(byte rfcID, int playerID, object obj0, object obj1, object obj2);
        void SendQuickly(byte rfcID, int playerID, object obj0, object obj1, object obj2, object obj3);

        #endregion

        /// <summary>
        /// Send a remote function call via UDP (if possible).
        /// </summary>

        void SendQuickly(string rfcName, Target target, params object[] objs);
        void SendQuickly(string rfcName, Target target);
        void SendQuickly(string rfcName, Target target, object obj0);
        void SendQuickly(string rfcName, Target target, object obj0, object obj1);
        void SendQuickly(string rfcName, Target target, object obj0, object obj1, object obj2);
        void SendQuickly(string rfcName, Target target, object obj0, object obj1, object obj2, object obj3);

        /// <summary>
        /// Send a remote function call via UDP (if possible).
        /// </summary>

        void SendQuickly(byte rfcID, Player target, params object[] objs);
        void SendQuickly(byte rfcID, Player target);
        void SendQuickly(byte rfcID, Player target, object obj0);
        void SendQuickly(byte rfcID, Player target, object obj0, object obj1);
        void SendQuickly(byte rfcID, Player target, object obj0, object obj1, object obj2);
        void SendQuickly(byte rfcID, Player target, object obj0, object obj1, object obj2, object obj3);

        /// <summary>
        /// Send a remote function call via UDP (if possible).
        /// </summary>

        void SendQuickly(string rfcName, Player target, params object[] objs);
        void SendQuickly(string rfcName, Player target);
        void SendQuickly(string rfcName, Player target, object obj0);
        void SendQuickly(string rfcName, Player target, object obj0, object obj1);
        void SendQuickly(string rfcName, Player target, object obj0, object obj1, object obj2);
        void SendQuickly(string rfcName, Player target, object obj0, object obj1, object obj2, object obj3);

        /// <summary>
        /// Send a remote function call via UDP (if possible).
        /// </summary>

        void SendQuickly(string rfcName, int playerID, params object[] objs);
        void SendQuickly(string rfcName, int playerID);
        void SendQuickly(string rfcName, int playerID, object obj0);
        void SendQuickly(string rfcName, int playerID, object obj0, object obj1);
        void SendQuickly(string rfcName, int playerID, object obj0, object obj1, object obj2);
        void SendQuickly(string rfcName, int playerID, object obj0, object obj1, object obj2, object obj3);

        /// <summary>
        /// Send a broadcast to the entire LAN. Does not require an active connection.
        /// </summary>

        void BroadcastToLAN(int port, byte rfcID, params object[] objs);

        /// <summary>
        /// Send a broadcast to the entire LAN. Does not require an active connection.
        /// </summary>

        void BroadcastToLAN(int port, string rfcName, params object[] objs);



        /// <summary>
        /// Remove a previously saved remote function call.
        /// </summary>

        void RemoveSavedRFC(string rfcName);

        /// <summary>
        /// Remove a previously saved remote function call.
        /// </summary>

        void RemoveSavedRFC(byte rfcID);
        #endregion

        #region Extension

        /// <summary>
        /// Register network module to TNObject.
        /// </summary>
        void Register<T>(T module) where T : class;

        #endregion

    }


    //public class FakeTno : BaseComponent
    //   {

    //	public virtual bool isMine
    //	{
    //		get => true;
    //		set => throw new KeyNotFoundException();
    //       }
    //   }

}