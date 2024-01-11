using Mono.Cecil;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ProjectApparatus
{
    public class Loader
    {
        private static GameObject o_Hack;
        private static GameObject o_Thirdperson;
        private static GameObject o_MainConsole;
        public static void Init()
        {
            o_Hack = new GameObject();
            o_Hack.AddComponent<Hacks>();
            UnityEngine.Object.DontDestroyOnLoad(o_Hack);

            o_Thirdperson = new GameObject();
            o_Thirdperson.AddComponent<Features.Thirdperson>();
            UnityEngine.Object.DontDestroyOnLoad(o_Thirdperson);

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            MainConsole.InitConsole();
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return LoadAssem();
        }

        public static Assembly LoadAssem()
        {
            byte[] ba;
            string resource = "ProjectApparatus.Resources.0Harmony.dll";
            Assembly curAsm = Assembly.GetExecutingAssembly();
            using (Stream stm = curAsm.GetManifestResourceStream(resource))
            {
                ba = new byte[(int)stm.Length];
                stm.Read(ba, 0, (int)stm.Length);
                return Assembly.Load(ba);
            }
        }

        public static void Unload()
		{
            MainConsole.DeAllocate();

			UnityEngine.Object.Destroy(Loader.o_Hack);
            UnityEngine.Object.Destroy(Loader.o_Thirdperson);
        }

	}
}
