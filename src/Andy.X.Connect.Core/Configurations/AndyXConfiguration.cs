namespace Andy.X.Connect.Core.Configurations
{
    public class AndyXConfiguration
    {
        public string[] BrokerServiceUrls { get; set; }
        public string Tenant { get; set; }
        public string Product { get; set; }
        public string Component { get; set; }

        public string TenantToken { get; set; }
        public string ComponentToken { get; set; }
    }
}
