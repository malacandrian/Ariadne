using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TranslatorForAzure;
using System.Web;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;

namespace zoom.WebServiceHelpers
{
    /// <summary>
    /// AzureToken handles everything to do with authorising requests on the Azure data market
    /// </summary>
    public class AzureToken
    {
        /// <summary>
        /// The ID of the program making the request
        /// </summary>
        public string ClientID { get; protected set; }

        /// <summary>
        /// The secret of the program making the request
        /// </summary>
        public string ClientSecret { get; protected set; }

        /// <summary>
        /// The scope of what the token is authorising for
        /// </summary>
        public string Scope { get; protected set; }

        /// <summary>
        /// The ID, encoded for sending over HTTP
        /// </summary>
        public string EncodedId { get { return HttpUtility.UrlEncode(ClientID); } }

        /// <summary>
        /// The secret, encoded for sending over HTTP
        /// </summary>
        public string EncodedSecret { get { return HttpUtility.UrlEncode(ClientSecret); } }

        /// <summary>
        /// The scope, encoded for sending over HTTP
        /// </summary>
        public string EncodedScope { get { return HttpUtility.UrlEncode(Scope); } }

        /// <summary>
        /// The Token that an authourisation request returns. This class is built to abstract on this token
        /// </summary>
        protected AdmAccessToken _AccessToken;

        /// <summary>
        /// The URI to request authorisation from
        /// </summary>
        protected static readonly string _DataMarketURI = @"https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";

        /// <summary>
        /// Gets the access token if it is currently active
        /// </summary>
        public string AccessToken
        {
            get
            {
                if (Active == true) { return _AccessToken.access_token; }
                else { return null; }
            }
        }

        /// <summary>
        /// Gets the type of token if it is currently active
        /// </summary>
        public string TokenType
        {
            get
            {
                if (Active == true) { return _AccessToken.access_token; }
                else { return null; }
            }
        }

        /// <summary>
        /// Determines whether the curren token is active
        /// </summary>
        public bool Active
        {
            get { return _AccessToken != null && ExpiresOn > DateTime.Now; }
            set
            {
                //Only do anything if the value is changing
                if (value != Active)
                {
                    if (value == true) { RequestToken(); }
                    else { _AccessToken = null; }
                }
            }
        }

        /// <summary>
        /// When the current token expires
        /// </summary>
        public DateTime ExpiresOn { get; protected set; }

        /// <summary>
        /// Create a new AzureToken
        /// </summary>
        /// <param Name="clientId">The ID of the program accessing the web service</param>
        /// <param Name="clientSecret">The secret of the program accessing the web service</param>
        /// <param Name="scope">The scope of what the token is for</param>
        public AzureToken(string clientId, string clientSecret, string scope)
        {
            ClientID = clientId;
            ClientSecret = clientSecret;
            Scope = scope;
        }

        /// <summary>
        /// Request a new token from the azure data market
        /// </summary>
        /// <returns>Whether it succesfully aquired a token</returns>
        public bool RequestToken()
        {
            //If there is already a _AccessToken, return true
            if (Active == true) { return true; }


            string request = String.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope={2}", EncodedId, EncodedSecret, EncodedScope);
            //If the RequestToken successfully executes and returns, return true, otherwise return false
            try
            {
                _AccessToken = HttpPost(_DataMarketURI, request);
                ExpiresOn = DateTime.Now.AddMinutes(Int32.Parse(_AccessToken.expires_in));
                return true;
            }
            catch
            {
                _AccessToken = null;
                return false;
            }

        }

        //This method is sourced from http://msdn.microsoft.com/en-us/library/hh454950.aspx#csharpexample
        private AdmAccessToken HttpPost(string DatamarketAccessUri, string requestDetails)
        {
            //Prepare OAuth RequestToken 
            WebRequest webRequest = WebRequest.Create(DatamarketAccessUri);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            byte[] bytes = Encoding.ASCII.GetBytes(requestDetails);
            webRequest.ContentLength = bytes.Length;
            using (Stream outputStream = webRequest.GetRequestStream())
            {
                outputStream.Write(bytes, 0, bytes.Length);
            }
            using (WebResponse webResponse = webRequest.GetResponse())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AdmAccessToken));
                //Get deserialized object from JSON stream
                AdmAccessToken token = (AdmAccessToken)serializer.ReadObject(webResponse.GetResponseStream());
                return token;
            }
        }
    }
}
