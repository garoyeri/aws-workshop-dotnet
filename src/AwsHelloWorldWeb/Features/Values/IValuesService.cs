namespace AwsHelloWorldWeb.Features.Values
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IValuesService
    {
        /// <summary>
        /// Get the value by ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> Get(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// List the values in ID order
        /// </summary>
        /// <param name="maxItems"></param>
        /// <param name="useBackwardQuery"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<string>> List(int maxItems = 100, bool? useBackwardQuery = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Append a new value as the next ID
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Append(string value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Insert or update a value by ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Upsert(int id, string value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete the specified value by ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Delete(int id, CancellationToken cancellationToken = default);
    }
}