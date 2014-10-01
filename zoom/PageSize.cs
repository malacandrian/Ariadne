using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zoom
{
    public class PageSize
    {
        public static readonly PageSize A4 = new PageSize("A4",595,842);

        public static IEnumerable<PageSize> Values
        {
            get
            {
                yield return A4;

            }
        }

        protected PageSize(string name,int width, int height)
        {
            this.name = name;
            this.width = width;
            this.height = height;
        }

        private readonly string name;
        private readonly int width;
        private readonly int height;

        public string Name { get { return name; } }
        public int Width { get { return width; } }
        public int Height { get { return height; } }


        public override string ToString()
        {
            return Name;
        }
    }
}
