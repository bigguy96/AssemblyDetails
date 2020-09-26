using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AssemblyReader
{
    class Program
    {
        static void Main(string[] args)
        {
            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var files = Directory.GetFiles(Path.Combine(myDocuments, "dll"), "*.dll");


            foreach (var file in files)
            {
                var assembly = Assembly.LoadFrom(file);
                var loaded = assembly.GetExportedTypes().Where(x => x.IsInterface);

                foreach (var type in loaded)
                {
                    var interf = type.Name;
                    var methods = type.GetMethods();

                    foreach (var methodInfo in methods)
                    {
                        var mname = methodInfo.Name;
                        var name = methodInfo.DeclaringType?.Name;
                        var ns = methodInfo.DeclaringType?.Namespace;
                        var rt = methodInfo.ReturnType.ToString();
                        var param = methodInfo.GetParameters();

                        var pp = param.Select(x => new
                        {
                            ParameterName = x.Name,
                            ParameterType = x.ParameterType.Name

                        });

                        foreach (var parameterInfo in param)
                        {
                            var p = parameterInfo.Name;
                            var t = parameterInfo.ParameterType.Name;
                        }
                    }

                }
            }


            Console.WriteLine("Done!");
            Console.ReadLine();

        }
    }
}
