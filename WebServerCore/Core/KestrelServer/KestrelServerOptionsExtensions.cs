using System.Security.Cryptography.X509Certificates;

namespace ApiWebServer.Core.KestrelServer
{
    public static class KestrelServerOptionsExtensions
    {
        public static X509Certificate2 LoadCertificate( string path, string password )
        {
            return new X509Certificate2( path, password );
        }
    }
}
