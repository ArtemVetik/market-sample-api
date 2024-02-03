using Agava.MarketSampleApi;
using Newtonsoft.Json;

var a = new UserCredentials()
{
    email = "artem.vetik58@gmail.com",
    password = "password123",
};
Console.WriteLine(JsonConvert.SerializeObject(a));
