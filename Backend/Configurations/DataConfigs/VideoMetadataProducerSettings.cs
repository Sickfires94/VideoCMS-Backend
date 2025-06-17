namespace Backend.Configurations.DataConfigs
{
    public class VideoMetadataProducerSettings
    {
        public string ExchangeName { get; set; } // Matches appsettings.json
        public string RoutingKey { get; set; }   // Matches appsettings.json
        public string EntityType { get; set; }   // Matches appsettings.json
    }
}
