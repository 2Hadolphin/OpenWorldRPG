//using NodeCanvas.Framework;
//using ParadoxNotion.Design;
//using NodeCanvas.Tasks.Actions;

//namespace Return.m_items.Weapons.Firearms
//{
//    [m_category("✫ Utility")]
//    [Description("Send a graph event with TSerializable value. If global is true, all graph owners in scene will receive this event. Use along with the 'CheckAdd Event' Condition")]
//    public class UTagEvent : SendEvent<UTag>
//    {
//        protected override string info
//        {
//            get { return string.Format("{0} Event [{1}] ({2}){3}", (sendGlobal ? "Global " : ""), eventName, eventValue, (delay.value > 0 ? " after " + delay + " sec." : "")); }
//        }

//        protected override void OnUpdate()
//        {
//            if (elapsedTime >= delay.value)
//            {
//                if (sendGlobal)
//                {
//                    Graph.SendGlobalEvent(eventName.value, eventValue.value, this);
//                }
//                else
//                {
//                    agent.SendEvent(eventName.value, eventValue.value, this);
//                }
//                EndAction();
//            }
//        }
//    }
//}