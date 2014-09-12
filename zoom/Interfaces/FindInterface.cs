using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;

namespace zoom.Interfaces
{
    class FindInterface : AbstractInterface
    {
        public Window Window { get; protected set; }

        public FindInterface(Window window)
        {
            Window = window;
        }

        public override void Release(object sender, UMD.HCIL.Piccolo.Event.PInputEventArgs e)
        {
            if (Entry.Text != "")
            {
                SearchDocuments(Window.Documents, Entry.Text);
            }
        }

        protected Selection SearchPage(Page page, string phrase)
        {
            int phraseStart = page.Text.Text.IndexOf(phrase);

            if (phraseStart > 0)
            {
                //Selection output = new Selection(page.Text, phraseStart, phraseStart + phrase.Length);
                //page.Text.AddChild(output);

                Selection output = page.Text.Select(phraseStart, phraseStart + phrase.Length);
                page.Text.ConfirmSelection(output);
                return output;
            }
            else
            {
                return null;
            }
        }

        protected Selection SearchDocument(Document document, string phrase)
        {
            Page[] pages = document.Pages;
            foreach (Page page in pages)
            {
                Selection found = SearchPage(page, phrase);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        protected Selection SearchDocuments(Document[] documents, string phrase)
        {
            foreach (Document document in documents)
            {
                Selection found = SearchDocument(document, phrase);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
