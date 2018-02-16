using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace CivLeagueJP.Client.Models.Civ6
{
    public abstract class SocketClient : IDisposable
    {
        protected static AsyncLock asyncLock = new AsyncLock();

        private System.Timers.Timer timerForStatusUpdate = new System.Timers.Timer();
        private IPEndPoint iPEndPoint;
        private Socket socket;
        private ConnectionCompletedHandler ConnectionCompletedDelegate;

        public event EventHandler ConnectionEstablished;
        public event EventHandler ConnectionLost;
        public event EventHandler<ConnectionTrialFailedArgs> ConnectionTrialFailed;

        public bool RetryOnConnectionRefused { get; set; }

        public IPEndPoint ConnectionTarget
        {
            get
            {
                return ConnectionTarget;
            }
            set
            {
                if (socket != null)
                {
                    throw new Exception("You must close current connection to change the target.");
                }
                iPEndPoint = value;
            }
        }

        public bool Connected
        {
            get
            {
                if (this.socket != null)
                {
                    return socket.Connected;
                }
                return false;
            }
        }

        public bool Connecting
        {
            get
            {
                if (socket != null)
                {
                    return !socket.Connected;
                }
                return false;
            }
        }

        protected SocketClient(IPEndPoint connectionTarget)
        {
            ConnectionTarget = connectionTarget;
            RetryOnConnectionRefused = true;
            ConnectionCompletedDelegate = new ConnectionCompletedHandler(ConnectionCompleted);
            OnConnectionLostDelegate = new OnConnectionLostHandler(this.OnConnectionLost);
            timerForStatusUpdate.Elapsed += new ElapsedEventHandler(TimerForStatusUpdate_Elapsed);
            timerForStatusUpdate.Interval = 500;
        }

        public void OpenConnection()
        {
            if (socket != null)
            {
                socket.Dispose();
            }
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.RemoteEndPoint = iPEndPoint;
            e.Completed += new EventHandler<SocketAsyncEventArgs>(ConnectionCompleted);
            socket.ConnectAsync(e);
        }

        private async void ConnectionCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.OperationAborted)
            {
                return;
            }
            using (await asyncLock.LockAsync())
            {
                ConnectionCompletedDelegate(e);
            }
        }

        private void OnConnectionFailed(SocketError error)
        {
            EventHandler<ConnectionTrialFailedArgs> connectionFailed = ConnectionTrialFailed;
            if (connectionFailed == null)
            {
                return;
            }
            connectionFailed(this, new ConnectionTrialFailedArgs()
            {
                SocketError = error
            });
        }

        private void ConnectionCompleted(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                EventHandler connectionEstablished = ConnectionEstablished;
                if (connectionEstablished != null)
                {
                    EventArgs empty = EventArgs.Empty;
                    connectionEstablished(this, empty);
                }
                timerForStatusUpdate.Enabled = true;
            }
            else if (e.SocketError == SocketError.ConnectionRefused && RetryOnConnectionRefused)
            {
                if (socket == null)
                {
                    return;
                }
                socket.ConnectAsync(e);
            }
            else
            {
                OnConnectionFailed(e.SocketError);
            }
        }

        public void CloseConnection()
        {
            if (socket == null)
            {
                return;
            }
            timerForStatusUpdate.Enabled = false;
            Socket _socket = socket;
            _socket = null;
            _socket.Close();
        }

        private async void TimerForStatusUpdate_Elapsed(object sender, ElapsedEventArgs e)
        {
            Socket _socket = socket;
            if (_socket == null || _socket.Connected)
            {
                return;
            }
            using (await asyncLock.LockAsync())
            {
                OnConnectionLostDelegate();
            }
        }

        protected OnConnectionLostHandler OnConnectionLostDelegate { get; set; }

        private void OnConnectionLost()
        {
            if (socket == null)
            {
                return;
            }
            CloseConnection();
            EventHandler connectionLost = ConnectionLost;
            if (connectionLost == null)
            {
                return;
            }
            connectionLost(this, EventArgs.Empty);
        }

        protected bool SendAsync(SocketAsyncEventArgs args)
        {
            Socket _socket = socket;
            if (_socket != null)
            {
                return _socket.SendAsync(args);
            }
            return false;
        }

        protected bool ReceiveAsync(SocketAsyncEventArgs args)
        {
            Socket _socket = socket;
            if (_socket != null)
            {
                return _socket.ReceiveAsync(args);
            }
            return false;
        }

        public void Dispose()
        {
            if (socket.Connected)
            {
                timerForStatusUpdate.Enabled = false;
                Socket _socket = socket;
                _socket = null;
                _socket.Close();
                ((IDisposable)timerForStatusUpdate).Dispose();
                ((IDisposable)socket).Dispose();
            }
        }

        public class ConnectionTrialFailedArgs : EventArgs
        {
            public SocketError SocketError;
        }

        private delegate void ConnectionCompletedHandler(SocketAsyncEventArgs e);

        protected delegate void OnConnectionLostHandler();
    }
}