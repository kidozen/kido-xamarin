using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidoZen
{
    public class Marketplace
    {
        public Uri Url { get; private set; }
        KZApplication app;

        internal Marketplace(KZApplication app, Uri endpoint)
        {
            this.Url = endpoint;
            this.app = app;
        }

        /// <summary>
        /// Gets the application's information.
        /// </summary>
        /// <param name="name">Required. Application names</param>
        /// <returns>A JSON instance</returns>
        public async Task<ServiceEvent<AppInfo>> GetApplication(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");

            var result = await QueryApplications(name);
            if (result.Data == null || result.Data.Length == 0)
                return result.Clone<AppInfo>(null);

            return result.Clone<AppInfo>(result.Data.First());
        }

        /// <summary>
        /// Gets all applications from the marketplace
        /// </summary>
        /// <returns>A JSON array instance</returns>
        public async Task<ServiceEvent<AppInfo[]>> GetApplications()
        {
            return await QueryApplications();
        }

        /// <summary>
        ///  Gets all application's names from the marketplace
        /// </summary>
        /// <returns>A string array with all application's names</returns>
        public async Task<ServiceEvent<string[]>> GetApplicationNames()
        {
            string[] names = null;
            var apps = await QueryApplications();
            if (apps.Data != null)
            {
                names = apps.Data.Select(a => a.Name).ToArray();
            }
            return apps.Clone<string[]>(names);
        }

        /// <summary>
        /// Gets all applications installed by the authenticated user.
        /// </summary>
        /// <returns>A JSON array where each element is the information ralated to a single application</returns>
        public async Task<ServiceEvent<AppInfo[]>> GetUserApplications()
        {
            if (!app.Initialized) throw new Exception("The application was not initialized.");
            if (app.User == null || app.User.TokenMarketplace == null) throw new Exception("User is not authenticated.");

            var apps = await Url.Concat("api/myapps").ExecuteAsync<JArray>(app, useToken:authentication.UseToken.Marketplace);
            var array = apps.Data == null ? 
                new AppInfo[0] : 
                apps
                    .Data
                    .Select(a => new AppInfo((JObject)a))
                    .ToArray();

            return apps.Clone<AppInfo[]>(array);
        }

        /// <summary>
        /// Gets all application's names that were installed by the authenticated user.
        /// </summary>
        /// <returns>A string array with all names</returns>
        public async Task<ServiceEvent<string[]>> GetUserApplicationNames()
        {
            string[] names = null;
            var apps = await GetUserApplications();
            if (apps.Data != null)
            {
                names = apps.Data.Select(a => a.Name).ToArray();
            }
            return apps.Clone<string[]>(names);
        }

        #region Private members

        private async Task<ServiceEvent<AppInfo[]>> QueryApplications(string name = null)
        {
            if (!app.Initialized) throw new Exception("The application was not initialized.");

            var resource = "publicapi/apps" + (string.IsNullOrWhiteSpace(name) ? "" : "?name=" + name);
            var apps = await Url.Concat(resource).ExecuteAsync<JArray>(app, useToken: authentication.UseToken.None);

            return apps.Clone<AppInfo[]>(apps
                .Data
                .Select(a => new AppInfo((JObject)a))
                .ToArray());
        }

        #endregion
    }
}
