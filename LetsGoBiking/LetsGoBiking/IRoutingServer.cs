using System.ServiceModel;
using System.ServiceModel.Web;

namespace RoutingService
{
    [ServiceContract]
    public interface IRoutingService
    {
        [OperationContract]
        [WebGet(UriTemplate = "/itinerary?origin={o}&destination={d}", ResponseFormat = WebMessageFormat.Json)]
        string GetItinerary(string o, string d);
    }
}
