using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ApiWebServer.Core.Swagger
{
    public static class SwaggerApiWebPacketDocs
    {
        private static Dictionary<string, string> _comments = new Dictionary<string, string>();

        public static string DocsFileName { get; set; } = "ApiWebPacket.xml";
        public static List<string> DocsTargetNamespaceList { get; set; } = new List<string>() { "Apis", "Entity" };

        public static void Initialize()
        {
            string path = Path.Combine(AppContext.BaseDirectory, DocsFileName);
            if (File.Exists(path) == false)
            {
                return;
            }

            string[] baseDocsName = DocsFileName.Split(".");
            if (baseDocsName.Length < 2)
            {
                return;
            }

            XDocument docs = XDocument.Load(path);
            foreach (string name in DocsTargetNamespaceList)
            {
                string targetNamespace = $"{baseDocsName[0]}.{name}";
                List<XElement> commentsList = docs.Root.Element("members").Elements("member").Where(x => CheckMember(x, targetNamespace)).ToList();
                AddComments(commentsList, targetNamespace);
            }
        }

        public static string GetComments(string key)
        {
            if (_comments.TryGetValue(key, out string value))
            {
                return value;
            }
            else
            {
                return string.Empty;
            }
        }

        private static bool CheckMember(XElement element, string containValue)
        {
            string member = element.Attribute("name").Value;

            if (!member.First().Equals('F')
                && !member.First().Equals('T')
                && !member.First().Equals('P'))
            {
                return false;
            }

            if (!member.Contains(containValue))
            {
                return false;
            }
            return true;
        }

        private static void AddComments(List<XElement> elements, string containValue)
        {
            elements.ForEach(x =>
            {
                string name = x.Attribute("name").Value;
                string key = name.Substring(name.IndexOf(containValue) + containValue.Length + 1);

                int start = x.Value.IndexOf('\n') + 1;
                int end = x.Value.LastIndexOf('\n');
                string subString = x.Value.Substring(start, end - start);
                _comments.Add(key, subString.Replace("\n", "<br></br>").Trim());
            });
        }
    }
}
