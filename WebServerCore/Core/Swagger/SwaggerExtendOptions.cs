using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiWebServer.Core.Swagger
{
    public static class SwaggerExtendOptions 
    {
        public static void Enable( this SwaggerGenOptions options )
        {
            options.SwaggerDoc( "client", new Info
            {
                Title = "Game Server Client API Controllers",
                Description = "클라이언트 연동을 위한 게임 서버 내에서 사용되는 API를 기반으로 요청 Url과 Data Description을 제공합니다.\n\n"
                                    + "궁금한 사항이나 개선 사항이 있으면 마구 감독판 서버팀에 요청\n",

                Contact = new Contact
                {
                    Name = "게임 서버 정보 - Game Server Gitlab",
                    Email = string.Empty,
                    Url = "http://183.110.18.41/magu-mobile-server/NewMaguMagu"
                }
            } );
            options.SwaggerDoc( "admin", new Info
            {
                Title = "Game Server Admin API Controllers",
                Description = "관리자 API를 기반으로 요청 Url과 Data Description을 제공하며 허용된 PC 에서만 요청 가능합니다.\n\n"
                                + "추가 기능 요청 시 감독판 서버팀에 문의 요청\n\n"
                                + "사용법은 해당 요청 -> [Try it out] -> 파라미터 입력 후 -> [Execute] \n",

                Contact = new Contact
                {
                    Name = "Game Server Gitlab",
                    Email = string.Empty,
                    Url = "http://183.110.18.41/magu-mobile-server/NewMaguMagu"
                }
            } );

            options.TagActionsBy( api =>
            {
                string[] splited = api.RelativePath.Split("/");
                if (splited == null || splited.Length < 2)
                {
                    return "Not Found Path";
                }
                if ( api.GroupName == "admin" )
                {
                    return splited[2] + " Controllers";
                }
                else
                {
                    if (splited.Length > 2)
                    {
                        return splited[1] + " Controllers";
                    }
                    else
                    {
                        return splited[0] + " Controllers";
                    }
                }
            } );

            AnnotationsSwaggerGenOptionsExtensions.EnableAnnotations( options );
        }
    }
}
