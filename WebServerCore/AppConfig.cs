using System.Text;
using WebSharedLib.Core.NPLib;

namespace ApiWebServer
{
    public static class AppConfig
    {
        // 파라미터 관련
        public static int ServerPort { get; set; }
        public static int ServerNumber { get; set; }
        public static bool IsRunGameServer { get; set; }
        public static bool IsRunChatServer { get; set; }

        // 암호화 관련 Keys
        public static byte[] ServerCryptKey { get; private set; } = Encoding.UTF8.GetBytes( "*fhqlsdlchlrhdi!" );
        public static byte[] ServerCryptIV { get; private set; } = Encoding.UTF8.GetBytes( "ckrnckrneoqkrsk!" );
        public static byte[] ClientCryptKey { get; private set; } = Encoding.UTF8.GetBytes( "*fhqlsdlchlrhdi!" );
        public static byte[] ClientCryptIV { get; private set; } = Encoding.UTF8.GetBytes( "ckrnckrneoqkrsk!" );
        public static byte[] LoginKey { get; private set; } = new byte[ NPCrypt.CRYPT_KEY_LEN ];
        public static byte[] LoginIV { get; private set; } = new byte[ NPCrypt.CRYPT_KEY_LEN ];
        public static byte[] ProfileURLKey { get; private set; } = Encoding.UTF8.GetBytes( "npark_profileurl" );
        public static byte[] ProfileURLIV { get; private set; } = Encoding.UTF8.GetBytes( "profileurl_iv_ab" );

    }
}
