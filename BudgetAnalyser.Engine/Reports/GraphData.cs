using System.Collections.Generic;
using System.Linq;

namespace BudgetAnalyser.Engine.Reports
{
    /// <summary>
    /// A data storage class to store graph data.  One instance of this class can store multiple lines/series for a graph.
    /// </summary>
    public class GraphData
    {
        public GraphData()
        {
            SeriesList = new List<SeriesData>();
        }

        /// <summary>
        /// The Graph Title
        /// </summary>
        public string GraphName { get; set; }

        /// <summary>
        /// A list of data series, one for each line/series on the graph.
        /// </summary>
        public IEnumerable<SeriesData> Series
        {
            get { return SeriesList; }
        }

        /// <summary>
        /// Calculates the smallest value from all the series stored in <see cref="Series"/>.
        /// </summary>
        public decimal GraphMinimumValue
        {
            get { return SeriesList.Min(s => s.MinimumValue); }
        }

        internal IList<SeriesData> SeriesList { get; set; }
    }
}