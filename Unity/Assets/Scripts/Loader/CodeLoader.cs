using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HybridCLR;
using UnityEngine;

namespace ET
{
    public class CodeLoader: Singleton<CodeLoader>
    {
        private Assembly model;

        public async ETTask Start()
        {
            if (Define.EnableCodes)
            {
                GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
                if (globalConfig.CodeMode != CodeMode.ClientServer)
                {
                    throw new Exception("ENABLE_CODES mode must use ClientServer code mode!");
                }

                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(assemblies);
                EventSystem.Instance.Add(types);
                foreach (Assembly ass in assemblies)
                {
                    string name = ass.GetName().Name;
                    if (name == "Unity.Model.Codes")
                    {
                        this.model = ass;
                    }
                }
            }
            else
            {
                byte[] assBytes;
                byte[] pdbBytes;
#if !UNITY_EDITOR || UNITY_WEBGL

                //Dictionary<string, UnityEngine.Object> dictionary = AssetsBundleHelper.LoadBundle("code.unity3d");
                Dictionary<string, UnityEngine.Object> dictionary = await WebGLHelper.LoadBundle("code.unity3d");
                assBytes = ((TextAsset)dictionary["Model.dll"]).bytes;
                pdbBytes = ((TextAsset)dictionary["Model.pdb"]).bytes;

                Dictionary<string, UnityEngine.Object> dicDlls = await WebGLHelper.LoadBundle("aotdlls.unity3d");
                foreach (var kv in dicDlls)
                {
                    byte[] bytes = (kv.Value as TextAsset).bytes;
                    RuntimeApi.LoadMetadataForAOTAssembly(bytes, HomologousImageMode.SuperSet);
                }
#else
                assBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Model.dll"));
                pdbBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Model.pdb"));

#endif
                this.model = Assembly.Load(assBytes, pdbBytes);
                await this.LoadHotfix();
            }

            IStaticMethod start = new StaticMethod(this.model, "ET.Entry", "Start");
            start.Run();
        }

        // 热重载调用该方法
        public async ETTask LoadHotfix()
        {
            byte[] assBytes;
            byte[] pdbBytes;
#if !UNITY_EDITOR || UNITY_WEBGL
            Dictionary<string, UnityEngine.Object> dictionary = await WebGLHelper.LoadBundle("code.unity3d");
            assBytes = ((TextAsset)dictionary["Hotfix.dll"]).bytes;
            pdbBytes = ((TextAsset)dictionary["Hotfix.pdb"]).bytes;

#else
            // 傻屌Unity在这里搞了个傻逼优化，认为同一个路径的dll，返回的程序集就一样。所以这里每次编译都要随机名字
            string[] logicFiles = Directory.GetFiles(Define.BuildOutputDir, "Hotfix_*.dll");
            if (logicFiles.Length != 1)
            {
                throw new Exception("Logic dll count != 1");
            }

            string logicName = Path.GetFileNameWithoutExtension(logicFiles[0]);
            assBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, $"{logicName}.dll"));
            pdbBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, $"{logicName}.pdb"));
#endif

            Assembly hotfixAssembly = Assembly.Load(assBytes, pdbBytes);
            
            Dictionary<string, Type> types =
                    AssemblyHelper.GetAssemblyTypes(typeof (Game).Assembly, typeof (Init).Assembly, this.model, hotfixAssembly);

            foreach (var e in types)
            {
                Log.Debug($"EventSystem Add: {e.Key}");
            }
            EventSystem.Instance.Add(types);
            
        }
    }
}