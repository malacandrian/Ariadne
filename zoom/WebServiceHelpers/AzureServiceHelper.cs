using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace zoom.WebServiceHelpers
{
    /// <summary>
    /// AzureServiceHelper provides a set of hooks for interacting with a web service on the Azure data market
    /// </summary>
    public class AzureServiceHelper
    {
        /// <summary>
        /// The _AccessToken authorising the use of the service
        /// </summary>
        public readonly AzureToken Token;

        /// <summary>
        /// The URI of the service being called
        /// </summary>
        public readonly string ServiceURI;

        /// <summary>
        /// Create a new AzureServiceHelper
        /// </summary>
        /// <param Name="serviceUri">The URI of the service being called</param>
        /// <param Name="_AccessToken">The _AccessToken authorising the use of the service</param>
        public AzureServiceHelper(string serviceUri, AzureToken token)
        {
            ServiceURI = serviceUri;
            Token = token;
        }

        /// <summary>
        /// Send a request to the web service with specific arguments
        /// </summary>
        /// <param Name="requestDetails">The arguments to pass with the request</param>
        /// <returns>A stream of the response to the request</returns>
        public Stream Request(Dictionary<string, string> requestDetails)
        {
            //Activate the _AccessToken to authorise the request
            Token.Active = true;

            //Prepare the request
            string requestUrl = GenerateURL(SanitiseDictionary(requestDetails));
            string authToken = "Bearer" + " " + Token.AccessToken;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUrl);
            httpWebRequest.Headers.Add("Authorization", authToken);

            //Attempt to perform the request, returning null if it failed
            try
            {
                WebResponse response = httpWebRequest.GetResponse();
                return response.GetResponseStream();
            }
            catch { return null; }
        }

        /// <summary>
        /// Encode any special characters in the arguments dictionary so they can be safely transmitted over HTTP
        /// </summary>
        /// <param Name="dictionary">The dictionary to sanitise</param>
        /// <returns>A sanitised version of the dictionary</returns>
        protected Dictionary<string, string> SanitiseDictionary(Dictionary<string, string> dictionary)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> entry in dictionary)
            {
                //For each item of the dictionary, encode both the key and the value
                output.Add(HttpUtility.UrlEncode(entry.Key), HttpUtility.UrlEncode(entry.Value));
            }

            return output;
        }

        /// <summary>
        /// Format a URL to create a request to the web service
        /// </summary>
        /// <param Name="requestDetails">The arguments to the request</param>
        /// <returns>The URL to make the request on</returns>
        protected string GenerateURL(Dictionary<string, string> requestDetails)
        {
            string output = ServiceURI;

            //Make sure the URI is set up to accept arguments
            if (output.Last() != '?') { output += '?'; }

            //Add the arguements
            foreach (KeyValuePair<string, string> argument in requestDetails)
            {
                output += String.Format(@"{0}={1}&", argument.Key, argument.Value);
            }

            //Remove the final ampersand
            output = output.Substring(0, output.Length - 1);

            return output;
        }
    }
}
