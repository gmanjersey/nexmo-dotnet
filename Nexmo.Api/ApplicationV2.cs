﻿using Newtonsoft.Json;
using Nexmo.Api.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Nexmo.Api
{
    public class AppRequest
    {
        /// <summary>
        /// Optional. Used when updating an existing application.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
        /// <summary>
        /// The name of your application. Required.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// The private and public keys to be used with the Application. 
        /// </summary>
        [JsonProperty("keys")]
        public Keys Keys { get; set; }
        /// <summary>
        /// The Nexmo product or products that you access with this application. Possible values are:
        ///   voice, messages, RTC, VBC
        ///   This contains the configuration for each product and replaces type from V1 of the Application API.
        /// </summary>
        [JsonProperty("capabilities")]
        public Capabilities Capabilities { get; set; }
    }

    public class Keys
    {
        /// <summary>
        /// The public key used to validate the jwt in a https://en.wikipedia.org/wiki/JSON_Web_Token.
        /// </summary>
        [JsonProperty("private_key")]
        public string PrivateKey { get; set; }
        /// <summary>
        /// The private key you use to generate the JSON Web Token (JWT) that authenticates your requests to Voice API.
        /// </summary>
        [JsonProperty("public_key")]
        public string PublicKey { get; set; }
    }

    public class Capabilities
    {
        [JsonProperty("voice")]
        public VoiceWebhook Voice { get; set; }
        [JsonProperty("rtc")]
        public RtcWebhook Rtc { get; set; }
        [JsonProperty("messages")]
        public MessagesWebhook Messages { get; set; }
        [JsonProperty("vbc")]
        public VbcWebhook Vbc { get; set; }

    }

    public class VoiceWebhook
    {
        [JsonProperty("answer_url")]
        public WebHook AnswerUrl { get; set; }
        [JsonProperty("event_url")]
        public WebHook EventUrl { get; set; }
    }

    public class RtcWebhook
    {
        [JsonProperty("event_url")]
        public WebHook EventUrl { get; set; }
    }

    public class MessagesWebhook
    {
        [JsonProperty("inbound_url")]
        public WebHook InboundUrl { get; set; }
        [JsonProperty("status_url")]
        public WebHook StatusUrl { get; set; }
    }

    public class VbcWebhook
    {
        
    }

    public class Webhook
    {
        [JsonProperty("address")]
        public string Address { get; set; }
        [JsonProperty("http_method")]
        public string HttpMethod { get; set; }
    }

    public class AppResponse
    {
        /// <summary>
        /// The name of your application
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// The Nexmo product or products that you access with this application. Currently only voice is supported.
        /// </summary>
        [JsonProperty("capabilities")]
        public Capabilities Capabilities { get; set; }
        /// <summary>
        /// The ID allocated to your application by Nexmo.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("keys")]
        public Keys Keys { get; set; }
    }

    public class AppList
    {
        [JsonProperty("applications")]
        public List<AppResponse> Applications { get; set; }
    }

    public class AppListResponse : PaginatedResponse<AppList>
    { }

    public class ApplicationV2
    {
        /// <summary>
        /// Create a new application
        /// </summary>
        /// <param name="request">Application request</param>
        /// <param name="credentials">(Optional) Overridden credentials for only this request</param>
        /// <returns></returns>
        public static AppResponse Create(AppRequest request, Credentials credentials = null)
        {
            var response = VersionedApiRequest.DoRequest("POST",ApiRequest.GetBaseUriFor(typeof(ApplicationV2), "/v2/applications"), request, credentials);

            return JsonConvert.DeserializeObject<AppResponse>(response.JsonResponse);
        }

        /// <summary>
        /// Gets an application
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public static AppResponse Get(string appId, Credentials credentials = null)
        {
            // This is a dirty hack for the GET requests on Application V2 to work
            // until the fix on the API level is implemented.
#if (!NETSTANDARD1_6)
            {
                string url = ApiRequest.GetBaseUriFor(typeof(ApplicationV2), $"/v2/applications/{appId}").ToString();
                string result;
                var authBytes = Encoding.UTF8.GetBytes(credentials.ApiKey + ":" + credentials.ApiSecret);
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.Authorization] = "Basic" + Convert.ToBase64String(authBytes);
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";

                    result = client.DownloadString(url);
                }
                return JsonConvert.DeserializeObject<AppResponse>(result);
            }

#endif

            return null;

            // proper solution
            //var response = ApiRequest.DoRequest(ApiRequest.GetBaseUriFor(typeof(ApplicationV2), $"/v2/applications/{appId}"),  credentials);

            //return JsonConvert.DeserializeObject<AppResponse>(response);
        }
       
        /// <summary>
        /// List all of the applications associated with this account
        /// </summary>
        /// <param name="pageSize">Set the number of items returned on each call to this endpoint. The default is 10 records.</param>
        /// <param name="page">Set the offset from the first page. The default value is 0, calls to this endpoint return a page of <page_size>. For example, set page_index to 3 to retrieve items 31 - 40 when page_size is the default value.</param>
        /// <param name="AppId">Optional id of specific application to retrieve</param>
        /// <param name="credentials">(Optional) Overridden credentials for only this request</param>
        /// <returns></returns>
        public static List<AppResponse> List(int pageSize = 10, int page = 0,  Credentials credentials = null)
        {
            // This is a dirty hack for the GET requests on Application V2 to work
            // until the fix on the API level is implemented.
#if (!NETSTANDARD1_6)
            {
                string url = ApiRequest.GetBaseUriFor(typeof(ApplicationV2), "/v2/applications").ToString();
                string result;
                var authBytes = Encoding.UTF8.GetBytes(credentials.ApiKey + ":" + credentials.ApiSecret);
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.Authorization] = "Basic" + Convert.ToBase64String(authBytes);
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.QueryString.Add("page_size", pageSize.ToString());
                    client.QueryString.Add("page", page.ToString());

                    result = client.DownloadString(url);
                }

                var response = JsonConvert.DeserializeObject<AppListResponse>(result);
                return response._embedded.Applications;
            }

#endif

            return null;

            // Proper solution

            //var searchFilter = new Dictionary<string, string>
            //{
            //    { "page_size", pageSize.ToString()},
            //    { "page", page.ToString()}
            //};

            //var json = VersionedApiRequest.DoRequest("GET", ApiRequest.GetBaseUriFor(typeof(ApplicationV2), "/v2/applications"), searchFilter, credentials);

            //var response = JsonConvert.DeserializeObject<AppListResponse>(json.JsonResponse);
            //return response._embedded.Applications;
        }

        public static List<AppResponse> List()
        {
            return List(10, 0);
        }

        /// <summary>
        /// Modify a single application
        /// </summary>
        /// <param name="appId">Id of the application to be updated</param>
        /// <param name="request">Application request</param>
        /// <param name="credentials">(Optional) Overridden credentials for only this request</param>
        /// <returns></returns>
        public static AppResponse Update(AppRequest request, Credentials credentials = null)
        {
            var response = VersionedApiRequest.DoRequest("PUT",ApiRequest.GetBaseUriFor(typeof(ApplicationV2),
                $"/v2/applications/{request.Id}"), request, credentials);

            return JsonConvert.DeserializeObject<AppResponse>(response.JsonResponse);
        }

        /// <summary>
        /// Delete a single application
        /// </summary>
        /// <param name="appId">The application id to delete</param>
        /// <param name="credentials">(Optional) Overridden credentials for only this request</param>
        /// <returns></returns>
        public static bool Delete(string appId, Credentials credentials = null)
        {
            var response = VersionedApiRequest.DoRequest("DELETE",ApiRequest.GetBaseUriFor(typeof(ApplicationV2),
                $"/v2/applications/{appId}"), null, credentials);

            return response.Status == HttpStatusCode.NoContent;
        }
    }
}
