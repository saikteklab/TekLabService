using System;
using System.Collections.Generic;
using System.Text;

namespace MongoClient
{
    class MongoConstants
    {
        public const string Objectid = "_id";
        public const string Payload = "Payload";
        public const string DefaultPort = "27017";
        public const string DefaultServerIP = "localhost";
        public const string DefaultDBName = "local";
        public const string ConnectionStringPrefix = "mongodb://";
        public const int MaxRecordCount = 10;

        public const string AESKey = "TekLab#Mongo@DB";
        public const string MongoAuthenticationDB = "admin";
        public static string name = "name";


    }
}
