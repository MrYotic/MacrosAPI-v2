using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MacrosAPI_v2
{
    public class MacrosLoader
    {
        private static readonly Dictionary<ulong, Assembly> CompileCache = new Dictionary<ulong, Assembly>();
        public static object Run(Macros apiHandler, string[] lines, string[] args, Dictionary<string, object> localVars, bool run = true)
        {
            ulong scriptHash = QuickHash(lines);
            Assembly assembly = null;
            lock (CompileCache)
            {
                if (!CompileCache.ContainsKey(scriptHash))
                {
                    bool scriptMain = true;
                    List<string> script = new List<string>(),
                    extensions = new List<string>(),
                    libs = new List<string>(),
                    dlls = new List<string>();
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("//using"))
                            libs.Add(line.Replace("//", "").Trim());
                        else if (line.StartsWith("//dll"))

                            dlls.Add(line.Replace("//dll ", "").Trim());
                        else if (line.StartsWith("//Script"))
                        {
                            if (line.EndsWith("Extensions"))
                                scriptMain = false;
                        }
                        else if (scriptMain)
                            script.Add(line);
                        else extensions.Add(line);
                    }
                    if (script.All(line => !line.StartsWith("return ") && !line.Contains(" return ")))
                        script.Add("return null;");
                    string code = string.Join("\n", new string[]
                    {
                        "using System;",
                        "using System.Collections.Generic;",
                        "using System.Text.RegularExpressions;",
                        "using System.Linq;",
                        "using System.Text;",
                        "using System.IO;",
                        "using System.Net;",
                        "using System.Threading;",
                        "using System.Windows.Forms;",
                        "using MacrosAPI_v2;",
                        string.Join("\n", libs),
                        "namespace ScriptLoader {",
                        "public class Script {",
                        "public CSharpAPI MCC;",
                        "public object __run(CSharpAPI __apiHandler, string[] args) {",
                            "this.MCC = __apiHandler;",
                            string.Join("\n", script),
                        "}",
                            string.Join("\n", extensions),
                        "}}"
                    });
                    CSharpCodeProvider compiler = new CSharpCodeProvider();
                    CompilerParameters parameters = new CompilerParameters();
                    parameters.ReferencedAssemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).Select(a => a.Location).ToArray());
                    parameters.CompilerOptions = "/t:library";
                    parameters.GenerateInMemory = true;
                    parameters.ReferencedAssemblies.AddRange(dlls.ToArray());
                    CompilerResults result = compiler.CompileAssemblyFromSource(parameters, code);
                    for (int i = 0; i < result.Errors.Count; i++)
                    {
                        throw new CSharpException(CSErrorType.LoadError, new InvalidOperationException(result.Errors[i].ErrorText + " | " + result.Errors[i].Line));
                    }
                    assembly = result.CompiledAssembly;
                    CompileCache[scriptHash] = result.CompiledAssembly;
                }
                assembly = CompileCache[scriptHash];
            }
            if (run)
            {
                try
                {
                    object compiledScript = assembly.CreateInstance("ScriptLoader.Script");
                    return compiledScript.GetType().GetMethod("__run").Invoke(compiledScript, new object[] { new CSharpAPI(apiHandler, localVars), args });
                }
                catch (Exception e) { throw new CSharpException(CSErrorType.RuntimeError, e); }
            }
            else return null;
        }
        private static ulong QuickHash(string[] lines)
        {
            ulong hashedValue = 3074457345618258791ul;
            for (int i = 0; i < lines.Length; i++)
            {
                for (int j = 0; j < lines[i].Length; j++)
                {
                    hashedValue += lines[i][j];
                    hashedValue *= 3074457345618258799ul;
                }
                hashedValue += '\n';
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }
        public enum CSErrorType { FileReadError, InvalidScript, LoadError, RuntimeError };
        public class CSharpException : Exception
        {
            private CSErrorType _type;
            public CSErrorType ExceptionType { get { return _type; } }
            public override string Message { get { return InnerException.Message; } }
            public override string ToString() { return InnerException.ToString(); }
            public CSharpException(CSErrorType type, Exception inner) : base(inner != null ? inner.Message : "", inner) => _type = type;
        }
    }
    public class CSharpAPI : Macros
    {
        public CSharpAPI(Macros apiHandler, Dictionary<string, object> localVars) => SetMaster(apiHandler);
        new public void LoadPlugin(Macros bot) => base.LoadPlugin(bot);
    }
}
