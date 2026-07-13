using Daocore.OracleEngine;
using Meteohost;
using MeteoLib;
using MeteoLib.Interfaces;
using MeteoLib.LoadService;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System.Text.Json;

namespace Meteohost.Factories
{
    public class Mega10Repository : IRepo
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger _logger;
        private readonly IAirports _airports;

        public Mega10Repository(ILogger<Mega10Repository> logger, IConnectionFactory connectionFactory, IAirports airports)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
            _airports = airports;
        }

        const string f_iata = "iata";
        const string f_sn = "sn";
        const string f_current_json = "current_json";
        const string f_current_eng_json = "current_eng_json";
        const string f_previous_json = "previous_json";
        const string f_triggers_json = "triggers_json";
        const string f_send_proceed = "send_proceed";

        readonly TableAdapter METAR = new("spp.meteo");

        readonly TableAdapter TAF = new("spp.taf");
        const string taf_json = "json";
        const string taf_json_eng = "eng_json";
        const string taf_date = "UPDATE_DT";

        public Packet? GetPacket(string iata)
        {
            using var c = _connectionFactory.CreateMegaConnection();



            var row = METAR.QueryRows(c, f_iata, iata.ToUpper()).FirstOrDefault();

            if (row is null) return null;


            var curJson = row.AsString(f_current_json);
            var prevJson = row.AsString(f_previous_json);
            var trigsJson = row.AsString(f_triggers_json);
            var sn = row.AsInt64(f_sn);
            var proceed = row.AsInteger(f_send_proceed) == 1;


            if (string.IsNullOrEmpty(curJson))
            {
                _logger.LogDebug("current_json is null [{iata}]", iata);
                return null;
            }



            try
            {

                var current = JsonSerializer.Deserialize<MeteoRecord>(curJson);
                var previous = JsonSerializer.Deserialize<MeteoRecord>(prevJson);
                var trigger = JsonSerializer.Deserialize<Trigger>(trigsJson);



                if (current is null) throw new Exception($"current is null [{iata}]");



                var packet = new Packet(sn, previous, current, trigger) { IsTriggerComplete = proceed };


                return packet;
            }
            catch (JsonException e)
            {
                _logger.LogWarning("{Message}, [{Iata}]", e.Message, iata);
                return null;
            }

        }

        private static long QuerySN(OracleConnection c)
        {
            return QM.Nextval(c, "spp.meteo_seq");
        }

        public long SavePacket(string iata, MeteoRecord? previous, MeteoRecord current, MeteoRecord? current_eng, Trigger trigger)
        {
            using var c = _connectionFactory.CreateMegaConnection();

            var curJson = JsonSerializer.Serialize(current);
            var curEngJson = JsonSerializer.Serialize(current_eng);
            var prevJson = JsonSerializer.Serialize(previous);
            var trigJson = JsonSerializer.Serialize(trigger);

            iata = iata.ToUpper();

            var sn = QuerySN(c);

            bool inserted = false;
            bool updated = false;

            if (METAR.Count(c, f_iata, iata) == 0)
            {
                METAR.Insert(c, f_sn, sn, f_iata, iata, f_current_json, curJson, f_current_eng_json, curEngJson, f_previous_json, prevJson, f_triggers_json, trigJson);
                inserted = true;
            }
            else
            {
                var dml = $"update spp.meteo set  {f_sn}=:0, {f_current_json}=:1, {f_current_eng_json}=:2, {f_previous_json}=:3, {f_triggers_json}=:4, {f_send_proceed}=null where {f_iata}=:5";

                QM.DML(c, dml, new object[] { sn, curJson, curEngJson, prevJson, trigJson, iata });
                updated = true;
            }
            _logger.LogInformation("{iata} save Metar / inserted:{inserted} updated:{updated}", iata, inserted, updated);

            return sn;
        }



        public IEnumerable<string> Unprocessed()
        {
            using var c = _connectionFactory.CreateMegaConnection();
            var sql = $"select iata from spp.meteo where send_proceed is null";

            var arr = QM.QueryArray<string>(c, sql).ToList();

            var ca = _airports.CodeIataList.Select(s => s.ToUpper());

            return arr.Intersect(ca);
        }

        public void TriggerComplete(long sn)
        {
            using var c = _connectionFactory.CreateMegaConnection();

            var dml = $"update spp.meteo set {f_send_proceed}=1 where {f_sn}=:0";

            QM.DML(c, dml, sn);
        }

        public void SaveTaf(string iata, TafMeteoRecord taf, TafMeteoRecord taf_eng)
        {
            iata = iata.ToUpper();

            var c = _connectionFactory.CreateMegaConnection();

            var curJson = JsonSerializer.Serialize(taf);
            var curEngJson = JsonSerializer.Serialize(taf_eng);

            bool inserted = false;
            bool updated = false;

            if (TAF.Count(c, f_iata, iata) == 0)
            {
                TAF.Insert(c, f_iata, iata, taf_json, curJson, taf_json_eng, curEngJson);
                inserted = true;
            }
            else
            {
                var dml = $"update spp.TAF set {taf_json} = :0, {taf_json_eng} = :1, {taf_date} = sysdate where {f_iata}=:2";

                QM.DML(c, dml, new object[] { curJson, curEngJson, iata });
                updated = true;
            }
            _logger.LogInformation("{iata} save Taf / inserted:{inserted} updated:{updated}", iata, inserted, updated);
        }
    }
}
