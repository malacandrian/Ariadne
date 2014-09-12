using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zoom.Generator
{
    public class SectionSelector
    {
        private Dictionary<Section,int> sections;
        public ReadOnlyDictionary<Section,int> Sections
        {
            get
            {
                return new ReadOnlyDictionary<Section,int>(sections);
            }
        }

        public int TerminateWeight = 0;

        protected int TotalWeight
        {
            get
            {
                return Sections.Values.Sum() + TerminateWeight;
            }
        }

        protected static Random Rand = new Random();

        public SectionSelector() : this(new Dictionary<Section, int>()) { }

        public SectionSelector(Dictionary<Section, int> s)
        {
            sections = s;
        }

        public void AddSection(Section section, int weight)
        {
            sections.Add(section, weight);
        }

        public Section Select()
        {
            int value = Rand.Next(TotalWeight);
            if (value < TerminateWeight)
            {
                return null;
            }
            value -= TerminateWeight;
            foreach (KeyValuePair<Section, int> sectionWeight in Sections)
            {
                if (value < sectionWeight.Value)
                {
                    return sectionWeight.Key;
                }
                else
                {
                    value -= sectionWeight.Value;
                }
            }

            throw new Exception("The SectionSelector failed to pick a section");
        }
    }
}
