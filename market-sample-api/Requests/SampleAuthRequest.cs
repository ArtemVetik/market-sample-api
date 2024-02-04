using Ydb.Sdk.Services.Table;

namespace Agava.MarketSampleApi
{
    internal class SampleAuthRequest : BaseRequest
    {
        public SampleAuthRequest(TableClient tableClient, Request request) : base(tableClient, request)
        { }

        protected override async Task<Response> Handle(TableClient client, Request request)
        {
            await Task.Yield();

            try
            {
                var userId = JwtTokenService.Validate(request.access_token, JwtTokenService.TokenType.Access);
                return new Response((uint)Ydb.Sdk.StatusCode.Success, Ydb.Sdk.StatusCode.Success.ToString(), userId, false);
            }
            catch (Exception exception)
            {
                return new Response((uint)StatusCode.ValidationError, StatusCode.ValidationError.ToString(), exception.Message, false);
            }

        }
    }
}
