namespace ENOC.Domain.Models
{
    public class AdConfig(string DomainIP, string DomainName)
    {
        public string DomainIP { get; set; } = DomainIP;
        public string DomainName { get; set; } = DomainName;
    }

}