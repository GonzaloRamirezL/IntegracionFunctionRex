using Common.ViewModels;
using System.Collections.Generic;

namespace IBusiness
{
    public interface ITimeOffBusiness
    {
        /// <summary>
        /// Synchronize all type of times off from files to GeoVictoria
        /// </summary>
        void SynchronizeAllTimesOff(RexExecutionVM rexExecutionVM);
    }
}
