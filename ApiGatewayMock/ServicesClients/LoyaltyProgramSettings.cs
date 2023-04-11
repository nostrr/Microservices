namespace ApiGatewayMock.ServicesClients
{
    public record LoyaltyProgramSettings()
    {
        public LoyaltyProgramSettings(string[] interests) : this()
        {
            Interests = interests;
        }

        public string[] Interests { get; init; } = Array.Empty<string>();
    }
}