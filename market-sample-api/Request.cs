namespace Agava.MarketSampleApi
{
    internal class Request
    {
        public string method { get; set; }
        public string body { get; set; }
        public string access_token { get; set; }

        public static Request Create(string method, string body, string accessToken)
        {
            return new Request()
            {
                method = method,
                body = body,
                access_token = accessToken,
            };
        }
    }
}
