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

namespace zoom.Commands
{
    class TranslateCommand : ICommand
    {
        public string Name { get { return "translate"; } }
        public string DefaultToLanguage;
        protected PCamera Camera;
        protected AzureToken TranslateToken;
        protected AzureServiceHelper TranslateService;
        protected AzureServiceHelper LanguageDetectService;

        public TranslateCommand(string defaultToLanguage, PCamera camera)
        {
            DefaultToLanguage = defaultToLanguage;
            Camera = camera;

            TranslateToken = new AzureToken(Resources.ClientSecrets.ClientID, Resources.ClientSecrets.ClientSecret, @"http://api.microsofttranslator.com");
            TranslateService = new AzureServiceHelper(@"http://api.microsofttranslator.com/v2/Http.svc/Translate", TranslateToken);
            LanguageDetectService = new AzureServiceHelper(@"http://api.microsofttranslator.com/v2/Http.svc/Detect", TranslateToken);
        }

        public void Execute(Selection selection, string[] arguments)
        {
            string to = DetectTo(arguments);
            string from = DetectFrom(selection.Text, arguments);

            if (to != null && from != null)
            {
                selection.Text = Translate(to, from, selection.Text);
            }
        }

        protected string DetectTo(string[] arguments)
        {
            string detected = DetectCommon("to", arguments);

            if (detected != null) { return detected; }
            else { return DefaultToLanguage; }
        }

        protected string DetectFrom(string text, string[] arguments)
        {
            string detected = DetectCommon("from", arguments);
            if (detected != null) { return detected; }
            else
            {
                Dictionary<string, string> args = new Dictionary<string, string>() { { "text", text } };
                using (Stream stream = LanguageDetectService.Request(args))
                {
                    System.Runtime.Serialization.DataContractSerializer dcs = new System.Runtime.Serialization.DataContractSerializer(Type.GetType("System.String"));
                    string language = (string)dcs.ReadObject(stream);
                    return language;
                }
            }
        }

        protected string DetectCommon(string signal, string[] arguments)
        {
            for (int i = 0; i < arguments.Length; i += 1)
            {
                if (signal == arguments[i] && i < arguments.Length - 1)
                {
                    return arguments[i + 1];
                }
            }

            return null;
        }

        protected string Translate(string to, string from, string text)
        {
            Dictionary<string, string> args = new Dictionary<string, string>()
            {
                { "to", to },
                { "from", from },
                { "text", text }
            };

            using(Stream stream = TranslateService.Request(args))
            {
                System.Runtime.Serialization.DataContractSerializer dcs = new System.Runtime.Serialization.DataContractSerializer(Type.GetType("System.String"));
                string translation = (string)dcs.ReadObject(stream);
                return translation;
            }
        }


        public UMD.HCIL.Piccolo.Nodes.PText Preview(Selection selection, string[] arguments)
        {
            return new PText();
        }
    }
}
