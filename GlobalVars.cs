using System;
using System.Configuration;

namespace APIGateways
{
    public class GlobalVars
    {
        // You need to add app.config to the projest's directory manually
        public static readonly string secret = ConfigurationManager.AppSettings["Secret"];
        public static readonly string domain = ConfigurationManager.AppSettings["Domain"];
        public static readonly string listenPort = ConfigurationManager.AppSettings["ListenPort"];
        public static readonly string[] corsDomains = ConfigurationManager.AppSettings["CorsDomains"].Split(",");
    }
}
