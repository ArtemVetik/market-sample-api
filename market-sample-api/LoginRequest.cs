using Ydb.Sdk.Services.Table;

namespace Agava.MarketSampleApi
{
    internal class LoginRequest : BaseRequest
    {
        public LoginRequest(TableClient tableClient, Request request)
            : base(tableClient, request)
        { }

        protected override async Task<Response> Handle(TableClient client, Request request)
        {
            await Task.Yield();
            return new Response(0, "Success", $"method: {request.method}, body: {request.body}, access token: {request.access_token}");
        }
    }
}
