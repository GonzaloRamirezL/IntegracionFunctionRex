using Common.DTO.Rex;
using Common.Enum;
using Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Helper
{
    public class RestConsumerRex
    {

        private static readonly double TimeOut = double.Parse(ConfigurationHelper.Value("TimeOut"));

        private static ConcurrentDictionary<string, HttpClient> RexClients = null;

        private readonly HttpClient Client = null;

        static RestConsumerRex()
        {
            RexClients = new ConcurrentDictionary<string, HttpClient>();
        }

        private void NewClient(string rexURL, string rexToken, string BASE_URL)
        {
            var rexHttpClient = new HttpClient
            {
                BaseAddress = new Uri(String.Format(BASE_URL, rexURL)),
                Timeout = TimeSpan.FromMinutes(TimeOut)
            };

            rexHttpClient.DefaultRequestHeaders.Add("Authorization", rexToken);
            RexClients.TryAdd(rexURL, rexHttpClient);
        }

        public RestConsumerRex(string rexURL, string rexToken, VersionConfiguration versionConfiguration)
        {
            if (!RexClients.ContainsKey(rexURL)) {
                this.NewClient(rexURL, rexToken, versionConfiguration.BASE_URL);
            }

            Client = RexClients[rexURL];
        }

        public T PostResponse<T, U>(string url, U obj)
        {
            HttpResponseMessage response = null;

            RetryHelper.Execute(
                () =>
                {
                    response = AsyncHelper.RunSync<HttpResponseMessage>(() => Client.PostAsync(url, CreateHttpContent<U>(obj)));
                }
            );

            return CreateHttpContent<T>(response);
        }

        public T DeleteResponse<T>(string url)
        {
            HttpResponseMessage response = null;

            RetryHelper.Execute(
                () =>
                {
                    response = AsyncHelper.RunSync<HttpResponseMessage>(() => Client.DeleteAsync(url));
                }
            );

            return CreateHttpContent<T>(response);
        }

        public async Task<T> GetResponseAsync<T, U>(string url, U obj)
        {
            T ret = default(T);

            HttpResponseMessage response = null;

            int currentRetry = 1;

            for (; ; )
            {
                try
                {
                    response = await Client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    break;
                }
                catch (Exception)
                {
                    currentRetry++;
                    if (currentRetry > 3)
                    {
                        throw;
                    }
                }
                Thread.Sleep(50);
            }


            if (response.IsSuccessStatusCode)
            {
                string resp = await response.Content.ReadAsStringAsync();
                var provisionaltest = JsonConvert.DeserializeObject<T>(resp);
                return provisionaltest;
            }

            return ret;
        }

        public T GetResponseSnake<T, U>(string url, U obj)
        {
            T ret = default(T);

            HttpResponseMessage response = null;

            int currentRetry = 1;

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };

            for (; ; )
            {
                try
                {
                    response = AsyncHelper.RunSync<HttpResponseMessage>(() => Client.PostAsync(url, CreateHttpContent<U>(obj)));
                    response.EnsureSuccessStatusCode();
                    break;
                }
                catch (Exception)
                {
                    currentRetry++;
                    if (currentRetry > 3)
                    {
                        throw;
                    }
                }
                Thread.Sleep(50);
            }


            if (response.IsSuccessStatusCode)
            {
                string resp = AsyncHelper.RunSync<string>(() => response.Content.ReadAsStringAsync());
                return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result, new JsonSerializerSettings
                {
                    ContractResolver = contractResolver,
                    Formatting = Formatting.Indented
                });

            }

            return ret;
        }

        public T PutResponse<T, U>(string url, U obj)
        {
            HttpResponseMessage response = null;

            RetryHelper.Execute(
                () =>
                {
                    response = AsyncHelper.RunSync<HttpResponseMessage>(() => Client.PutAsync(url, CreateHttpContent<U>(obj)));
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        var resp = CreateHttpContent<BadRequestRex>(response);
                        string message = $"{resp.detalle}";
                        if (resp.mensajes != null)
                        {
                            message += $" - {string.Join(". ", resp.mensajes)}";
                        }
                        if (resp.informacion != null)
                        {
                            message += $" - {string.Join(". ", resp.informacion)}";
                        }
                        throw new Exception(message);
                    }
                    else
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
            );

            return CreateHttpContent<T>(response);
        }

        public T DeleteResponse<T, U>(string url, U obj)
        {
            HttpResponseMessage response = null;

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(Client.BaseAddress, url),
                Content = CreateHttpContent<U>(obj)
            };

            RetryHelper.Execute(
                () =>
                {
                    response = AsyncHelper.RunSync<HttpResponseMessage>(() => Client.SendAsync(request));
                    response.EnsureSuccessStatusCode();
                }
            );

            return CreateHttpContent<T>(response);
        }

        private T CreateHttpContent<T>(HttpResponseMessage response)
        {

            T ret = default(T);

            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                string resp = AsyncHelper.RunSync<string>(() => response.Content.ReadAsStringAsync());
                return JsonConvert.DeserializeObject<T>(resp);
            }

            return ret;
        }

        private HttpContent CreateHttpContent<T>(T content)
        {
            var json = JsonConvert.SerializeObject(content);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }
}
