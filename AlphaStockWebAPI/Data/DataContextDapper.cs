using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DotnetAPI.Data
{
    public class DataContextDapper
    {
        private IConfiguration _config; // The IConfiguration object is automatically available in the dependency injection container because it is built by the WebApplication.CreateBuilder(args) method in Program.cs. This object contains all the configuration settings from appsettings.json, appsettings.Development.json, environment variables, and other sources.
                                        // When DataContextEF is instantiated, ASP.NET Core will automatically inject the IConfiguration instance into its constructor which is below.

        public DataContextDapper(IConfiguration config)
        {
            _config = config;
        }


        // This string is used to configure and open a connection to the SQL Server database (local or remote depending on server config).
        // _ before the variable name is convention for Private fields

        public IEnumerable<T> LoadData<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Query<T>(sql);
        }
        // The LoadData<T> method is a generic method that runs a SQL query (supplied as a string argument).
        // LoadData<T> - Hey compiler, in this method, I’m going to use a generic type called T. You’ll find it in the return type or parameters — but don’t worry, I’ll tell you what T means when the method is called.
        // string sql - It also expects a string containing an SQL query to SELECT table headers - by including (string sql)
        // IEnumerable<T> — It returns a collection of objects (instances of T), each representing a row from the SQL result set.
        // to work, the method uses the SqlConnection class from the Microsoft.Data.SqlClient library, which requires an string argument containing the configuration to the Server database
        // The SqlConnection class is used to generate an instance (a real SQL Server connection) that is stored in a variable declared as IDbConnection, which is an interface (a contract defining the behavior of a database connection)
        // Query is a Dapper method described in the sqlmapper  


        public T LoadDataSingle<T>(string sql) // when we call this method, we need to define the type (<T> is just a placeholder) and then pass the argument (i.e. (string sql))
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.QuerySingle<T>(sql);
        }
        // It returns a primitive type — it could be any primitive type as defined in the model class.

        public bool ExecuteSql(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return (dbConnection.Execute(sql) > 0);
        }
        // It returns a boolean.

        public int ExecuteSqlWithRowCount(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql);
        }

        public bool ExecuteSqlWithParameters(string sql, DynamicParameters parameters)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql, parameters) > 0;
            
            // SqlCommand commandWithParam = new SqlCommand(sql);

            // foreach (SqlParameter parameter in parameters)
            // {
            //     commandWithParam.Parameters.Add(parameter);
            // }



            // SqlConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            // dbConnection.Open();
            // commandWithParam.Connection = dbConnection;
            // int rowsAffected = commandWithParam.ExecuteNonQuery();
            // dbConnection.Close();

            // return rowsAffected > 0;
        }        
        
        public IEnumerable<T> LoadDataWithParameters<T>(string sql, DynamicParameters parameters)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Query<T>(sql, parameters);
        }
        // The LoadData<T> method is a generic method that runs a SQL query (supplied as a string argument).
        // LoadData<T> - Hey compiler, in this method, I’m going to use a generic type called T. You’ll find it in the return type or parameters — but don’t worry, I’ll tell you what T means when the method is called.
        // string sql - It also expects a string containing an SQL query to SELECT table headers - by including (string sql)
        // IEnumerable<T> — It returns a collection of objects (instances of T), each representing a row from the SQL result set.
        // to work, the method uses the SqlConnection class from the Microsoft.Data.SqlClient library, which requires an string argument containing the configuration to the Server database
        // The SqlConnection class is used to generate an instance (a real SQL Server connection) that is stored in a variable declared as IDbConnection, which is an interface (a contract defining the behavior of a database connection)
        // Query is a Dapper method described in the sqlmapper  


        public T LoadDataSingleWithParameters<T>(string sql, DynamicParameters parameters) // when we call this method, we need to define the type (<T> is just a placeholder) and then pass the argument (i.e. (string sql))
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.QuerySingle<T>(sql, parameters);
        }
    }
}