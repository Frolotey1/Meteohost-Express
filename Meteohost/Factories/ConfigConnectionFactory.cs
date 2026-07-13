using Daocore.OracleEngine;
using MeteoLib.Interfaces;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Concurrent;

internal class ConfigConnectionFactory : IConnectionFactory
{

    private readonly IConfiguration _configuration;
    private readonly ConcurrentDictionary<string, string> _csDictionary = new(); 

    public ConfigConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }




    public OracleConnection CreateMegaConnection()
    {


        var s = _configuration.GetConnectionString("svx");

        if (string.IsNullOrWhiteSpace(s))
            throw new Exception("connection string is empty");

        var c = new OracleConnection(s);
        c.Open();
        return c;


    }


    public OracleConnection CreateAssetConnection(string asset)
    {
        var assetUpper = asset.ToUpper();

        if (assetUpper == "SVX") return CreateMegaConnection();

        var connectionString = string.Empty;

        if (_csDictionary.ContainsKey(assetUpper))
        {
            connectionString = _csDictionary[assetUpper];
        }
        else
        {

            using var mainConnection = CreateMegaConnection();

            var sql = @"select ID_ASSETS, DB_NAME, spp.database_pkg.get_connection_string(ID_ASSETS) connection
                        from nsi.dict_assets t inner join nsi.dict_ap ap on ap.id=t.id_assets
                        where iata=:0";

            var row = QM.QueryRows(mainConnection, sql, assetUpper).FirstOrDefault();

            if (row is null)
                throw new Exception($"Missing asset {assetUpper} in nsi.dict_assets ");

            var connectionParams = row.AsString("connection").Split(';');

            connectionString = $"Data Source = {connectionParams[0]}:{connectionParams[1]}/{connectionParams[2]}; User Id = spp; Password = plan";
            
            _csDictionary.TryAdd(assetUpper, connectionString);
        }

        var c = new OracleConnection(connectionString);

        c.Open();

        return c;


    }



}