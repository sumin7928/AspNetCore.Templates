using ApiServer.Core.Swagger.Docs;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace ApiServer.Core.Swagger
{
    public class SwaggerDescriptionAttribute : SwaggerOperationAttribute
    {
        public SwaggerDescriptionAttribute(Type requestBody = null, Type responseBody = null)
        {
            SetDetails(null, null, null, requestBody, responseBody);
        }

        public SwaggerDescriptionAttribute(string tagName, Type requestBody = null, Type responseBody = null)
        {
            SetDetails(tagName, null, null, requestBody, responseBody);
        }

        public SwaggerDescriptionAttribute(string tagName, string summary, Type requestBody = null, Type responseBody = null)
        {
            SetDetails(tagName, summary, null, requestBody, responseBody);
        }

        public SwaggerDescriptionAttribute(string tagName, string summary, string description, Type requestBody = null, Type responseBody = null)
        {
            SetDetails(tagName, summary, description, requestBody, responseBody);
        }

        private void SetDetails(string tagName, string summary, string description, Type requestBody = null, Type responseBody = null)
        {
            if (tagName != null)
            {
                Tags = new[] { tagName };
            }
            if (summary != null)
            {
                Summary = summary;
            }
            Description = MakeDescription(description, requestBody, responseBody);
        }

        private string MakeDescription(string description, Type requestBody, Type responseBody)
        {
            StringBuilder sb = new StringBuilder();

            if (description != null)
            {
                sb.AppendLine(description).AppendLine();
            }

            sb.AppendLine($"### api web packet body description ###");

            if (requestBody != null)
            {
                sb.AppendLine($"## Request Body ##");
                MakeTableDescription(requestBody, sb);
            }
            if (responseBody != null)
            {
                sb.AppendLine($"## Response Body ##");
                MakeTableDescription(responseBody, sb);
            }

            return sb.ToString();
        }

        private static void MakeTableDescription(Type type, StringBuilder sb)
        {
            sb.AppendLine($"### {type.Name} - {SwaggerCustomDescription.GetComments($"{type.Namespace}.{type.Name}")} ###");
            sb.AppendLine("| Name | Data Type | Sub Type | Description |");
            sb.AppendLine("| --- | --- | --- | --- |");

            foreach (var properties in type.GetProperties())
            {
                string key = $"{type.Namespace}.{type.Name}.{properties.Name}";
                AppendDataLine(sb, properties.PropertyType, properties.Name, key);
            }

            foreach (var field in type.GetFields())
            {
                string key = $"{type.Namespace}.{type.Name}.{field.Name}";
                AppendDataLine(sb, field.FieldType, field.Name, key);
            }
        }

        private static void AppendDataLine(StringBuilder sb, Type type, string name, string key)
        {
            if (type.Name.Contains("List"))
            {
                string listName = type.ToString();
                string subString = listName.Substring(listName.IndexOf("List"));
                string[] splitedString = subString.Split('.');
                string typeName = $"List[{splitedString[splitedString.Length - 1].Split(']')[0]}]";

                sb.AppendLine($"| {name} | {typeName} | | {SwaggerCustomDescription.GetComments(key)} |");

                var list = type.GenericTypeArguments[0];
                if (list.Namespace.Contains("Models"))
                {
                    foreach (var listField in list.GetFields())
                    {
                        string listKey = $"{list.Name}.{listField.Name}";
                        string value = SwaggerCustomDescription.GetComments(listKey);
                        if (value == null)
                        {
                            // 상속 받은 경우 부모 클래스의 정보 가져옴
                            listKey = $"{listField.DeclaringType.Name}.{listField.Name}";
                            sb.AppendLine($"| | | {listField.Name} [{listField.FieldType.Name}] | {SwaggerCustomDescription.GetComments(listKey)} |");
                        }
                        else
                        {

                        }
                    }
                    foreach (var listProperties in list.GetProperties())
                    {
                        string listKey = $"{list.Name}.{listProperties.Name}";
                        string value = SwaggerCustomDescription.GetComments(listKey);
                        if (value == null)
                        {
                            // 상속 받은 경우 부모 클래스의 정보 가져옴
                            listKey = $"{listProperties.DeclaringType.Name}.{listProperties.Name}";
                        }
                        sb.AppendLine($"| | | {listProperties.Name} [{listProperties.PropertyType.Name}] | {SwaggerCustomDescription.GetComments(listKey)} |");
                    }
                }
            }
            else if (type.Name.Contains("Dictionary"))
            {
                string dicsName = type.ToString();
                string subString = dicsName.Substring(dicsName.IndexOf("Dictionary"));
                string[] splitedString = subString.Split('.');
                string typeName = $"List[{splitedString[splitedString.Length - 1].Split(']')[0]}]";

                subString = subString.Replace("System.", "");
                subString = subString.Replace("ApiWebServer.Models.", "");

                sb.AppendLine($"| {name} | {subString} | | {SwaggerCustomDescription.GetComments(key)} |");

                // 키에 대해서는 데이터를 표현할 수 없음으로 처리..
                //var dicKeys = field.FieldType.GenericTypeArguments[ 0 ];

                var dicValues = type.GenericTypeArguments[1];

                if (dicValues.Namespace.Contains("Models"))
                {
                    foreach (var valueField in dicValues.GetFields())
                    {
                        string listValue = $"{dicValues.Name}.{valueField.Name}";

                        if (valueField.FieldType.Name.Contains("Dictionary"))
                        {
                            string subDicsName = valueField.FieldType.ToString();
                            string subName = subDicsName.Substring(subDicsName.IndexOf("Dictionary"));
                            subName = subName.Replace("System.", "");
                            subName = subName.Replace("ApiWebServer.Models.", "");

                            sb.AppendLine($"| | | {valueField.Name} [{subName}] | {SwaggerCustomDescription.GetComments(listValue)} |");
                            continue;
                        }

                        sb.AppendLine($"| | | {valueField.Name} [{valueField.FieldType.Name}] | {SwaggerCustomDescription.GetComments(listValue)} |");
                    }
                }
            }
            else if (type.Namespace.Contains("Models"))
            {
                sb.AppendLine($"| {name} | {type.Name} | | {SwaggerCustomDescription.GetComments(key)} |");

                foreach (var entityField in type.GetFields())
                {
                    string listKey = $"{type.Name}.{entityField.Name}";
                    string value = SwaggerCustomDescription.GetComments(listKey);
                    if (value == null)
                    {
                        // 상속 받은 경우 부모 클래스의 정보 가져옴
                        listKey = $"{entityField.DeclaringType.Name}.{entityField.Name}";
                    }
                    else
                    {
                        if (entityField.FieldType.Name.Contains("Dictionary"))
                        {
                            string subDicsName = entityField.FieldType.ToString();
                            string subName = subDicsName.Substring(subDicsName.IndexOf("Dictionary"));
                            subName = subName.Replace("System.", "");
                            subName = subName.Replace("ApiWebServer.Models.", "");

                            sb.AppendLine($"| | | {entityField.Name} [{subName}] | {SwaggerCustomDescription.GetComments(listKey)} |");
                            continue;
                        }
                    }

                    sb.AppendLine($"| | | {entityField.Name} [{entityField.FieldType.Name}] | {SwaggerCustomDescription.GetComments(listKey)} |");
                }
            }
            else
            {
                sb.AppendLine($"| {name} | {type.Name} | | {SwaggerCustomDescription.GetComments(key)} |");
            }
        }
    }
}
