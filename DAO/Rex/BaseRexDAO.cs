using Common.DTO.Rex;
using Helper;
using Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAO.Rex
{
    public class BaseRexDAO
    {
        private readonly int MAX_PROCESS;
        private readonly int MAX_PROCESS_FOR_COMPANIES;

        public BaseRexDAO()
        {
            bool success = int.TryParse(ConfigurationHelper.Value("maxParallel"), out int maxProcess);
            MAX_PROCESS = success ? maxProcess : 10;

            bool successForCompanies = int.TryParse(ConfigurationHelper.Value("maxParallelCompanies"), out int maxProcessforCompanies);
            MAX_PROCESS_FOR_COMPANIES = successForCompanies ? maxProcessforCompanies : 5;
        }

        /// <summary>
        /// Se realiza llamada GET por cada url (perteneciente a una razon social c/u) asociada a la empresa.
        /// Obtiene un listado de objetos T, si el servicio web indica una respuesta paginada, se llamará a cada página.
        /// Devuelve la lista de todos los objetos de todas las paginas para cada una de las razones sociales.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="urls"></param>
        /// <param name="endpoint"></param>
        /// <param name="token"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public List<T> GetAll<T>(List<string> urls, string endpoint, string token, VersionConfiguration versionConfiguration, string filter = "")
        {
            string type = typeof(T).ToString();
            ConcurrentBag<T> bag = new ConcurrentBag<T>();
            Parallel.ForEach(urls, new ParallelOptions { MaxDegreeOfParallelism = MAX_PROCESS_FOR_COMPANIES }, (url) =>
            {
                var items = this.GetAllByUrl<T>(url, endpoint, token, versionConfiguration, filter);
                foreach (var item in items)
                {
                    bag.Add(item);
                }
            });

            LogHelper.Log($"API Rex+ --- Get ALL {type}: {bag.Count} registros");
            return bag.ToList();
        }

        public List<T> GetAllByUrl<T>(string url, string endpoint, string token, VersionConfiguration versionConfiguration, string filter = "")
        {
            string type = typeof(T).ToString();
            ConcurrentBag<T> bag = new ConcurrentBag<T>();
            int currentPage = 1;
            try
            {
                var response = new RestConsumerRex(url, token, versionConfiguration).GetResponseAsync<ResponseRex<T>, Object>(endpoint + filter, null);
                var responseRex = response.Result;
                foreach (var item in responseRex.objetos)
                {
                    bag.Add(item);
                }

                if (responseRex.cantidad_paginas > 1)
                {
                    //Paginas desde 2 hasta la ultima pagina
                    var pages = Enumerable.Range(2, responseRex.cantidad_paginas - 1).ToList();

                    Parallel.ForEach(pages, new ParallelOptions { MaxDegreeOfParallelism = MAX_PROCESS }, (page) =>
                    {
                        var items = this.GetAllPaged<T>(url, endpoint, token, page, versionConfiguration);
                        foreach (var item in items)
                        {
                            bag.Add(item);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                LogHelper.Log("ERROR: " + e.Message);
                throw;
            }

            return bag.ToList();
        }

        public List<T> GetList<T>(string baseURL, string endpoint, string token, VersionConfiguration versionConfiguration, string filter)
        {
            var response = new RestConsumerRex(baseURL, token, versionConfiguration).GetResponseAsync<List<T>, Object>(endpoint + filter, null);
            return response.Result;
        }

        /// <summary>
        /// Obtiene un listado de objetos T de la página indicada desde el servicio web de Rex+.
        /// </summary>
        /// <typeparam name="T">El tipo de clase al cual pertenencerá la propiedad objetos de la respuesta entregada por el servicio web de Rex+</typeparam>
        /// <param name="baseURL">URL perteneciente a una Razon Social en Rex+</param>
        /// <param name="endpoint">Tiene un formato "/api/v2/xxxx"</param>
        /// <param name="token"></param>
        /// <param name="page">Pagina actual a obtener</param>
        /// <returns></returns>
        public List<T> GetAllPaged<T>(string baseURL, string endpoint, string token, int page, VersionConfiguration versionConfiguration)
        {
            List<T> list = new List<T>();

            string filter = $"?pagina={page}";
            var response = new RestConsumerRex(baseURL, token, versionConfiguration).GetResponseAsync<ResponseRex<T>, Object>(endpoint + filter, null);
            var responseRex = response.Result;
            list.AddRange(responseRex.objetos);

            return list;
        }

        /// <summary>
        /// Se realiza llamada POST para la url indicada entregando como body el parametro data serializado en json.
        /// Devuelve la response recibida.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseURL"></param>
        /// <param name="endpoint"></param>
        /// <param name="token"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ResponseRexMessage Post<T>(string baseURL, string endpoint, string token, T data, VersionConfiguration versionConfiguration)
        {
            var response = new RestConsumerRex(baseURL, token, versionConfiguration).PostResponse<ResponseRexMessage, T>(endpoint, data);

            return response;
        }

        /// <summary>
        /// Se realiza llamada PUT para la url indicada entregando como body el parametro data serializado en json.
        /// Devuelve la response recibida.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseURL"></param>
        /// <param name="endpoint"></param>
        /// <param name="token"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ResponseRexMessage Put<T>(string baseURL, string endpoint, string token, T data, VersionConfiguration versionConfiguration)
        {
            var response = new RestConsumerRex(baseURL, token, versionConfiguration).PutResponse<ResponseRexMessage, T>(endpoint, data);
            return response;
        }

        /// <summary>
        /// Se realiza llamada POST para la url indicada entregando como body el parametro data serializado en json.
        /// Devuelve la response recibida.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseURL"></param>
        /// <param name="endpoint"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public ResponseRexMessage Delete(string baseURL, string endpoint, string token, VersionConfiguration versionConfiguration)
        {
            var response = new RestConsumerRex(baseURL, token, versionConfiguration).DeleteResponse<ResponseRexMessage>(endpoint);

            return response;
        }
    }
}
