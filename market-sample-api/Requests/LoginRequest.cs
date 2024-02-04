using Newtonsoft.Json;
using System.Text;
using Ydb.Sdk.Services.Table;
using Ydb.Sdk.Value;

namespace Agava.MarketSampleApi
{
    internal class LoginRequest : BaseRequest
    {
        public LoginRequest(TableClient tableClient, Request request)
            : base(tableClient, request)
        { }

        protected override async Task<Response> Handle(TableClient client, Request request)
        {
            var userCredentials = JsonConvert.DeserializeObject<UserCredentials>(request.body);

            var response = await client.SessionExec(async session =>
            {
                var query = $@"
                    DECLARE $email AS string;
                    DECLARE $password AS string;

                    SELECT user_id
                    FROM `users_credentials`
                    WHERE email = $email AND password = $password;
                ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit(),
                    parameters: new Dictionary<string, YdbValue>
                    {
                        { "$email", YdbValue.MakeString(Encoding.UTF8.GetBytes(userCredentials.email)) },
                        { "$password", YdbValue.MakeString(Encoding.UTF8.GetBytes(userCredentials.password)) },
                    }
                );
            });

            if (response.Status.IsSuccess == false)
                return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), string.Empty, false);

            var resultRows = ((ExecuteDataQueryResponse)response).Result.ResultSets[0].Rows;

            if (resultRows.Count == 0)
                return new Response((uint)StatusCode.ValidationError, StatusCode.ValidationError.ToString(), "Invalid credentials", false);

            var userId = resultRows[0]["user_id"].GetOptionalString();

            var accessToken = JwtTokenService.Create(TimeSpan.FromMinutes(5), Encoding.UTF8.GetString(userId), JwtTokenService.TokenType.Access);
            var refreshToken = JwtTokenService.Create(TimeSpan.FromMinutes(15), Encoding.UTF8.GetString(userId), JwtTokenService.TokenType.Refresh);

            response = await client.SessionExec(async session =>
            {
                var query = $@"
                    DECLARE $user_id AS string;
                    DECLARE $refresh AS string;

                    UPSERT INTO `refresh_tokens` (user_id, refresh_token)
                    VALUES ($user_id, $refresh);
                ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit(),
                    parameters: new Dictionary<string, YdbValue>
                    {
                        { "$user_id", YdbValue.MakeString(userId) },
                        { "$refresh", YdbValue.MakeString(Encoding.UTF8.GetBytes(refreshToken)) },
                    }
                );
            });

            if (response.Status.IsSuccess == false)
                return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), string.Empty, false);

            var responseBody = new
            {
                access = accessToken,
                refresh = refreshToken
            };

            var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseBody));
            return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), Convert.ToBase64String(jsonBytes), true);
        }
    }
}
