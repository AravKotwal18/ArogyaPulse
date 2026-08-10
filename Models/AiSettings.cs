namespace ArogyaPulse.Api.Models
{
    public class AiSettings
    {
        public string Provider { get; set; } = "Gemini";

        public string ApiKey { get; set; } = string.Empty;

        public string Model { get; set; } = "gemini-1.5-flash";

        public string BaseUrl { get; set; } =
            "https://generativelanguage.googleapis.com/v1beta";
    }
}