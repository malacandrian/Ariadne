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
    public class AzureServiceHelper
    {
        public readonly AzureToken Token;
        public readonly string ServiceURI;

        public AzureServiceHelper(string serviceUri, AzureToken token)
        {
            ServiceURI = serviceUri;
            Token = token;
        }

        public Stream Request(Dictionary<string,string> requestDetails)
        {
            Token.Active = true;

            string requestUrl = GenerateURL(SanitiseDictionary(requestDetails));
            string authToken = "Bearer" + " " + Token.AccessToken;

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUrl);
            httpWebRequest.Headers.Add("Authorization", authToken);

            try
            {
                WebResponse response = httpWebRequest.GetResponse();
                return response.GetResponseStream();
            }
            catch
            {
                return null;
            }
        }

        protected Dictionary<string, string> SanitiseDictionary(Dictionary<string, string> dictionary)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> entry in dictionary)
            {
                output.Add(HttpUtility.UrlEncode(entry.Key), HttpUtility.UrlEncode(entry.Value));
            }

            return output;
        }

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
