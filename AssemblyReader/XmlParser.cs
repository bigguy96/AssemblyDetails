using System;
using System.Linq;
using System.Xml.Linq;

namespace AssemblyReader
{
    class XmlParser
    {
        internal void Parse(string path)
        {
            //const string path = @"C:\";
            var root = XElement.Load(path);
            var w = root.Elements("members").Descendants("member")
                .Select(x => new
                {
                    Name = x.Attribute("name").Value,
                    Values = x.Descendants().Select(s => new
                    {
                        Name = s.Name.LocalName.Trim(),
                        Value = s.Value.Trim()
                    })
                });

            foreach (var el in w)
            {
                Console.WriteLine(el.Name);

                foreach (var item in el.Values)
                {
                    Console.WriteLine(item.Name);
                    Console.WriteLine("\t" + item.Value);
                }
                Console.WriteLine();
            }

            Console.ReadKey();
            Console.WriteLine("Done!");
        }
    }
}