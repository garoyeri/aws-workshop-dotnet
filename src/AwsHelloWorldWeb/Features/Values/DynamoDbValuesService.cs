namespace AwsHelloWorldWeb.Features.Values
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.DynamoDBv2.DataModel;
    using Amazon.DynamoDBv2.DocumentModel;

    public class DynamoDbValuesService : IValuesService
    {
        private readonly IDynamoDBContext _context;

        public DynamoDbValuesService(IDynamoDBContext context)
        {
            _context = context;
        }

        public static string LatestIdHashKey = "latest|0";
        public static string GenerateHashKey(int id) => $"value|{id}";
        public static int ExtractId(string hashKey) => int.Parse(hashKey.Split("|").Last());

        /// <summary>
        /// Get the value by ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<string> Get(int id, CancellationToken cancellationToken = default)
        {
            return (await _context.LoadAsync<DynamoDbValueItem>(GenerateHashKey(id), cancellationToken))?.Value;
        }

        /// <summary>
        /// List the values in ID order
        /// </summary>
        /// <param name="maxItems"></param>
        /// <param name="useBackwardQuery"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<string>> List(int maxItems = 100, bool? useBackwardQuery = null, CancellationToken cancellationToken = default)
        {
            maxItems = Math.Clamp(maxItems, 1, 100);
            var filter = new ScanFilter();
            filter.AddCondition("id", ScanOperator.BeginsWith, "value|");
            var search = _context.FromScanAsync<DynamoDbValueItem>(new ScanOperationConfig
            {
                IndexName = DynamoDbValueItem.SortedIndex,
                Limit = maxItems,
                Filter = filter
            }, new DynamoDBOperationConfig
            {
                BackwardQuery = useBackwardQuery
            });

            return (await search.GetRemainingAsync(cancellationToken))
                .OrderBy(v => v.Id)
                .Select(v => v.Value)
                .ToList();
        }

        /// <summary>
        /// Append a new value as the next ID
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Append(string value, CancellationToken cancellationToken = default)
        {
            var latest = await _context.LoadAsync<DynamoDbValueItem>(LatestIdHashKey, cancellationToken);
            if (latest == null)
            {
                latest = new DynamoDbValueItem(LatestIdHashKey, GenerateHashKey(1));
            }
            else
            {
                latest.Value = GenerateHashKey(ExtractId(latest.Value) + 1);
            }

            var newItem = new DynamoDbValueItem(latest.Value, value);

            var batch = _context.CreateBatchWrite<DynamoDbValueItem>();
            batch.AddPutItem(latest);
            batch.AddPutItem(newItem);

            await batch.ExecuteAsync(cancellationToken);
        }

        /// <summary>
        /// Insert or update a value by ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Upsert(int id, string value, CancellationToken cancellationToken = default)
        {
            var item = new DynamoDbValueItem(id, value);
            return _context.SaveAsync(item, cancellationToken);
        }

        /// <summary>
        /// Delete the specified value by ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Delete(int id, CancellationToken cancellationToken = default)
        {
            return _context.DeleteAsync<DynamoDbValueItem>(GenerateHashKey(id), cancellationToken);
        }
    }
}