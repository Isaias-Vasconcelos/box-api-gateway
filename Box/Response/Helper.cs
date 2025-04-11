using System.Text.Json;

namespace Box.Response
{
    public static class Helper
    {
        public static HttpResponseAPI Response(string origin, string enpoint , string method , int statusCode , JsonElement data)
        {
            return new HttpResponseAPI
            {
                Origin = origin,
                Endpoint = enpoint ,
                Method = method ,
                StatusCode = statusCode ,
                Data = data
            };
        }
    }
}