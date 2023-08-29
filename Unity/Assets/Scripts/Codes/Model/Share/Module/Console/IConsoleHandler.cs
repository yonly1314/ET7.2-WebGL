#if !UNITY_WEBGL
namespace ET
{
    public interface IConsoleHandler
    {
        ETTask Run(ModeContex contex, string content);
    }
}
#endif