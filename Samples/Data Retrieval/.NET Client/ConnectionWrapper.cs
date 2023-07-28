using CanaryWebServiceHelper;
using CanaryWebServiceHelper.HistorianWebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadData
{
    internal class ConnectionException : Exception
    {
        public ConnectionException(string message = "Unable to connect to endpoint.") : base(message)
        {
        }
    }

    internal class ConnectionWrapper : IDisposable
    {
        #region Private Members

        private bool _isConnected;
        private HistorianWebServiceClient _client;
        private HWS_ConnectionType _connectionType;
        private string _host;
        private int? _port;
        private string _appString;
        private string _userString;
        private string _user;
        private string _password;
        private int _cci; // client connection id

        #endregion Private Members

        #region Private Methods

        private bool Connect(string identity = null)
        {
            if (_isConnected)
                return true;

            string host = _host;
            if (_port.HasValue)
                host = $"{host}:{_port.Value}";
            if (!string.IsNullOrEmpty(identity))
                host = $"{host};{identity}";

            try
            {
                HWS_ConnectionHelper.WebServiceConnect(
                    _connectionType,
                    host,
                    _appString,
                    _userString,
                    _user,
                    _password,
                    out _client,
                    out _cci);

                _isConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                if (identity == null && _connectionType == HWS_ConnectionType.NetTcp_Username)
                {
                    // try to parse dns identity from exception
                    // necessary when connecting to username endpoint
                    if (TryGetIdentity(ex, out identity))
                        return Connect(identity);
                }
            }

            _isConnected = false;
            return false;
        }

        private bool Reconnect()
        {
            if (_isConnected)
            {
                Disconnect();
                Connect();
                return true;
            }
            return false;
        }

        private void Disconnect()
        {
            try
            {
                if (_client != null)
                    _client.ReleaseClientConnectId(_appString, _userString, _cci);
                _client.Close();
            }
            catch
            {
            }
            finally
            {
                _isConnected = false;
                _client = null;
                _cci = 0;
            }
        }

        private bool TryGetIdentity(Exception ex, out string identity)
        {
            string message = ex.Message;
            string[] split = message.Split('\'');
            identity = (split.Length >= 4) ? split[3] : null;
            return !string.IsNullOrEmpty(identity);
        }

        private void ValidateConnectionParameters()
        {
            if (!_isConnected)
            {
                if (_connectionType == HWS_ConnectionType.Https_Username || _connectionType == HWS_ConnectionType.NetTcp_Username)
                {
                    if (string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_password))
                        throw new ConnectionException("Credentials must be set when using a username endpoint.");
                }
                else if (!string.IsNullOrEmpty(_user) || !string.IsNullOrEmpty(_password))
                    throw new ConnectionException("Endpoint does not use credentials. Do not set them.");
            }
        }

        #endregion Private Methods

        #region Constructor/Destructor

        public ConnectionWrapper(string host, HWS_ConnectionType connectionType, string application = null, string clientDescription = null)
        {
            _host = host;
            _connectionType = connectionType;
            _appString = string.IsNullOrEmpty(application) ? "Read Data Example" : application;
            _userString = string.IsNullOrEmpty(clientDescription) ? Environment.MachineName : clientDescription;
            _user = null;
            _password = null;
            _cci = 0;
        }

        ~ConnectionWrapper()
        {
            Disconnect();
        }

        #endregion Constructor/Destructor

        #region Public Properties

        public string Username
        {
            set { _user = value; }
        }

        public string Password
        {
            set { _password = value; }
        }

        public int Port
        {
            set { _port = value; }
        }

        #endregion Public Properties

        #region Public Methods

        public void Run(Action<HistorianWebServiceClient, int> action)
        {
            try
            {
                ValidateConnectionParameters();
                if (Connect())
                    action(_client, _cci);
            }
            catch
            {
                if (Reconnect())
                    action(_client, _cci);
                else
                    throw;
            }

            if (!_isConnected)
                throw new ConnectionException();
        }

        public void Dispose()
        {
            Disconnect();
        }

        #endregion Public Methods
    }
}