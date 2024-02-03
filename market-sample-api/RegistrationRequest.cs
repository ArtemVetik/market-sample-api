using Newtonsoft.Json;
using System.Text;
using Ydb.Sdk.Services.Table;
using Ydb.Sdk.Value;

namespace Agava.MarketSampleApi
{
    internal class RegistrationRequest : BaseRequest
    {
        public RegistrationRequest(TableClient tableClient, Request request) : base(tableClient, request)
        { }

        protected override async Task<Response> Handle(TableClient client, Request request)
        {
            var userCredentials = JsonConvert.DeserializeObject<UserCredentials>(request.body);

            var response = await client.SessionExec(async session =>
            {
                var query = $@"
                    DECLARE $email AS string;
                    DECLARE $password AS string;
                    DECLARE $user_id AS string;

                    INSERT INTO `users_credentials` (email, password, user_id)
                    VALUES ($email, $password, $user_id);
                ";

                return await session.ExecuteDataQuery(
                    query: query,
                    txControl: TxControl.BeginSerializableRW().Commit(),
                    parameters: new Dictionary<string, YdbValue>
                    {
                        { "$email", YdbValue.MakeString(Encoding.UTF8.GetBytes(userCredentials.email)) },
                        { "$password", YdbValue.MakeString(Encoding.UTF8.GetBytes(userCredentials.password)) },
                        { "$user_id", YdbValue.MakeString(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())) },
                    }
                );
            });

            return new Response((uint)response.Status.StatusCode, response.Status.StatusCode.ToString(), string.Empty);
        }
    }
}
