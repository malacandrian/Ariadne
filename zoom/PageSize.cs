using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zoom
{
    /// <summary>
    /// PageSize represents the possible sizes a piece of paper can be.
    /// The class is designed to mimic the functionality of a Java 5 enum
    /// </summary>
    public class PageSize
    {
        /// <summary>
        /// Size A0
        /// </summary>
        public static readonly PageSize A0 = new PageSize("A0", 2380, 3368);

        /// <summary>
        /// Size A1
        /// </summary>
        public static readonly PageSize A1 = new PageSize("A1", 1684, 2380);

        /// <summary>
        /// Size A2
        /// </summary>
        public static readonly PageSize A2 = new PageSize("A2", 1190, 1164);

        /// <summary>
        /// Size A3
        /// </summary>
        public static readonly PageSize A3 = new PageSize("A3", 842, 1190);

        /// <summary>
        /// Size A4
        /// </summary>
        public static readonly PageSize A4 = new PageSize("A4", 595, 842);

        /// <summary>
        /// Size A5
        /// </summary>
        public static readonly PageSize A5 = new PageSize("A5", 421, 595);

        /// <summary>
        /// Iterate through all predefined sizes
        /// </summary>
        public static IEnumerable<PageSize> Values
        {
            get
            {
                yield return A0;
                yield return A1;
                yield return A2;
                yield return A3;
                yield return A4;
                yield return A5;
            }
        }

        /// <summary>
        /// The name of the size
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The width of the size
        /// </summary>
        public readonly int Width;

        /// <summary>
        /// The height of the size
        /// </summary>
        public readonly int Height;

        /// <summary>
        /// Create a new page size
        /// </summary>
        /// <param name="name">The name of the size</param>
        /// <param name="width">The width of the size</param>
        /// <param name="height">The height of the size</param>
        protected PageSize(string name, int width, int height)
        {
            Name = name;
            Width = width;
            Height = height;
        }
        public override string ToString() { return Name; }
    }
}
