using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace AssemblyReader
{
    /// <summary>
    /// Xml document parser
    /// </summary>
    public static class XmlDocumentReader
    {
        #region System.Reflection.Assembly       

        /// <summary>Gets the file path of an assembly.</summary>
        /// <param name="assembly">The assembly to get the file path of.</param>
        /// <returns>The file path of the assembly.</returns>
        public static string GetDirectoryPath(this Assembly assembly)
        {
            string codeBase = assembly.CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        #endregion

        #region XML Code Documentation

        internal static System.Collections.Generic.HashSet<Assembly> loadedAssemblies = new System.Collections.Generic.HashSet<Assembly>();
        internal static System.Collections.Generic.Dictionary<string, string> loadedXmlDocumentation = new System.Collections.Generic.Dictionary<string, string>();

        internal static void LoadXmlDocumentation(Assembly assembly)
        {
            if (loadedAssemblies.Contains(assembly))
            {
                return;
            }
            string directoryPath = assembly.GetDirectoryPath();
            string xmlFilePath = Path.Combine(directoryPath, assembly.GetName().Name + ".xml");
            if (File.Exists(xmlFilePath))
            {
                using StreamReader streamReader = new StreamReader(xmlFilePath);
                LoadXmlDocumentation(streamReader);
            }
            // currently marking assembly as loaded even if the XML file was not found
            // may want to adjust in future, but I think this is good for now
            loadedAssemblies.Add(assembly);
        }

        /// <summary>Loads the XML code documentation into memory so it can be accessed by extension methods on reflection types.</summary>
        /// <param name="xmlDocumentation">The content of the XML code documentation.</param>
        public static void LoadXmlDocumentation(string xmlDocumentation)
        {
            if (!File.Exists(xmlDocumentation))
            {
                throw new Exception("File doesn't exist");
            }

            //https://www.ipreferjim.com/2014/09/data-at-the-root-level-is-invalid-line-1-position-1/
            var xml = File.ReadAllText(xmlDocumentation);
            //var byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble()); 
            //if (xml.StartsWith(byteOrderMarkUtf8)) { xml = xml.Remove(0, byteOrderMarkUtf8.Length); }

            using StringReader stringReader = new StringReader(xml);
            LoadXmlDocumentation(stringReader);
        }

        /// <summary>Loads the XML code documentation into memory so it can be accessed by extension methods on reflection types.</summary>
        /// <param name="textReader">The text reader to process in an XmlReader.</param>
        public static void LoadXmlDocumentation(TextReader textReader)
        {
            //var r = new XmlReaderSettings
            //{
            //    ValidationType = ValidationType.None
            //};
            using XmlReader xmlReader = XmlReader.Create(textReader);
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "member")
                {
                    string raw_name = xmlReader["name"];
                    loadedXmlDocumentation[raw_name] = xmlReader.ReadInnerXml();
                }
            }
        }

        /// <summary>Clears the currently loaded XML documentation.</summary>
        public static void ClearXmlDocumentation()
        {
            loadedAssemblies.Clear();
            loadedXmlDocumentation.Clear();
        }

        /// <summary>Gets the XML documentation on a type.</summary>
        /// <param name="type">The type to get the XML documentation of.</param>
        /// <returns>The XML documentation on the type.</returns>
        /// <remarks>The XML documentation must be loaded into memory for this function to work.</remarks>
        public static string GetDocumentation(this Type type)
        {
            LoadXmlDocumentation(type.Assembly);
            string key = "T:" + XmlDocumentationKeyHelper(type.FullName, null);
            loadedXmlDocumentation.TryGetValue(key, out string documentation);
            return documentation;
        }

        /// <summary>Gets the XML documentation on a method.</summary>
        /// <param name="methodInfo">The method to get the XML documentation of.</param>
        /// <returns>The XML documentation on the method.</returns>
        /// <remarks>The XML documentation must be loaded into memory for this function to work.</remarks>
        public static string GetDocumentation(this MethodInfo methodInfo)
        {
            LoadXmlDocumentation(methodInfo.DeclaringType.Assembly);

            System.Collections.Generic.Dictionary<string, int> typeGenericMap = new System.Collections.Generic.Dictionary<string, int>();
            int tempTypeGeneric = 0;
            Array.ForEach(methodInfo.DeclaringType.GetGenericArguments(), x => typeGenericMap[x.Name] = tempTypeGeneric++);

            System.Collections.Generic.Dictionary<string, int> methodGenericMap = new System.Collections.Generic.Dictionary<string, int>();
            int tempMethodGeneric = 0;
            Array.ForEach(methodInfo.GetGenericArguments(), x => methodGenericMap.Add(x.Name, tempMethodGeneric++));

            ParameterInfo[] parameterInfos = methodInfo.GetParameters();

            string memberTypePrefix = "M:";
            string declarationTypeString = GetXmlDocumenationFormattedString(methodInfo.DeclaringType, false, typeGenericMap, methodGenericMap);
            string memberNameString = methodInfo.Name;
            string methodGenericArgumentsString =
                methodGenericMap.Count > 0 ?
                "``" + methodGenericMap.Count :
                string.Empty;
            string parametersString =
                parameterInfos.Length > 0 ?
                "(" + string.Join(",", methodInfo.GetParameters().Select(x => GetXmlDocumenationFormattedString(x.ParameterType, true, typeGenericMap, methodGenericMap))) + ")" :
                string.Empty;

            string key =
                memberTypePrefix +
                declarationTypeString +
                "." +
                memberNameString +
                methodGenericArgumentsString +
                parametersString;

            if (methodInfo.Name == "op_Implicit" ||
                methodInfo.Name == "op_Explicit")
            {
                key += "~" + GetXmlDocumenationFormattedString(methodInfo.ReturnType, true, typeGenericMap, methodGenericMap);
            }

            loadedXmlDocumentation.TryGetValue(key, out string documentation);
            return documentation;
        }

        /// <summary>Gets the XML documentation on a constructor.</summary>
        /// <param name="constructorInfo">The constructor to get the XML documentation of.</param>
        /// <returns>The XML documentation on the constructor.</returns>
        /// <remarks>The XML documentation must be loaded into memory for this function to work.</remarks>
        public static string GetDocumentation(this ConstructorInfo constructorInfo)
        {
            LoadXmlDocumentation(constructorInfo.DeclaringType.Assembly);

            System.Collections.Generic.Dictionary<string, int> typeGenericMap = new System.Collections.Generic.Dictionary<string, int>();
            int tempTypeGeneric = 0;
            Array.ForEach(constructorInfo.DeclaringType.GetGenericArguments(), x => typeGenericMap[x.Name] = tempTypeGeneric++);

            // constructors don't support generic types so this will always be empty
            System.Collections.Generic.Dictionary<string, int> methodGenericMap = new System.Collections.Generic.Dictionary<string, int>();

            ParameterInfo[] parameterInfos = constructorInfo.GetParameters();

            string memberTypePrefix = "M:";
            string declarationTypeString = GetXmlDocumenationFormattedString(constructorInfo.DeclaringType, false, typeGenericMap, methodGenericMap);
            string memberNameString = "#ctor";
            string parametersString =
                parameterInfos.Length > 0 ?
                "(" + string.Join(",", constructorInfo.GetParameters().Select(x => GetXmlDocumenationFormattedString(x.ParameterType, true, typeGenericMap, methodGenericMap))) + ")" :
                string.Empty;

            string key =
                memberTypePrefix +
                declarationTypeString +
                "." +
                memberNameString +
                parametersString;

            loadedXmlDocumentation.TryGetValue(key, out string documentation);
            return documentation;
        }

        internal static string GetXmlDocumenationFormattedString(
            Type type,
            bool isMethodParameter,
            System.Collections.Generic.Dictionary<string, int> typeGenericMap,
            System.Collections.Generic.Dictionary<string, int> methodGenericMap)
        {
            if (type.IsGenericParameter)
            {
                return methodGenericMap.TryGetValue(type.Name, out int methodIndex)
                    ? "``" + methodIndex
                    : "`" + typeGenericMap[type.Name];
            }
            else if (type.HasElementType)
            {
                string elementTypeString = GetXmlDocumenationFormattedString(
                    type.GetElementType(),
                    isMethodParameter,
                    typeGenericMap,
                    methodGenericMap);

                if (type.IsPointer)
                {
                    return elementTypeString + "*";
                }
                else if (type.IsArray)
                {
                    int rank = type.GetArrayRank();
                    string arrayDimensionsString = rank > 1
                        ? "[" + string.Join(",", Enumerable.Repeat("0:", rank)) + "]"
                        : "[]";
                    return elementTypeString + arrayDimensionsString;
                }
                else if (type.IsByRef)
                {
                    return elementTypeString + "@";
                }
                else
                {
                    // Hopefully this will never hit. At the time of writing
                    // this code, type.HasElementType is only true if the type
                    // is a pointer, array, or by reference.
                    throw new Exception(nameof(GetXmlDocumenationFormattedString) +
                        " encountered an unhandled element type.");
                }
            }
            else
            {
                string prefaceString = type.IsNested
                    ? GetXmlDocumenationFormattedString(
                        type.DeclaringType,
                        isMethodParameter,
                        typeGenericMap,
                        methodGenericMap) + "."
                    : type.Namespace + ".";

                string typeNameString = isMethodParameter
                    ? typeNameString = Regex.Replace(type.Name, @"`\d+", string.Empty)
                    : typeNameString = type.Name;

                string genericArgumentsString = type.IsGenericType && isMethodParameter
                    ? "{" + string.Join(",",
                        type.GetGenericArguments().Select(argument =>
                            GetXmlDocumenationFormattedString(
                                argument,
                                isMethodParameter,
                                typeGenericMap,
                                methodGenericMap))
                        ) + "}"
                    : string.Empty;

                return prefaceString + typeNameString + genericArgumentsString;
            }
        }

        /// <summary>Gets the XML documentation on a property.</summary>
        /// <param name="propertyInfo">The property to get the XML documentation of.</param>
        /// <returns>The XML documentation on the property.</returns>
        /// <remarks>The XML documentation must be loaded into memory for this function to work.</remarks>
        public static string GetDocumentation(this PropertyInfo propertyInfo)
        {
            LoadXmlDocumentation(propertyInfo.DeclaringType.Assembly);
            string key = "P:" + XmlDocumentationKeyHelper(propertyInfo.DeclaringType.FullName, propertyInfo.Name);
            loadedXmlDocumentation.TryGetValue(key, out string documentation);
            return documentation;
        }

        /// <summary>Gets the XML documentation on a field.</summary>
        /// <param name="fieldInfo">The field to get the XML documentation of.</param>
        /// <returns>The XML documentation on the field.</returns>
        /// <remarks>The XML documentation must be loaded into memory for this function to work.</remarks>
        public static string GetDocumentation(this FieldInfo fieldInfo)
        {
            LoadXmlDocumentation(fieldInfo.DeclaringType.Assembly);
            string key = "F:" + XmlDocumentationKeyHelper(fieldInfo.DeclaringType.FullName, fieldInfo.Name);
            loadedXmlDocumentation.TryGetValue(key, out string documentation);
            return documentation;
        }

        /// <summary>Gets the XML documentation on an event.</summary>
        /// <param name="eventInfo">The event to get the XML documentation of.</param>
        /// <returns>The XML documentation on the event.</returns>
        /// <remarks>The XML documentation must be loaded into memory for this function to work.</remarks>
        public static string GetDocumentation(this EventInfo eventInfo)
        {
            LoadXmlDocumentation(eventInfo.DeclaringType.Assembly);
            string key = "E:" + XmlDocumentationKeyHelper(eventInfo.DeclaringType.FullName, eventInfo.Name);
            loadedXmlDocumentation.TryGetValue(key, out string documentation);
            return documentation;
        }

        internal static string XmlDocumentationKeyHelper(string typeFullNameString, string memberNameString)
        {
            string key = Regex.Replace(typeFullNameString, @"\[.*\]", string.Empty).Replace('+', '.');
            if (!(memberNameString is null))
            {
                key += "." + memberNameString;
            }
            return key;
        }

        /// <summary>Gets the XML documentation on a member.</summary>
        /// <param name="memberInfo">The member to get the XML documentation of.</param>
        /// <returns>The XML documentation on the member.</returns>
        /// <remarks>The XML documentation must be loaded into memory for this function to work.</remarks>
        public static string GetDocumentation(this MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo fieldInfo)
            {
                return fieldInfo.GetDocumentation();
            }
            else if (memberInfo is PropertyInfo propertyInfo)
            {
                return propertyInfo.GetDocumentation();
            }
            else if (memberInfo is EventInfo eventInfo)
            {
                return eventInfo.GetDocumentation();
            }
            else if (memberInfo is ConstructorInfo constructorInfo)
            {
                return constructorInfo.GetDocumentation();
            }
            else if (memberInfo is MethodInfo methodInfo)
            {
                return methodInfo.GetDocumentation();
            }
            else if (memberInfo is Type type) // + TypeInfo
            {
                return type.GetDocumentation();
            }
            else if (memberInfo.MemberType.HasFlag(MemberTypes.Custom))
            {
                // This represents a cutom type that is not part of
                // the standard .NET languages as far as I'm aware.
                // This will never be supported so return null.
                return null;
            }
            else
            {
                // Hopefully this will never hit. At the time of writing
                // this code, I am only aware of the following Member types:
                // FieldInfo, PropertyInfo, EventInfo, ConstructorInfo,
                // MethodInfo, and Type.
                throw new Exception(nameof(GetDocumentation) +
                    " encountered an unhandled type [" + memberInfo.GetType().FullName + "]. ");
            }
        }

        /// <summary>Gets the XML documentation for a parameter.</summary>
        /// <param name="parameterInfo">The parameter to get the XML documentation for.</param>
        /// <returns>The XML documenation of the parameter.</returns>
        public static string GetDocumentation(this ParameterInfo parameterInfo)
        {
            string memberDocumentation = parameterInfo.Member.GetDocumentation();
            if (!(memberDocumentation is null))
            {
                string regexPattern =
                    Regex.Escape(@"<param name=" + "\"" + parameterInfo.Name + "\"" + @">") +
                    ".*?" +
                    Regex.Escape(@"</param>");

                Match match = Regex.Match(memberDocumentation, regexPattern);
                if (match.Success)
                {
                    return match.Value;
                }
            }
            return null;
        }

        #endregion

        #region System.Reflection.MethodInfo

        /// <summary>Determines if a MethodInfo is a local function.</summary>
        /// <param name="methodInfo">The method info to determine if it is a local function.</param>
        /// <returns>True if the MethodInfo is a local function. False if not.</returns>
        public static bool IsLocalFunction(this MethodInfo methodInfo) =>
            Regex.Match(methodInfo.Name, @"g__.+\|\d+_\d+").Success;

        #endregion
    }
}
