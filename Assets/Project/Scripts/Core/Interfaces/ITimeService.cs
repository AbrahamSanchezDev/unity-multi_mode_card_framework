using System;
using System.Threading.Tasks;

namespace CardFramework.Core.Interfaces {
    public interface ITimeService {
        /// <summary>
        /// Fetches the authoritative UTC time straight from the PlayFab cloud servers
        /// to prevent client-side device clock tampering.
        /// </summary>
        Task<DateTime> GetServerTimeUtcAsync();
    }
}