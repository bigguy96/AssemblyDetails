using ClassLibrary;
using System;
using System.Reflection;

namespace AssemblyReader
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //var files = Directory.GetFiles(Path.Combine(myDocuments, "dll"), "*.dll");
            //var sb = new StringBuilder("Documentation");
            //MTOA.BLL.Interfaces.dll
            
            //var assembly = typeof(User).Assembly;            
            //foreach (Type type in assembly.GetTypes())
            //{
            //    foreach (var methodInfo in type.GetMethods())
            //    {
            //        var methodDocumentation = methodInfo.GetDocumentation();
            //        if (!string.IsNullOrWhiteSpace(methodDocumentation))
            //        {
            //            Console.WriteLine(methodDocumentation);
            //        }

            //        foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
            //        {
            //            var parameterDocumentation = parameterInfo.GetDocumentation();
            //            if (!string.IsNullOrWhiteSpace(parameterDocumentation))
            //            {
            //                Console.WriteLine(parameterDocumentation);
            //            }                        
            //        }
            //    }
            //}

            const string path = @"C:\Users\gaeta\source\repos\AssemblyDetails\ClassLibrary\bin\Debug\ClassLibrary.xml";
            new XmlParser().Parse(path);

            //foreach (var file in files)
            //{
            //    var assembly = Assembly.LoadFrom(file);
            //    var loaded = assembly.GetExportedTypes().Where(x => x.IsInterface);

            //    foreach (var type in loaded)
            //    {
            //       sb.AppendLine("");
            //       sb.AppendLine($"Interface: {type.Name}");
            //        var methods = type.GetMethods();

            //        foreach (var methodInfo in methods)
            //        {
            //            sb.AppendLine($"Method: {methodInfo.Name}");
            //            sb.AppendLine($"Returns: {methodInfo.ReturnType}");

            //            var parameters = methodInfo.GetParameters().Select(x => new string($"{x.ParameterType.Name} {x.Name}".ToCharArray()));
            //            sb.AppendLine($"Parameters: {string.Join(", ", parameters)}");
            //            sb.AppendLine("");

            //            //var name = methodInfo.DeclaringType?.Name;
            //            //var ns = methodInfo.DeclaringType?.Namespace;
            //            //var rt1 = methodInfo.ReturnType.GenericTypeArguments[0].Name;

            //            //foreach (var parameterInfo in param)
            //            //{
            //            //    var p = parameterInfo.Name;
            //            //    var t = parameterInfo.ParameterType.Name;
            //            //}
            //        }

            //    }
            //}

            //File.WriteAllText(Path.Combine(myDocuments,"dll", "dll.txt"), sb.ToString());

            //Console.WriteLine(sb.ToString());
            Console.WriteLine("Done!");
            Console.ReadLine();
        }
    }
}