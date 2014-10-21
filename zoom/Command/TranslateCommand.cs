using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Microsoft;
using System.Data.Services.Client;
using UMD.HCIL.Piccolo;
using System.Web;
using zoom.WebServiceHelpers;
using System.IO;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;
using UMD.HCIL.Piccolo.Nodes;
using System.Runtime.Serialization;

namespace zoom.Command
{
    /// <summary>
    /// Translate the current text of the selection to another language
    /// </summary>
    class TranslateCommand : ICommand
    {
        /// <summary>
        /// The Name of the command - the user needs to type this to execute the command
        /// </summary>
        public string Name { get { return "translate"; } }

        /// <summary>
        /// The PCamera object to attach errors to
        /// </summary>
        public PCamera Camera { get; protected set; }

        /// <summary>
        /// The default language the text should be translated to, if none is specified
        /// </summary>
        public string DefaultToLanguage;

        /// <summary>
        /// The authorisation _AccessToken for the Azure Bing translator service
        /// </summary>
        protected AzureToken TranslateToken;

        /// <summary>
        /// A hook for the Azure Bing translator service
        /// </summary>
        protected AzureServiceHelper TranslateService;

        /// <summary>
        /// A hook for the Azure Bing language detection service
        /// </summary>
        protected AzureServiceHelper LanguageDetectService;

        /// <summary>
        /// Create a new Translate command with a given default to language
        /// </summary>
        /// <param Name="defaultToLanguage">The default language the command should be translated to</param>
        public TranslateCommand(string defaultToLanguage, PCamera camera)
        {
            DefaultToLanguage = defaultToLanguage;
            Camera = camera;

            //Request a _AccessToken from the Azure Bing Translate service
            TranslateToken = new AzureToken(Resources.ClientSecrets.ClientID, Resources.ClientSecrets.ClientSecret, @"http://api.microsofttranslator.com");

            //Create hooks for the translate and language detection services using the translate _AccessToken for authorisation
            TranslateService = new AzureServiceHelper(@"http://api.microsofttranslator.com/v2/Http.svc/Translate", TranslateToken);
            LanguageDetectService = new AzureServiceHelper(@"http://api.microsofttranslator.com/v2/Http.svc/Detect", TranslateToken);
        }

        /// <summary>
        /// Replace the text of the selection with the translated text
        /// </summary>
        /// <param Name="selection">The current selection - the current text is sent to the translator and replaced with the translate text</param>
        /// <param Name="argument">
        /// The to and from languages are specified here - 
        /// it searches for the words "to" and "from" in the argument array
        /// and takes the value of the next item in the array for that language
        /// </param>
        public void Execute(Selection selection, string[] arguments)
        {
            //Detect which languages the translation is happening between
            string to = DetectTo(arguments);
            string from = DetectFrom(selection.Text, arguments);

            //If it cannot detect a from language, throw an error
            if (from == null) { new Error("No source language specified or detected", Camera); }

            //Otherwise, translate the text
            else { selection.Text = Translate(to, from, selection.Text); }

        }

        /// <summary>
        /// Detect what langauge the text should be transated to
        /// </summary>
        /// <param Name="argument">The argument provided to the command</param>
        /// <returns>The language the text should be translated to</returns>
        protected string DetectTo(string[] arguments)
        {
            //If the user specified a language in the argument, get that
            string detected = DetectCommon("to", arguments);
            if (detected != null) { return detected; }

            //Otherwise, translate to the default language
            else { return DefaultToLanguage; }
        }

        /// <summary>
        /// Detect what language the text should be translated from
        /// </summary>
        /// <param Name="text">The text that needs to be translated</param>
        /// <param Name="argument">The argument provided to the command</param>
        /// <returns>The language the text should be translated from</returns>
        protected string DetectFrom(string text, string[] arguments)
        {
            //If the user specified a language in the argument, get that
            string detected = DetectCommon("from", arguments);
            if (detected != null) { return detected; }

            //Otherwise, detect the language using the Azure Bing Detection service
            else
            {
                //Send a request to the service
                Dictionary<string, string> args = new Dictionary<string, string>() { { "text", text } };
                using (Stream stream = LanguageDetectService.Request(args))
                {
                    //Read and return the service'sections response
                    DataContractSerializer dcs = new DataContractSerializer(Type.GetType("System.String"));
                    return (string)dcs.ReadObject(stream); ;
                }
            }
        }

        /// <summary>
        /// The common logic for detecting languages in the argument
        /// </summary>
        /// <param Name="signal">The phrase to look out for in the argument, eg "to", "from"</param>
        /// <param Name="argument">The argument sent to the command</param>
        /// <returns>The language the user specified, or null if it isn't detected</returns>
        protected string DetectCommon(string signal, string[] arguments)
        {
            //Check all of the argument
            for (int i = 0; i < arguments.Length; i += 1)
            {
                //If one of them matches the signal, and there'sections an argument after it, return that
                if (signal == arguments[i] && i < arguments.Length - 1) { return arguments[i + 1]; }
            }

            //If nothing was detected return null
            return null;
        }

        /// <summary>
        /// Translate text between two languages using the Azure Bing Translate API
        /// </summary>
        /// <param Name="to">The language to translate to</param>
        /// <param Name="from">The language to translate from</param>
        /// <param Name="text">The text to translate</param>
        /// <returns>The translated text</returns>
        protected string Translate(string to, string from, string text)
        {
            //Pack the argument into a dictionary for use in the web service call
            Dictionary<string, string> args = new Dictionary<string, string>()
            {
                { "to", to },
                { "from", from },
                { "text", text }
            };

            //Send a request to the Translate service
            using (Stream stream = TranslateService.Request(args))
            {
                //Read the result from the Translate service
                DataContractSerializer dcs = new DataContractSerializer(Type.GetType("System.String"));
                return (string)dcs.ReadObject(stream);
            }
        }

        /// <summary>
        /// Due to restriction in how frequently the web service can be called, preview is not enabled for the translate command
        /// </summary>
        /// <param Name="selection"></param>
        /// <param Name="argument"></param>
        /// <returns></returns>
        public UMD.HCIL.Piccolo.Nodes.PText Preview(Selection selection, string[] arguments)
        {
            return new PText();
        }
    }
}
