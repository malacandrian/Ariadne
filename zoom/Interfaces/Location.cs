using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zoom.Interfaces
{
    /// <summary>
    /// Location represents a single location within a single page of a document
    /// </summary>
    public class Location
    {
        /// <summary>
        /// The window the location exists within
        /// </summary>
        public readonly Window Window;
        
        /// <summary>
        /// The document the location exists within
        /// </summary>
        public readonly Document Document;

        /// <summary>
        /// The page the location exists within. Null is a wildcard value
        /// </summary>
        public readonly Page Page;

        /// <summary>
        /// The character index of the location. Null is a wildcard value
        /// </summary>
        public readonly int? CharIndex;

        /// <summary>
        /// The index of the page within the document.
        /// </summary>
        public int? PageIndex
        {
            get
            {
                //If a page is actually specified find the location within the document's pages
                if (Page != null)
                {
                    int output = Array.IndexOf(Document.Pages, Page);
                    if (output < 0) { throw new Exception(String.Format(@"Page {0} is not a member of Document {1}", Page.ToString(), Document.ToString())); }
                    return output;
                }
                else { return null; }
            }
        }

        /// <summary>
        /// The index of the document within the Window's list of documents
        /// </summary>
        public int DocumentIndex
        {
            get
            {
                int output = Array.IndexOf(Window.Documents, Document);
                if (output < 0) { throw new Exception(String.Format(@"Document {0} is not a member of Window {1}", Document.ToString(), Window.ToString())); }
                return output;
            }
        }

        /// <summary>
        /// Create a new Location
        /// </summary>
        /// <param Name="window">The Window the location exists within</param>
        /// <param Name="document">The document the location exists within</param>
        /// <param Name="page">The page the location exists within, Null is a wildcard value</param>
        /// <param Name="charIndex">The character index of the location. Null is a wildcard value</param>
        public Location(Window window, Document document, Page page, int? charIndex)
        {
            Window = window;
            Document = document;
            Page = page;
            CharIndex = charIndex;
        }
    }
}
