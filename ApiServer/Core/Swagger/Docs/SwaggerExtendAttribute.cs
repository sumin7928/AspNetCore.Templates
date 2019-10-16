using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace ApiWebServer.Core.Swagger
{
    public class SwaggerExtendAttribute : SwaggerOperationAttribute
    {

        public SwaggerExtendAttribute(string groupName, Type packet)
        {
            StringBuilder sb = new StringBuilder();

            FieldInfo fieldInfo = packet.GetField("Path");
            sb.AppendLine($"### api web packet path : {fieldInfo.GetValue(packet)} ###");

            Type[] arguments = packet.BaseType.GenericTypeArguments;

            // request data
            MakeTableDescription(arguments[0], sb);
            // response data
            MakeTableDescription(arguments[1], sb);

            Summary = SwaggerApiWebPacketDocs.GetComments(packet.Name);
            Description = sb.ToString();
            Tags = new[] { groupName };
        }

        private static void MakeTableDescription(Type type, StringBuilder sb)
        {
            sb.AppendLine($"## {type.Name} ##");
            sb.AppendLine("| Name | Data Type | Sub Type | Description |");
            sb.AppendLine("| --- | --- | --- | --- |");

            foreach (var properties in type.GetProperties())
            {
                string key = $"{type.Name}.{properties.Name}";
                AppendDataLine(sb, properties.PropertyType, properties.Name, key);
            }

            foreach (var field in type.GetFields())
            {
                string key = $"{type.Name}.{field.Name}";
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

                sb.AppendLine($"| {name} | {typeName} | | {SwaggerApiWebPacketDocs.GetComments(key)} |");

                var list = type.GenericTypeArguments[0];
                if (list.Namespace.Contains("Entity"))
                {
                    foreach (var listField in list.GetFields())
                    {
                        string listKey = $"{list.Name}.{listField.Name}";
                        string value = SwaggerApiWebPacketDocs.GetComments(listKey);
                        if (value == null)
                        {
                            // 상속 받은 경우 부모 클래스의 정보 가져옴
                            listKey = $"{listField.DeclaringType.Name}.{listField.Name}";
                            sb.AppendLine($"| | | {listField.Name} [{listField.FieldType.Name}] | {SwaggerApiWebPacketDocs.GetComments(listKey)} |");
                        }
                        else
                        {

                        }
                    }
                    foreach (var listProperties in list.GetProperties())
                    {
                        string listKey = $"{list.Name}.{listProperties.Name}";
                        string value = SwaggerApiWebPacketDocs.GetComments(listKey);
                        if (value == null)
                        {
                            // 상속 받은 경우 부모 클래스의 정보 가져옴
                            listKey = $"{listProperties.DeclaringType.Name}.{listProperties.Name}";
                        }
                        sb.AppendLine($"| | | {listProperties.Name} [{listProperties.PropertyType.Name}] | {SwaggerApiWebPacketDocs.GetComments(listKey)} |");
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
                subString = subString.Replace("WebSharedLib.Entity.", "");

                sb.AppendLine($"| {name} | {subString} | | {SwaggerApiWebPacketDocs.GetComments(key)} |");

                // 키에 대해서는 데이터를 표현할 수 없음으로 처리..
                //var dicKeys = field.FieldType.GenericTypeArguments[ 0 ];

                var dicValues = type.GenericTypeArguments[1];

                if (dicValues.Namespace.Contains("Entity"))
                {
                    foreach (var valueField in dicValues.GetFields())
                    {
                        string listValue = $"{dicValues.Name}.{valueField.Name}";

                        if (valueField.FieldType.Name.Contains("Dictionary"))
                        {
                            string subDicsName = valueField.FieldType.ToString();
                            string subName = subDicsName.Substring(subDicsName.IndexOf("Dictionary"));
                            subName = subName.Replace("System.", "");
                            subName = subName.Replace("WebSharedLib.Entity.", "");

                            sb.AppendLine($"| | | {valueField.Name} [{subName}] | {SwaggerApiWebPacketDocs.GetComments(listValue)} |");
                            continue;
                        }

                        sb.AppendLine($"| | | {valueField.Name} [{valueField.FieldType.Name}] | {SwaggerApiWebPacketDocs.GetComments(listValue)} |");
                    }
                }
            }
            else if (type.Namespace.Contains("Entity"))
            {
                sb.AppendLine($"| {name} | {type.Name} | | {SwaggerApiWebPacketDocs.GetComments(key)} |");

                foreach (var entityField in type.GetFields())
                {
                    string listKey = $"{type.Name}.{entityField.Name}";
                    string value = SwaggerApiWebPacketDocs.GetComments(listKey);
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
                            subName = subName.Replace("WebSharedLib.Entity.", "");

                            sb.AppendLine($"| | | {entityField.Name} [{subName}] | {SwaggerApiWebPacketDocs.GetComments(listKey)} |");
                            continue;
                        }
                    }

                    sb.AppendLine($"| | | {entityField.Name} [{entityField.FieldType.Name}] | {SwaggerApiWebPacketDocs.GetComments(listKey)} |");
                }
            }
            else
            {
                sb.AppendLine($"| {name} | {type.Name} | | {SwaggerApiWebPacketDocs.GetComments(key)} |");
            }
        }
    }
}
