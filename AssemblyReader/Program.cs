using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AssemblyReader
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var files = Directory.GetFiles(Path.Combine(myDocuments, "dll"), "*.dll");
            var sb = new StringBuilder("Documentation");

            foreach (var file in files)
            {
                var assembly = Assembly.LoadFrom(file);
                var loaded = assembly.GetExportedTypes().Where(x => x.IsInterface);

                foreach (var type in loaded)
                {
                   sb.AppendLine("");
                   sb.AppendLine($"Interface: {type.Name}");
                    var methods = type.GetMethods();

                    foreach (var methodInfo in methods)
                    {
                        sb.AppendLine($"Method: {methodInfo.Name}");
                        sb.AppendLine($"Returns: {methodInfo.ReturnType}");
                        
                        var parameters = methodInfo.GetParameters();
                        var values = parameters.Select(x => new
                        {
                            ParameterType = $"{x.ParameterType.Name} {x.Name}"
                        });

                        sb.AppendLine($"Parameters: {string.Join(" ,", values)}");
                        sb.AppendLine("");
                        //var name = methodInfo.DeclaringType?.Name;
                        //var ns = methodInfo.DeclaringType?.Namespace;
                        //var rt1 = methodInfo.ReturnType.GenericTypeArguments[0].Name;

                        //foreach (var parameterInfo in param)
                        //{
                        //    var p = parameterInfo.Name;
                        //    var t = parameterInfo.ParameterType.Name;
                        //}
                    }

                }
            }

            File.WriteAllText(Path.Combine(myDocuments,"dll", "dll.txt"), sb.ToString());

            Console.WriteLine(sb.ToString());
            Console.WriteLine("Done!");
            Console.ReadLine();

        }
    }
}