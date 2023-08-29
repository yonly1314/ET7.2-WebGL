#if !UNITY_WEBGL

namespace ET
{
    [ConsoleHandler(ConsoleMode.ReloadDll)]
    public class ReloadDllConsoleHandler: IConsoleHandler
    {
        public async ETTask Run(ModeContex contex, string content)
        {
            switch (content)
            {
                case ConsoleMode.ReloadDll:
                    contex.Parent.RemoveComponent<ModeContex>();
                    #if !UNITY
                    CodeLoader.Instance.LoadHotfix();
                    #else
                    await CodeLoader.Instance.LoadHotfix();
                    #endif
                    EventSystem.Instance.Load();
                    break;
            }
            
            await ETTask.CompletedTask;
        }
    }
}
#endif