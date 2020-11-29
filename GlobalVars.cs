using System;
using System.Configuration;

namespace APIGateways
{
    public static class GlobalVars
    {
        // You need to add app.config to the projest's directory manually
        public static readonly string secret = ConfigurationManager.AppSettings["Secret"].ToString();
        public static readonly string domain = ConfigurationManager.AppSettings["Domain"].ToString();
        public static readonly int listenPort = Convert.ToInt32(ConfigurationManager.AppSettings["Domain"]);
    }
}
