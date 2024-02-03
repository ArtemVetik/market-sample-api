namespace Agava.MarketSampleApi
{
    public class Response
    {
        public Response(uint statusCode, string reasonPhrase, string body)
        {
            StatusCode = statusCode;
            Body = body;
            ReasonPhrase = reasonPhrase;
        }

        public uint StatusCode { get; private set; }
        public string ReasonPhrase { get; private set; }
        public string Body { get; private set; }
    }
}
