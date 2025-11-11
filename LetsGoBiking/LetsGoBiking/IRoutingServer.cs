using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace RoutingService
{
    [ServiceContract]
    public interface IRoutingService
    {
        [OperationContract]
        [WebGet(UriTemplate = "/itinerary?origin={o}&destination={d}", 
                ResponseFormat = WebMessageFormat.Json,
                BodyStyle = WebMessageBodyStyle.Bare)]
        Stream GetItinerary(string o, string d);
    }
}
