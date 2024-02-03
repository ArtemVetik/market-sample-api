using Yandex.Cloud.Credentials;
using Yandex.Cloud.Functions;
using Ydb.Sdk.Auth;
using Ydb.Sdk.Services.Table;
using Ydb.Sdk;

namespace Agava.MarketSampleApi
{
    internal partial class Handler : YcFunction<Request, Task<Response>>
    {
        public async Task<Response> FunctionHandler(Request request, Context context)
        {
            string ydbEndpoint = Environment.GetEnvironmentVariable("YdbEndpoint");
            string ydbDatabase = Environment.GetEnvironmentVariable("YdbDatabase");

            var token = new TokenProvider(new MetadataCredentialsProvider().GetToken());
            var config = new DriverConfig(ydbEndpoint, ydbDatabase, token);

            var driver = new Driver(config);
            await driver.Initialize();

            var tableClient = new TableClient(driver, new TableClientConfig());

            try
            {
                BaseRequest requestHandler = request.method switch
                {
                    "LOGIN" => new LoginRequest(tableClient, request),
                    _ => throw new InvalidOperationException($"Method {request.method} not found")
                };

                return await requestHandler.Handle();
            }
            finally
            {
                tableClient.Dispose();
                driver.Dispose();
            }
        }
    }
}
