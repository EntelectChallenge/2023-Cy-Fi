using Microsoft.Extensions.Hosting;

namespace Domain.Models
{
    public class AppSettings
    {
        private string appEnvironment 
        { 
            get
            {
                return Environment.GetEnvironmentVariable("ENVIRONMENT") ?? Environments.Development;
            }
        }
        public string? ApiUrl 
        { 
            get
            {
                return IsCloud ? Environment.GetEnvironmentVariable("API_URL") : null;
            }
         }
        public string? ApiKey 
        {
            get
            {
                return IsCloud ? Environment.GetEnvironmentVariable("API_KEY") : null;
            }
        }
        public string? MatchId 
        { 
            get
            {
                return IsCloud? Environment.GetEnvironmentVariable("MATCH_ID") : null;
            }
        }
        public bool IsLocal => appEnvironment.Equals(Environments.Development, StringComparison.InvariantCultureIgnoreCase);
        public bool IsStaging => appEnvironment.Equals(Environments.Staging, StringComparison.InvariantCultureIgnoreCase);
        public bool IsProduction => appEnvironment.Equals(Environments.Production, StringComparison.InvariantCultureIgnoreCase);
        public bool IsCloud => !IsLocal;
    }
}
