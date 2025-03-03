using RestSharp;
using RestSharp.Authenticators;
using System.Configuration;
using System.Net;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using Helpers;
using Common.ViewModels;

namespace Helper
{
    public class ConnectionHelper
    {
        /// <summary>
        /// Stablish connection to GeoVictoria API
        /// </summary>
        /// <returns></returns>
        public static IRestClient ConnectGeoVictoria(GeoVictoriaConnectionVM gvConnection)
        {
            string urlService = string.Empty;
            if (gvConnection.TestEnvironment)
            {
                urlService = ConfigurationHelper.Value("UrlSandboxService");
            } 
            else
            {
                urlService = ConfigurationHelper.Value("UrlService");
            }

            IRestClient client = new RestClient(urlService);
            client.Authenticator = OAuth1Authenticator.ForProtectedResource(
                gvConnection.ApiKey,
                gvConnection.ApiSecret, string.Empty, string.Empty);

            return client;
        }

        /// <summary>
        /// Creates a RestClient object an executes the given request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="url"></param>
        /// <param name="api"></param>
        /// <param name="endpoint"></param>
        /// <param name="withAuthenticator"></param>
        /// <param name="errorMessage"></param>
        /// <param name="sucessMessage"></param>
        /// <returns></returns>
        public static IRestResponse GetResponse(IRestRequest request, string url, string api, string endpoint, string key, string secret, bool withAuthenticator = false,
            string errorMessage = "", string sucessMessage = "")
        {
            IRestClient client = new RestClient(url);
            client.Timeout = 1000 * 5000;
            if (withAuthenticator)
            {
                client.Authenticator = OAuth1Authenticator.ForProtectedResource(key, secret, string.Empty, string.Empty);
            }
            IRestResponse response = null;
            int currentAttempt = 1;
            for (; ; )
            {
                try
                {
                    response = client.Execute(request);
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        //throw new APIResponseException(api, endpoint, response.StatusCode.ToString(), response.Content);
                    }
                    break;
                }
                catch (Exception ex)
                {
                    currentAttempt++;
                    if (currentAttempt > 3)
                    {
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            LogHelper.Log(errorMessage);
                        }
                        throw ex;
                    }
                }
            }

            if (!string.IsNullOrEmpty(sucessMessage))
            {
                LogHelper.Log(sucessMessage);
            }

            return response;
        }
        public static T CreateHttpContent<T>(HttpResponseMessage response)
        {

            T ret = default(T);

            if (response.IsSuccessStatusCode)
            {
                string resp = RunSync<string>(() => response.Content.ReadAsStringAsync());
                
                return JsonConvert.DeserializeObject<T>(resp);
            }

            return ret;
        }
        public static T RunSync<T>(Func<Task<T>> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            T ret = default(T);
            synch.Post(async _ =>
            {
                try
                {
                    ret = await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            return ret;
        }
    }
    public class ExclusiveSynchronizationContext : SynchronizationContext
    {
        private bool done;
        public Exception InnerException { get; set; }
        readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
        readonly Queue<Tuple<SendOrPostCallback, object>> items =
        new Queue<Tuple<SendOrPostCallback, object>>();

        public override void Send(SendOrPostCallback d, object state)
        {
            throw new NotSupportedException("We cannot send to our same thread");
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            lock (items)
            {
                items.Enqueue(Tuple.Create(d, state));
            }
            workItemsWaiting.Set();
        }

        public void EndMessageLoop()
        {
            Post(_ => done = true, null);
        }

        public void BeginMessageLoop()
        {
            while (!done)
            {
                Tuple<SendOrPostCallback, object> task = null;
                lock (items)
                {
                    if (items.Count > 0)
                    {
                        task = items.Dequeue();
                    }
                }
                if (task != null)
                {
                    task.Item1(task.Item2);
                    if (InnerException != null) // the method threw an exeption
                    {
                        throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
                    }
                }
                else
                {
                    workItemsWaiting.WaitOne();
                }
            }
        }

        public override SynchronizationContext CreateCopy()
        {
            return this;
        }
    }
}
