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
    public class AzureToken
    {
        public string ClientID { get; protected set; }
        public string ClientSecret { get; protected set; }
        public string Scope { get; protected set; }

        public string EncodedId { get { return HttpUtility.UrlEncode(ClientID); } }
        public string EncodedSecret { get { return HttpUtility.UrlEncode(ClientSecret); } }
        public string EncodedScope { get { return HttpUtility.UrlEncode(Scope); } }

        protected AdmAccessToken token;
        protected static readonly string DataMarketURI = @"https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";

        public string AccessToken
        {
            get
            {
                if (Active == true) { return token.access_token; }
                else { return null; }
            }
        }

        public string TokenType
        {
            get
            {
                if (Active == true) { return token.access_token; }
                else { return null; }
            }
        }

        public bool Active
        {
            get
            {
                return token != null;
            }
            set
            {
                //Only do anything if the value is changing
                if (value != Active)
                {
                    if (value == true)
                    {
                        RequestToken();
                    }
                    else
                    {
                        token = null;
                    }
                }
            }
        }

        public DateTime ExpiresOn { get; protected set; }

        public AzureToken(string clientId, string clientSecret, string scope)
        {
            ClientID = clientId;
            ClientSecret = clientSecret;
            Scope = scope;
        }

        public bool RequestToken()
        {
            //If there is already a token, return true
            if (Active == true) { return true; }


            string request = String.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope={2}", EncodedId, EncodedSecret, EncodedScope);
            //If the RequestToken successfully executes and returns, return true, otherwise return false
            try
            {
                token = HttpPost(DataMarketURI, request);
                ExpiresOn = DateTime.Now.AddMinutes(Int32.Parse(token.expires_in));
                return true;
            }
            catch
            {
                token = null;
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
