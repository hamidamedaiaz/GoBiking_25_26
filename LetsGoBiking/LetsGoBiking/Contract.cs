using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RoutingService
{
    [DataContract]
    public class Contract
    {
        public string name { get; set; }
        public string commercial_name { get; set; }
    }
}