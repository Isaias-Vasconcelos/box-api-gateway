using Polly;
using Polly.Retry;

namespace Box.Http.Implementations
{
    public class HttpService(HttpClient httpClient, Gateway gateway) : IHttpService
    {
        private readonly ResiliencePipeline<HttpResponseMessage> _pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<Exception>()
                    .HandleResult(static result => !result.IsSuccessStatusCode),
                Delay = gateway.Config?.HttpRetry?.DelayTypeTime switch
                {
                    "SECONDS" => TimeSpan.FromSeconds(gateway.Config.HttpRetry.DelayTime),
                    "MINUTES" => TimeSpan.FromMinutes(gateway.Config.HttpRetry.DelayTime),
                    "HOURS" => TimeSpan.FromHours(gateway.Config.HttpRetry.DelayTime),
                    "DAYS" => TimeSpan.FromDays(gateway.Config.HttpRetry.DelayTime),
                    _ => TimeSpan.FromSeconds(gateway.Config!.HttpRetry!.DelayTime),
                },
                MaxRetryAttempts = gateway.Config!.HttpRetry.MaxRetryAttempts,
                BackoffType = DelayBackoffType.Constant,
                OnRetry = args =>
                {
                    Console.WriteLine($"Attempt: {args.AttemptNumber + 1} - Delay: {args.RetryDelay} - Duration: {args.Duration}");   
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
        
        public Task<HttpResponseMessage> DeleteAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<HttpResponseMessage> GetAsync(string origin, string endpoint)
        {
            httpClient.BaseAddress = new Uri(origin);
            var response = await _pipeline.ExecuteAsync(async token => await httpClient.GetAsync(endpoint, token));
            return response;
        }

        public async Task<HttpResponseMessage> PostAsync(string origin, string endpoint, Stream body)
        {
            httpClient.BaseAddress = new Uri(origin);
            using var reader = new StreamReader(body);
            var bodyString = await reader.ReadToEndAsync();
            var content = new StringContent(bodyString, Encoding.UTF8, "application/json");
            var response = await _pipeline.ExecuteAsync(async token => await httpClient.PostAsync(endpoint, content, token));
            return response;
        }

        public Task<HttpResponseMessage> PutAsync()
        {
            throw new NotImplementedException();
        }
    }
}