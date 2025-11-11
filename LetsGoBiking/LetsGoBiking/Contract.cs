using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RoutingService
{
    [DataContract]
    public class Contract
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "commercial_name")]
        public string CommercialName { get; set; }

        [DataMember(Name = "cities")]
        public List<string> Cities { get; set; }

        [DataMember(Name = "country_code")]
        public string CountryCode { get; set; }
    }
}