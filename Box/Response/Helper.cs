namespace Box.Response
{
    public static class Helper
    {
        public static HttpResponseApi Response(string origin, string enpoint , string method , int statusCode , JsonElement data)
        {
            return new HttpResponseApi
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