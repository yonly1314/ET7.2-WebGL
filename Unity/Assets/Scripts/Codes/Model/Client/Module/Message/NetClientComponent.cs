using System.Net.Sockets;

namespace ET.Client
{
    public struct NetClientComponentOnRead
    {
        public Session Session;
        public object Message;
    }
#if !UNITY_WEBGL
    [ComponentOf(typeof(Scene))]
    public class NetClientComponent: Entity, IAwake<AddressFamily>, IDestroy
    {
        public int ServiceId;
    }
#else
    [ComponentOf(typeof (Scene))]
    public class NetClientComponent: Entity, IAwake, IDestroy
    {
        public int ServiceId;
    }
#endif
}