using Newtonsoft.Json;
using System.Text;
using Ydb.Sdk.Services.Table;
using Ydb.Sdk.Value;

namespace Agava.MarketSampleApi
{
    internal class RefreshRequest : BaseRequest
    {
        public RefreshRequest(TableClient tableClient, Request request) : base(tableClient, request)
        { }

        protected override async Task<Response> Handle(TableClient client, Request request)
        {
            string userId;
            try
            {
                userId = JwtTokenService.Validate(request.body, JwtTokenService.TokenType.Refresh);
            }
            catch (Exception exception)
            {
                return new Response((uint)StatusCode.ValidationError, StatusCode.ValidationError.ToString(), exception.Message, false);
            }

            var response = await client.SessionExec(async session =>
            {
                var query = $@"
                    DECLARE $user_id AS string;

                    SELECT refresh_token
                    FROM `refresh_tokens`
                    WHERE user_id = $user_id;
                ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit(),
                    parameters: new Dictionary<string, YdbValue>
                    {
                        { "$user_id", YdbValue.MakeString(Encoding.UTF8.GetBytes(userId)) },
                    }
                );
            });

            if (response.Status.IsSuccess == false)
                return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), string.Empty, false);

            var resultRows = ((ExecuteDataQueryResponse)response).Result.ResultSets[0].Rows;

            var dbToken = resultRows[0]["refresh_token"].GetOptionalString();

            if (Encoding.UTF8.GetString(dbToken) != request.body)
                return new Response((uint)StatusCode.ValidationError, StatusCode.ValidationError.ToString(), "Invalid token", false);

            var responseBody = new
            {
                access = JwtTokenService.Create(TimeSpan.FromMinutes(1), userId, JwtTokenService.TokenType.Access),
                refresh = request.body
            };

            var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseBody));
            return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), Convert.ToBase64String(jsonBytes), true);
        }
    }
}
