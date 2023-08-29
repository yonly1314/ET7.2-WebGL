using System.Threading;

namespace ET
{
    [ComponentOf(typeof (Scene))]
    public class NetThreadComponent: Entity, IAwake, ILateUpdate, IDestroy
    {
        [StaticField]
        public static NetThreadComponent Instance;
#if !UNITY_WEBGL
        public Thread thread;
        public bool isStop;
#endif
    }
}