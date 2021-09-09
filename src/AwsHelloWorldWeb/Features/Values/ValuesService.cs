namespace AwsHelloWorldWeb.Features.Values
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.DynamoDBv2.DataModel;
    using Amazon.DynamoDBv2.DocumentModel;

    public class ValuesService
    {
        private readonly IDynamoDBContext _context;

        public ValuesService(IDynamoDBContext context)
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
        public Task<ValueItem> Get(int id, CancellationToken cancellationToken = default)
        {
            return _context.LoadAsync<ValueItem>(GenerateHashKey(id), cancellationToken);
        }

        /// <summary>
        /// List the values in ID order
        /// </summary>
        /// <param name="maxItems"></param>
        /// <param name="useBackwardQuery"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<ValueItem>> List(int maxItems = 100, bool? useBackwardQuery = null, CancellationToken cancellationToken = default)
        {
            maxItems = Math.Clamp(maxItems, 1, 100);
            var filter = new ScanFilter();
            filter.AddCondition("id", ScanOperator.BeginsWith, "value|");
            var search = _context.FromScanAsync<ValueItem>(new ScanOperationConfig
            {
                IndexName = ValueItem.SortedIndex,
                Limit = maxItems,
                Filter = filter
            }, new DynamoDBOperationConfig
            {
                BackwardQuery = useBackwardQuery
            });

            return await search.GetRemainingAsync(cancellationToken);
        }

        /// <summary>
        /// Append a new value as the next ID
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ValueItem> Append(string value, CancellationToken cancellationToken = default)
        {
            var latest = await _context.LoadAsync<ValueItem>(LatestIdHashKey, cancellationToken);
            if (latest == null)
            {
                latest = new ValueItem(LatestIdHashKey, GenerateHashKey(1));
            }
            else
            {
                latest.Value = GenerateHashKey(ExtractId(latest.Value) + 1);
            }

            var newItem = new ValueItem(latest.Value, value);

            var batch = _context.CreateBatchWrite<ValueItem>();
            batch.AddPutItem(latest);
            batch.AddPutItem(newItem);

            await batch.ExecuteAsync(cancellationToken);

            return newItem;
        }

        /// <summary>
        /// Insert or update a value by ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ValueItem> Upsert(int id, string value, CancellationToken cancellationToken = default)
        {
            var item = new ValueItem(id, value);
            await _context.SaveAsync(item, cancellationToken);

            return item;
        }

        /// <summary>
        /// Delete the specified value by ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Delete(int id, CancellationToken cancellationToken = default)
        {
            return _context.DeleteAsync<ValueItem>(GenerateHashKey(id), cancellationToken);
        }
    }
}