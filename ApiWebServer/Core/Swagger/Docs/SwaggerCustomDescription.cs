using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ApiWebServer.Core.Swagger.Docs
{
    public static class SwaggerCustomDescription
    {
        private static readonly string xmlDocsFile = "ApiWebServer.xml";
        private static readonly string[] targetNamespace = new string[] { "ApiWebServer.Models", "ApiWebServer.Controllers" }; 
        private static Dictionary<string, string> comments = new Dictionary<string, string>();

        public static void Initialize()
        {
            string path = Path.Combine(AppContext.BaseDirectory, xmlDocsFile);
            if (File.Exists(path) == false)
            {
                return;
            }

            XDocument docs = XDocument.Load(path);
            List<XElement> commentsList = docs.Root.Element("members").Elements("member").Where(x => CheckMember(x, targetNamespace)).ToList();
            commentsList.ForEach(x =>
            {
                string name = x.Attribute("name").Value;
                string key = name.Split(":")[1];
                int start = x.Value.IndexOf('\n') + 1;
                int end = x.Value.LastIndexOf('\n');
                string subString = x.Value.Substring(start, end - start);
                comments.Add(key, subString.Replace("\n", "<br></br>").Trim());
            });
        }

        public static string GetComments(string key)
        {
            if (comments.TryGetValue(key, out string value))
            {
                return value;
            }
            else
            {
                return string.Empty;
            }
        }

        private static bool CheckMember(XElement element, string[] containValue)
        {
            string member = element.Attribute("name").Value;

            if (!member.First().Equals('F')
                && !member.First().Equals('T')
                && !member.First().Equals('P'))
            {
                return false;
            }

            foreach (string value in containValue)
            {
                if(member.Contains(value))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
