namespace Backend.Configurations.DataConfigs
{
    public class VideoMetadataIndexingOptions
    {
        public string Exchange { get; set; } // Matches appsettings.json
        public string Queue { get; set; } // Matches appsettings.json
        public string RoutingKey { get; set; }   // Matches appsettings.json
        public string EntityType { get; set; }   // Matches appsettings.json
    }
}
