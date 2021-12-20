using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MongoClient
{
    public class MongoDBWrapper
    {
        #region Class members
        public string Server { get; set; }
        public string Port { get; set; }
        public string DBName { get; set; }
        public string DBUserName { get; set; }
        public string DBPassword { get; set; }
        public string DBSecurity { get; set; }
        #endregion

        #region Private members
        public string _connectionString;
        private EncryptionManager m_AesManager;
        #endregion 

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="server">Server IP</param>
        /// <param name="port">Server Port</param>
        /// <param name="dbname">Server Port</param>
        public MongoDBWrapper(string connectionString,string _DBName)
        {
            //Server = server.Trim();
            //Port = port.Trim();
            // DBName = dbname.Trim();
            //DBUserName = dbusername.Trim();
            //DBPassword = dbpassword.Trim();
            //DBSecurity = dbsecurity.Trim();
            _connectionString = connectionString;
             DBName = _DBName;
            //ValidateAndCreateConnectionString();
        }

        /// <summary>
        /// Validate server ip, port, db name and create connection string.
        /// </summary>
        private void ValidateAndCreateConnectionString()
        {
            // If server or port is null then throw exception
            if (String.IsNullOrEmpty(Server) || String.IsNullOrEmpty(Port))
            {
                throw new Exception("Server ip or server port is empty.");
            }

            // Check DBName
            if (String.IsNullOrEmpty(DBName))
            {
                throw new Exception("DB Name is empty.");
            }

            if (String.IsNullOrEmpty(DBSecurity))
            {
                throw new Exception("DB Security is empty.");
            }

            // Security is On append Username and Password to connectionstring
            if (!Convert.ToBoolean(DBSecurity))
            {
                // Create connection string in format mongodb://machineip:port
                _connectionString = MongoConstants.ConnectionStringPrefix + Server + ":" + Port;

            }
            else
            {

                if (String.IsNullOrEmpty(DBUserName) || String.IsNullOrEmpty(DBPassword))
                {
                    throw new Exception("Username or Password are empty");
                }
                m_AesManager = new EncryptionManager();
                DBUserName = m_AesManager.DecryptText(DBUserName, MongoConstants.AESKey);
                DBPassword = m_AesManager.DecryptText(DBPassword, MongoConstants.AESKey);
                _connectionString = MongoConstants.ConnectionStringPrefix + DBUserName + ":" + DBPassword + "@" + Server + ":" + Port + "/" + MongoConstants.MongoAuthenticationDB;

            }


        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Insert large number of documents at once.
        /// </summary>
        /// <param name="jsonDocs">Json document list</param>
        /// <param name="collection">Collection to insert</param>
        /// <returns>Number of records inserted</returns>
        public async Task BulkInsert(List<string> jsonDocs, string collection)
        {
            // Get clinical\imaging collection and init bulk operation
            var dataCol = GetCollection(collection);

            List<BsonDocument> alldocs = new List<BsonDocument>();
            foreach (string jsonDoc in jsonDocs)
            {
                // Get bson doc from json
                BsonDocument bsonDocument = BsonDocument.Parse(jsonDoc);
                alldocs.Add(bsonDocument); // Insert doc
            }
            await dataCol.InsertManyAsync(alldocs);
        }

        /// <summary>
        /// Insert a json document to collection.
        /// </summary>
        /// <param name="jsonData"> Json string to insert. </param>
        /// <param name="collection"> Collection to insert. </param>
        /// <returns> Result response. </returns>
        public async Task Insert(string jsonData, string collection)
        {
            // Get collection to update
            var dataCol = GetCollection(collection);
            // Get bson doc from json
            var bsonDocument = BsonDocument.Parse(jsonData);
            // Insert doc
            await dataCol.InsertOneAsync(bsonDocument);
        }

        /// <summary>
        /// Inserts the asynchronous.
        /// </summary>
        /// <param name="jsonData">The json data.</param>
        /// <param name="collection">The collection.</param>
        /// <returns></returns>
        public async Task<string> InsertAsync(string jsonData, string collection)
        {
            BsonValue value = null;
            // Get collection to update
            var dataCol = GetCollection(collection);
            // Get bson doc from json
            var bsonDocument = BsonDocument.Parse(jsonData);
            // Insert doc
            await dataCol.InsertOneAsync(bsonDocument);
            // Getting inserted object id
            bsonDocument.TryGetValue("_id", out value);
            // return object id
            return value.ToString();
        }

        public async Task Update(string key, string value, string jsonData, string collection)
        {
            // Get collection to update
            var dataCol = GetCollection(collection);
            // Get bson doc from json
            var bsonDocument = BsonDocument.Parse(jsonData);

            var options = new UpdateOptions { IsUpsert = true };
            var result = await dataCol.ReplaceOneAsync(
                new BsonDocument(key, value),
                bsonDocument, options);
        }

        public async Task Update(string key, string value, Dictionary<string, string> valuesToUpdate,
            string collection)
        {
            // Get collection to update
            var dataCol = GetCollection(collection);
            var filter = Builders<BsonDocument>.Filter.Eq(key, value);

            var update = "";

            var result = await dataCol.UpdateOneAsync(filter, update);
        }

      

        /// <summary>
        /// Updates the specified unique key.
        /// </summary>
        /// <param name="uniqueKey"> The unique key. </param>
        /// <param name="uniqueValue"> The unique value. </param>
        /// <param name="updateKey"> The update key. </param>
        /// <param name="updateValue"> The update value. </param>
        /// <param name="collection"> The collection. </param>        
        public async Task Update(string uniqueKey, string uniqueValue, string updateKey, string updateValue, string collection)
        {
            // Get collection to update.
            var dataCol = GetCollection(collection);

            // Set filter and update queries.
            var filter = Builders<BsonDocument>.Filter.Eq(uniqueKey, uniqueValue);
            var update = Builders<BsonDocument>.Update.Set(updateKey, updateValue);

            // Update one document
            var result = await dataCol.UpdateOneAsync(filter, update);
        }

        /// <summary>
        /// Update the filtered records with a set of values
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="valuesToUpdate"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public async Task UpdateMany<TFilterType, TValueType>(Dictionary<string, TFilterType> filterValues, Dictionary<string, TValueType> valuesToUpdate, string collection)
        {
            // Get collection to update
            var dataCol = GetCollection(collection);

            // Validate there are at least one filter and one update element
            if ((filterValues.Count() < 1) || (valuesToUpdate.Count() < 1))
                return;

            // Prepare the filter
            var firstFilterKey = filterValues.Keys.First();
            var filter = Builders<BsonDocument>.Filter.Eq(firstFilterKey, filterValues[firstFilterKey]);

            foreach (var filterKey in filterValues.Keys)
            {
                if (filterKey != firstFilterKey)
                    filter = filter & Builders<BsonDocument>.Filter.Eq(filterKey, filterValues[filterKey]);
            }

            // Prepare columns to update
            var firstKey = valuesToUpdate.Keys.First();
            var update = Builders<BsonDocument>.Update.Set(firstKey, valuesToUpdate[firstKey]);

            if (valuesToUpdate.Count > 1)
                foreach (var valueKey in valuesToUpdate.Keys)
                {
                    if (valueKey != firstKey)
                        update = update.Set(valueKey, valuesToUpdate[valueKey]);
                }

            var result = await dataCol.UpdateManyAsync(filter, update);
        }

        /// <summary>
        /// Determines whether [is document exist] [the specified key].
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="collection">The collection.</param>
        /// <returns></returns>
        public async Task<bool> IsDocumentExist(string key, string value, string collection)
        {
            // Get collection to update
            var dataCol = GetCollection(collection);
            var filter = Builders<BsonDocument>.Filter.Eq(key, value);
            var count = await dataCol.CountAsync(filter);
            if (count <= 0)
                return false; // No documents found

            return true; // Document found
        }

        /// <summary>
        /// Collections the exists.
        /// </summary>
        /// <param name="collectionName"> Name of the collection. </param>
        /// <returns> Collection exists or not(true/false). </returns>
        public async Task<bool> CollectionExists(string collectionName)
        {
            var filter = new BsonDocument(MongoConstants.name, collectionName);
            // Filter by collection name.
            var client = new MongoDB.Driver.MongoClient(_connectionString);
            var collections = await client.GetDatabase(DBName).ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
            // Check for existence.
            int count = (await collections.ToListAsync()).Count;
            if (count > 0)
                return true;
            else
                return false;
        }

        public async Task<string> Find(string key, string value, string collection)
        {
            try
            {
                var dataCol = GetCollection(collection);
                var filter = Builders<BsonDocument>.Filter.Eq(key, value);
                var count = await dataCol.CountAsync(filter);
                if (count <= 0)
                    return ""; // No documents found
                var projection = Builders<BsonDocument>.Projection.Exclude("_id");
                var document = await dataCol.Find(filter).Project(projection).FirstAsync();

                var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
                return document.ToJson(jsonWriterSettings);
            }
            catch (Exception ex)
            {

                throw;
            }
            // Get collection to update
           

            //return document.ToString();
        }

        public async Task<string> FindMultiple(string key, string value, string collection)
        {
            // Get collection to update
            var dataCol = GetCollection(collection);
            var filter = Builders<BsonDocument>.Filter.Eq(key, value);
            var count = await dataCol.CountAsync(filter);
            if (count <= 0)
                return ""; // No documents found

            var projection = Builders<BsonDocument>.Projection.Exclude("_id");
            var document = await dataCol.Find(filter).Project(projection).ToListAsync();

            var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
            return document.ToJson(jsonWriterSettings);

            //return document.ToString();
        }

        public async Task<string> FindAll(string collection)
        {
            // Get collection to update
            var dataCol = GetCollection(collection);
            var documents = await dataCol.Find(_ => true).ToListAsync();
            var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
            return documents.ToJson(jsonWriterSettings);
        }

       
        /// <summary>
        /// Gets the data according to field.
        /// </summary>
        /// <param name="collection"> The collection. </param>
        /// <param name="field"> The field. </param>
        /// <returns> DB value. </returns>
        public async Task<string> GetData(string collection, string field)
        {
            string value = "";
            var dataCol = GetCollection(collection);
            var list = await dataCol.Find(new BsonDocument()).ToListAsync();
            foreach (var item in list)
            {
                value = BsonTypeMapper.MapToDotNetValue(item[field]).ToString();
                break;
            }
            return value;
        }

        /// <summary>
        /// Gets all data of a specific collection.
        /// </summary>
        /// <param name="collection"> The collection. </param>        
        /// <returns> BsonDocument array. </returns>
        public async Task<List<BsonDocument>> GetAllData(string collection)
        {
            var dataCol = GetCollection(collection);
            var list = await dataCol.Find(new BsonDocument()).ToListAsync();
            return list;
        }

        /// <summary>
        /// Gets the collection.
        /// </summary>
        /// <param name="collection"> Collection name. </param>
        /// <returns> Collection instance. </returns>
        public IMongoCollection<BsonDocument> GetCollection(string collection)
        {
            if (String.IsNullOrEmpty(collection))
                throw new Exception("The collection is null or empty");

            var client = new MongoDB.Driver.MongoClient(_connectionString);
            var mongoDb = client.GetDatabase(DBName);
            var dataCol = mongoDb.GetCollection<BsonDocument>(collection);

            if (dataCol == null)
                throw new Exception("The data collection is null");
            return dataCol;
        }

        /// <summary>
        /// Delete multiple records based on the filter
        /// </summary>
        /// <param name="collection"> Collection name. </param>
        /// <param name="collection"> Collection name. </param>
        /// <param name="collection"> Collection name. </param>
        /// <returns> Collection instance. </returns>
        public async Task DeleteMany(string key, string value, string collection)
        {
            // Get collection to update
            var dataCol = GetCollection(collection);
            var filter = Builders<BsonDocument>.Filter.Eq(key, value);
            var result = await dataCol.DeleteManyAsync(filter);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public async Task DeleteOne(string key, string value, string collection)
        {
            var dataCol = GetCollection(collection);
            var filter = Builders<BsonDocument>.Filter.Eq(key, value);
            var result = await dataCol.DeleteOneAsync(filter);
        }

        public IMongoCollection<BsonDocument> GetCollection(string collection, string dataBase)
        {
            if (String.IsNullOrEmpty(collection))
                throw new Exception("The collection is null or empty");

            var client = new MongoDB.Driver.MongoClient(_connectionString);
            var mongoDb = client.GetDatabase(dataBase);
            var dataCol = mongoDb.GetCollection<BsonDocument>(collection);

            if (dataCol == null)
                throw new Exception("The data collection is null");
            return dataCol;
        }

        #endregion         
    }
}
