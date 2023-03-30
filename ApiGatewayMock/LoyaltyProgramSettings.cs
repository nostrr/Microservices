namespace ApiGatewayMock
{
    public record LoyaltyProgramSettings()
    {
        public LoyaltyProgramSettings(string[] interests) : this()
        {
            this.Interests = interests;
        }

        public string[] Interests { get; init; } = Array.Empty<string>();
    }
}