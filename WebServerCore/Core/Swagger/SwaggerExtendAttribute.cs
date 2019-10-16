using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ApiWebServer.Core.Swagger
{
    public class SwaggerExtendAttribute : SwaggerOperationAttribute
    {
        private static Dictionary<string, string> _contentsMemberComments;
        private static Dictionary<string, string> _entityMemberComments;

        static SwaggerExtendAttribute()
        {
            string path = Path.Combine( AppContext.BaseDirectory, "WebSharedLib.xml" );
            if ( File.Exists( path ) )
            {
                var docs = XDocument.Load( path );
                var docsContentsComments = docs.Root.Element( "members" ).Elements( "member" ).Where( x =>
                {
                    if ( !x.Attribute( "name" ).Value.First().Equals( 'F' ) && !x.Attribute( "name" ).Value.First().Equals( 'T' ) )
                    {
                        return false;
                    }
                    if ( !x.Attribute( "name" ).Value.Contains( "Contents" ) )
                    {
                        return false;
                    }
                    return true;
                } ).ToList();

                if ( docsContentsComments.Count > 0 )
                {
                    _contentsMemberComments = new Dictionary<string, string>();
                    docsContentsComments.ForEach( x =>
                    {
                        string name = x.Attribute( "name" ).Value;
                        string key = string.Empty;
                        if( name.First().Equals( 'F' ) )
                        {
                            key = name.Substring( name.IndexOf( "Contents" ) + 9 );
                        }
                        else if( name.First().Equals( 'T' ) )
                        {
                            key = name.Substring( name.IndexOf( "Api" ) + 4 );
                        }
                        int start = x.Value.IndexOf( '\n' ) + 1;
                        int end = x.Value.LastIndexOf( '\n' );
                        string subString = x.Value.Substring( start, end - start );
                        _contentsMemberComments.TryAdd( key, subString.Replace( "\n", "<br></br>" ).Trim() );
                    } );
                }

                var docsEntitiyComments = docs.Root.Element( "members" ).Elements( "member" ).Where( x =>
                {
                    if ( !x.Attribute( "name" ).Value.First().Equals( 'F' ) && !x.Attribute( "name" ).Value.First().Equals( 'T' ) )
                    {
                        return false;
                    }
                    if ( !x.Attribute( "name" ).Value.Contains( "Entity" ) )
                    {
                        return false;
                    }
                    return true;
                } ).ToList();

                if ( docsEntitiyComments.Count > 0 )
                {
                    _entityMemberComments = new Dictionary<string, string>();
                    docsEntitiyComments.ForEach( x =>
                    {
                        string name = x.Attribute( "name" ).Value;
                        string key = string.Empty;
                        if ( name.First().Equals( 'F' ) )
                        {
                            key = name.Substring( name.IndexOf( "Entity" ) + 7 );
                        }
                        int start = x.Value.IndexOf( '\n' ) + 1;
                        int end = x.Value.LastIndexOf( '\n' );
                        string subString = x.Value.Substring( start, end - start );
                        _entityMemberComments.TryAdd( key, subString.Replace( "\n", "<br></br>" ).Trim() );
                    } );
                }
            }
        }

        public SwaggerExtendAttribute( string summary, Type packet ) : base( summary )
        {
            StringBuilder sb = new StringBuilder();

            var field = packet.GetField( "apiURL" );
            sb.AppendLine( $"### api path : {field.GetValue( packet )} ###" );
            sb.AppendLine( $"# {packet.Name} #" );
            sb.AppendLine( _contentsMemberComments.GetValueOrDefault( packet.Name ) );

            var arguments = packet.BaseType.GenericTypeArguments;
            MakeTableDescription( arguments[ 0 ], sb );
            MakeTableDescription( arguments[ 1 ], sb );

            Description = sb.ToString();
        }

        private static void MakeTableDescription( Type type, StringBuilder sb )
        {
            sb.AppendLine( $"## {type.Name} ##" );
            sb.AppendLine( "| Variable Name | Data Type | Data Detail | Description |" );
            sb.AppendLine( "| --- | --- | --- | --- |" );

            foreach( var field in type.GetFields() )
            {
                string key = $"{type.Name}.{field.Name}";

                if ( field.FieldType.Name.Contains( "List" ) )
                {
                    string listName = field.FieldType.ToString();
                    string subString = listName.Substring( listName.IndexOf( "List" ) );
                    subString = subString.Replace( "System.", "" );
                    subString = subString.Replace( "WebSharedLib.Entity.", "" );

                    sb.AppendLine( $"| {field.Name} | {subString} | | {_contentsMemberComments.GetValueOrDefault( key )} |" );

                    var list = field.FieldType.GenericTypeArguments[ 0 ];
                    if( list.Namespace.Contains( "Entity" ) )
                    {
                        foreach ( var listField in list.GetFields() )
                        {
                            string listKey = $"{list.Name}.{listField.Name}";
                            string value = _entityMemberComments.GetValueOrDefault( listKey );
                            if( value == null )
                            {
                                // 상속 받은 경우 부모 클래스의 정보 가져옴
                                listKey = $"{listField.DeclaringType.Name}.{listField.Name}";
                            }
                            sb.AppendLine( $"| | | {listField.Name} [{listField.FieldType.Name}] | {_entityMemberComments.GetValueOrDefault( listKey )} |" );
                        }
                    }

                    continue;
                }
                if( field.FieldType.Name.Contains( "Dictionary") )
                {
                    string dicsName = field.FieldType.ToString();
                    string subString = dicsName.Substring( dicsName.IndexOf( "Dictionary" ) );
                    subString = subString.Replace( "System.", "" );
                    subString = subString.Replace( "WebSharedLib.Entity.", "" );

                    sb.AppendLine( $"| {field.Name} | {subString} | | {_contentsMemberComments.GetValueOrDefault( key )} |" );

                    // 키에 대해서는 데이터를 표현할 수 없음으로 처리..
                    //var dicKeys = field.FieldType.GenericTypeArguments[ 0 ];

                    var dicValues = field.FieldType.GenericTypeArguments[ 1 ];

                    if ( dicValues.Namespace.Contains( "Entity" ) )
                    {
                        foreach ( var valueField in dicValues.GetFields() )
                        {
                            string listValue = $"{dicValues.Name}.{valueField.Name}";

                            if ( valueField.FieldType.Name.Contains( "Dictionary" ) )
                            {
                                string subDicsName = valueField.FieldType.ToString();
                                string subName = subDicsName.Substring( subDicsName.IndexOf( "Dictionary" ) );
                                subName = subName.Replace( "System.", "" );
                                subName = subName.Replace( "WebSharedLib.Entity.", "" );

                                sb.AppendLine( $"| | | {valueField.Name} [{subName}] | {_entityMemberComments.GetValueOrDefault( listValue )} |" );
                                continue;
                            }

                            sb.AppendLine( $"| | | {valueField.Name} [{valueField.FieldType.Name}] | {_entityMemberComments.GetValueOrDefault( listValue )} |" );
                        }
                    }

                    continue;
                }

                // 배열이 아닐 경우 일반 처리
                sb.AppendLine( $"| {field.Name} | {field.FieldType.Name} | | {_contentsMemberComments.GetValueOrDefault( key )} |" );

                if ( field.FieldType.Namespace.Contains( "Entity" ) )
                {
                    foreach ( var entityField in field.FieldType.GetFields() )
                    {
                        string listKey = $"{field.FieldType.Name}.{entityField.Name}";
                        string value = _entityMemberComments.GetValueOrDefault( listKey );
                        if ( value == null )
                        {
                            // 상속 받은 경우 부모 클래스의 정보 가져옴
                            listKey = $"{entityField.DeclaringType.Name}.{entityField.Name}";
                        }
                        else
                        {
                            if ( entityField.FieldType.Name.Contains( "Dictionary" ) )
                            {
                                string subDicsName = entityField.FieldType.ToString();
                                string subName = subDicsName.Substring( subDicsName.IndexOf( "Dictionary" ) );
                                subName = subName.Replace( "System.", "" );
                                subName = subName.Replace( "WebSharedLib.Entity.", "" );

                                sb.AppendLine( $"| | | {entityField.Name} [{subName}] | {_entityMemberComments.GetValueOrDefault( listKey )} |" );
                                continue;
                            }
                        }

                        sb.AppendLine( $"| | | {entityField.Name} [{entityField.FieldType.Name}] | {_entityMemberComments.GetValueOrDefault( listKey )} |" );
                    }
                }

            }
        }
    }
}
