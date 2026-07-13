using Daocore.OracleEngine;
using MeteoLib.Interfaces;

namespace Meteohost.Core.Impl.Messenger
{
    public class FlttMessenger : IFlttMessenger
    {
        private readonly IConnectionFactory _connectionFactory;

        public FlttMessenger(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void Send(string asset, string message) => AddMessage(asset, message);

        private void AddMessage(string asset, string message)
        {
            using var c = _connectionFactory.CreateAssetConnection(asset);

            QM.DML(c, @"insert into event (event_type, custom_text, date_time)
                         values(5, :0, sys_extract_utc(systimestamp))", message);

        }





    }
}
