using System;
using System.Threading;

namespace Helper
{
    public static class RetryHelper
    {
        private const string DEFAULT_TOTAL_ATTEMPTS = "3";

        private static readonly string TOTAL_ATTEMPTS = Environment.GetEnvironmentVariable("maxAttempts") ?? DEFAULT_TOTAL_ATTEMPTS;

        public static T Execute<T>(Func<T> method)
        {
            int currentRetry = 1;

            T value = default(T);

            for (; ; )
            {
                try
                {
                    value = method();
                    break;
                }
                catch (Exception)
                {
                    currentRetry++;
                    if (currentRetry > int.Parse(TOTAL_ATTEMPTS))
                    {
                        throw;
                    }
                }
                Thread.Sleep(50);
            }

            return value;

        }

        public static void Execute(Action method)
        {
            int currentRetry = 1;

            for (; ; )
            {
                try
                {
                    method();
                    break;
                }
                catch (Exception)
                {
                    currentRetry++;
                    if (currentRetry > int.Parse(TOTAL_ATTEMPTS))
                    {
                        throw;
                    }
                }
                Thread.Sleep(50);
            }
        }
    }
}
