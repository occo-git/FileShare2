using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2;

namespace FileShare.Factories
{
    public interface ITableFactory
    {
        Table Create(string tableName);
    }

    public class TableFactory : ITableFactory
    {
        private readonly IAmazonDynamoDB _dynamoDBClient;

        public TableFactory() { }

        public TableFactory(IAmazonDynamoDB dynamoDBClient)
        {
            _dynamoDBClient = dynamoDBClient;
        }

        public Table Create(string tableName)
        {
            return Table.LoadTable(_dynamoDBClient, tableName);
        }
    }
}
