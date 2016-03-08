using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

namespace Bluewire.Common.Console.Hosting
{
    public class BindingRedirects : IEnumerable<XmlElement>
    {
        private const string DependentAssemblyElementNamespace = "urn:schemas-microsoft-com:asm.v1";

        private readonly ICollection<XmlElement> elements;
        
        public BindingRedirects(ICollection<XmlElement> elements)
        {
            if (GetUnexpectedNamespaces(elements).Any()) throw new ArgumentException($"One or more elements lie in a namespace other than {DependentAssemblyElementNamespace}.");
            this.elements = elements;
        }

        public int Count => elements.Count;

        public IEnumerator<XmlElement> GetEnumerator()
        {
            return elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static bool IsDependentAssemblyElement(XmlElement element)
        {
            return element.Name == "dependentAssembly" && element.NamespaceURI == DependentAssemblyElementNamespace;
        }

        private static string[] GetUnexpectedNamespaces(ICollection<XmlElement> elements)
        {
            return elements
                .Where(e => !IsDependentAssemblyElement(e))
                .Select(e => e.NamespaceURI)
                .Distinct()
                .ToArray();
        }

        public static BindingRedirects ReadFrom(XmlDocument configuration)
        {
            var namespaces = CreateNamespaceManager(configuration.NameTable);

            var nodes = configuration.SelectNodes("/configuration/runtime/asmv1:assemblyBinding/asmv1:dependentAssembly", namespaces)?.Cast<XmlNode>().ToArray();
            // I have no idea if this can even happen, but throwing a useful exception will help if it ever does.
            if(nodes == null || !nodes.All(n => n is XmlElement)) throw new XmlException("Could not read binding redirects from configuration. 'dependentAssembly' nodes were not of the expected type.");
            
            return new BindingRedirects(nodes.Cast<XmlElement>().ToList());
        }

        private static XmlNamespaceManager CreateNamespaceManager(XmlNameTable nameTable)
        {
            var namespaces = new XmlNamespaceManager(nameTable);
            namespaces.AddNamespace("asmv1", DependentAssemblyElementNamespace);
            return namespaces;
        }

        public static BindingRedirects ReadFrom(string filePath)
        {
            var xml = new XmlDocument();
            xml.Load(filePath);
            return ReadFrom(xml);
        }

        public void ApplyTo(XmlDocument configuration)
        {
            var runtimeContainer = configuration.SelectSingleNode("/configuration/runtime");
            if(runtimeContainer == null)
            {
                runtimeContainer = configuration.CreateElement("runtime");
                configuration.DocumentElement.AppendChild(runtimeContainer);
            }
            
            var namespaces = CreateNamespaceManager(configuration.NameTable);
            var assemblyBindingContainer = runtimeContainer.SelectNodes("./asmv1:assemblyBinding", namespaces)?.Cast<XmlElement>().LastOrDefault();
            if(assemblyBindingContainer == null)
            {
                assemblyBindingContainer = configuration.CreateElement("assemblyBinding", DependentAssemblyElementNamespace);
                runtimeContainer.AppendChild(assemblyBindingContainer);
            }

            foreach(var element in elements)
            {
                assemblyBindingContainer.AppendChild(configuration.ImportNode(element, true));
            }
        }
    }
}
