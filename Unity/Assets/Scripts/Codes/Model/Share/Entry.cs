using System;
using System.Collections.Generic;

namespace ET
{
    namespace EventType
    {
        public struct EntryEvent1
        {
        }

        public struct EntryEvent2
        {
        }

        public struct EntryEvent3
        {
        }
    }

    public static class Entry
    {
        public static void Init()
        {
        }

        public static void Start()
        {
            StartAsync().Coroutine();
        }

        private static async ETTask StartAsync()
        {
            Log.Debug("Entry StartAsync");
            WinPeriod.Init();
            Log.Debug("Entry WinPeriod.Init()");
            MongoHelper.Init();
            Log.Debug("Entry MongoHelper.Init()");
            ProtobufHelper.Init();
            Log.Debug("Entry ProtobufHelper.Init()");
            Game.AddSingleton<NetServices>();
            Log.Debug("Entry NetServices.Init()");
            Game.AddSingleton<Root>();
            Log.Debug("Entry Root.Init()");
#if !UNITY_WEBGL
            await Game.AddSingleton<ConfigComponent>().LoadAsync();
#endif
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent1());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent2());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent3());
        }
    }
}