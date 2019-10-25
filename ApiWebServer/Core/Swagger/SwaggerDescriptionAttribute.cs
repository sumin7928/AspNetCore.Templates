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


        private static StringBuilder MakeTableDescription(Type type, StringBuilder sb)
        {
            sb.AppendLine($"## {type.FullName} [{SwaggerCustomDescription.GetComments($"{type.FullName}")}] ##");
            sb.AppendLine("| Variable Name | Data Type | Data Detail | Description |");
            sb.AppendLine("| --- | --- | --- | --- |");

            foreach (var field in type.GetFields())
            {
                string key = $"{type.Namespace}.{type.Name}.{field.Name}";
                AppendDataLine(sb, field.FieldType, field.Name, key);
            }

            foreach (var properties in type.GetProperties())
            {
                string key = $"{type.Namespace}.{type.Name}.{properties.Name}";
                AppendDataLine(sb, properties.PropertyType, properties.Name, key);
            }

            return sb;
        }

        private static void AppendEntityDataLine(StringBuilder sb, Type type)
        {
            foreach (var entityField in type.GetFields())
            {
                string key = $"{type.Namespace}.{type.Name}.{entityField.Name}";
                string value = SwaggerCustomDescription.GetComments(key);
                if (value == null)
                {
                    // 상속 받은 경우 부모 클래스의 정보 가져옴
                    key = $"{type.Namespace}.{entityField.DeclaringType.Name}.{entityField.Name}";
                }

                AppendLastLine(sb, entityField.FieldType, entityField.Name, key);
            }

            foreach (var propertiesField in type.GetProperties())
            {
                string key = $"{type.Namespace}.{type.Name}.{propertiesField.Name}";
                string value = SwaggerCustomDescription.GetComments(key);
                if (value == null)
                {
                    // 상속 받은 경우 부모 클래스의 정보 가져옴
                    key = $"{type.Namespace}.{propertiesField.DeclaringType.Name}.{propertiesField.Name}";
                }

                AppendLastLine(sb, propertiesField.PropertyType, propertiesField.Name, key);
            }
        }

        private static void AppendLastLine(StringBuilder sb, Type type, string name, string key)
        {
            if (type.Namespace.Contains("System.Collections"))
            {
                if (type.Name.Contains("List"))
                {
                    if (type.GenericTypeArguments.Length > 0)
                    {
                        Type argType = type.GenericTypeArguments[0];
                        string typeName = $"List [{argType.Name}]";
                        sb.AppendLine($"| &nbsp; | | {name} [{typeName}] | {SwaggerCustomDescription.GetComments(key)} |");
                    }
                }
                else if (type.Name.Contains("Dictionary"))
                {
                    if (type.GenericTypeArguments.Length > 1)
                    {
                        Type argTypeKey = type.GenericTypeArguments[0];
                        Type argTypeValue = type.GenericTypeArguments[1];
                        string typeName = string.Empty;

                        if (argTypeValue.Namespace.Contains("System.Collections") && argTypeValue.Name.Contains("List"))
                        {
                            if (argTypeValue.GenericTypeArguments.Length > 0)
                            {
                                Type argType = argTypeValue.GenericTypeArguments[0];
                                typeName = $"Dictionary <{argTypeKey.Name},List [{argType.Name}]>";
                            }
                        }
                        else
                        {
                            typeName = $"Dictionary <{argTypeKey.Name},{argTypeValue.Name}>";
                        }
                        sb.AppendLine($"| &nbsp; | | {name} [{typeName}] | {SwaggerCustomDescription.GetComments(key)} |");
                    }
                }
            }
            else
            {
                sb.AppendLine($"| &nbsp; | | {name} [{type.Name}] | {SwaggerCustomDescription.GetComments(key)} |");
            }
        }

        private static void AppendDataLine(StringBuilder sb, Type type, string name, string key, bool isInner = false)
        {
            if (type.Namespace.Contains("System.Collections"))
            {
                if (type.Name.Contains("List"))
                {
                    if (type.GenericTypeArguments.Length > 0)
                    {
                        Type argType = type.GenericTypeArguments[0];

                        string typeName = $"List [{argType.Name}]";
                        sb.AppendLine($"| {name} | {typeName} | | {SwaggerCustomDescription.GetComments(key)} |");

                        if (argType.Namespace.Contains("ApiServer.Models"))
                        {
                            AppendEntityDataLine(sb, argType);
                        }
                    }
                }
                else if (type.Name.Contains("Dictionary"))
                {
                    if (type.GenericTypeArguments.Length > 1)
                    {
                        Type argTypeKey = type.GenericTypeArguments[0];
                        Type argTypeValue = type.GenericTypeArguments[1];

                        string typeName = $"Dictionary <{argTypeKey.Name},{argTypeValue.Name}>";
                        sb.AppendLine($"| {name} | {typeName} | | {SwaggerCustomDescription.GetComments(key)} |");

                        // value 타입에 대해서만 추가 문서 작성
                        if (argTypeValue.Namespace.Contains("ApiServer.Models"))
                        {
                            AppendEntityDataLine(sb, argTypeValue);
                        }
                    }
                }
            }
            else if (type.Namespace.Contains("ApiServer.Models"))
            {
                sb.AppendLine($"| {name} | {type.Name} | | {SwaggerCustomDescription.GetComments(key)} |");
                AppendEntityDataLine(sb, type);
            }
            else
            {
                if (isInner != true)
                {
                    sb.AppendLine($"| {name} | {type.Name} | | {SwaggerCustomDescription.GetComments(key)} |");
                }
            }
        }
    }
}
