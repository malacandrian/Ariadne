using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zoom.Generator
{
    /// <summary>
    /// SectionSelector declares the links between the states of the FSM, 
    /// and randomly determines which one should be followed at each step
    /// </summary>
    /// <remarks>
    /// Rather than specifying probabilities, and having to ensure they all add up to 1
    /// SectionSelector instead uses weights, where the probability of each result
    /// is equal to window/W where 'window' is the weight of the result, and 'W' us the total weight of all results
    /// </remarks>
    public class SectionSelector
    {
        /// <summary>
        ///The list of sections and relevant weights
        /// </summary>
        private Dictionary<Section, int> _Sections;

        /// <summary>
        ///The list of sections and relevant weights
        /// </summary>
        public ReadOnlyDictionary<Section, int> Sections { get { return new ReadOnlyDictionary<Section, int>(_Sections); } }

        /// <summary>
        /// The weight that the FSM will choose to terminate after this iteration
        /// </summary>
        public int TerminateWeight = 0;

        /// <summary>
        /// The total weight of all possible results
        /// </summary>
        protected int TotalWeight { get { return Sections.Values.Sum() + TerminateWeight; } }

        /// <summary>
        /// A random number generator for randomly selecting sections
        /// </summary>
        protected static Random _Random = new Random();

        /// <summary>
        /// Create a new SectionSelector with an empty list
        /// </summary>
        public SectionSelector() : this(new Dictionary<Section, int>()) { }

        /// <summary>
        /// Create a new SectionSelector from a dictionary of sections and weights
        /// </summary>
        /// <param Name="sections"></param>
        public SectionSelector(Dictionary<Section, int> sections) { _Sections = sections; }

        /// <summary>
        /// Add a new section to the SectionSelector
        /// </summary>
        /// <param Name="section">The section to add</param>
        /// <param Name="weight">The weight to assign to it</param>
        public void AddSection(Section section, int weight) { _Sections.Add(section, weight); }

        /// <summary>
        /// Randomly select a section from the list
        /// </summary>
        /// <returns>The selected section, null if it chose to terminate</returns>
        public Section Select()
        {
            //Pick a random number strictly smaller than the total weight of all results
            int value = _Random.Next(TotalWeight);

            //Check each possible result in turn
            //If the current value is less than the weight for that result, select that result
            //Otherwise, subtract the weight of the result and check the next one
            if (value < TerminateWeight) { return null; }
            value -= TerminateWeight;
            foreach (KeyValuePair<Section, int> sectionWeight in Sections)
            {
                if (value < sectionWeight.Value) { return sectionWeight.Key; }
                else { value -= sectionWeight.Value; }
            }

            //If this is reached, my math was wrong
            throw new Exception("The SectionSelector failed to pick a section");
        }
    }
}
