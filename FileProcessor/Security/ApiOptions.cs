namespace FileProcessor.Security
{
    public sealed class ApiOptions
    {
        public const string SectionName = "ApiSecurity";
        public string HeaderName { get; set; } = "X-Api-Key";
        public string ApiKey { get; set; } = string.Empty;
    }
}
