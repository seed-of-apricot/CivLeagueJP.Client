using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CivLeagueJP.Client.Models.Civ6
{
    internal class Client : SocketClient
    {

        private byte[] buffer = new byte[1048576];
        private int bufferOffset;
        private object responseLock = new object();
        private List<Response> responses = new List<Response>();
        private List<Response> responsesStack = new List<Response>();
        private List<Listener> listeners = new List<Listener>();
        private Dictionary<Listener,int> listenersIndexMap = new Dictionary<Listener, int>();
        private ListenForMessagesDelegate listenForMessagesDelegate;

        private SocketAsyncEventArgs socketArgs;

        private bool ifPendingReceive;
        private byte[] message;
        private int messageIndex;
        private bool ifRoutingMessages;

        public OutputMessageHandler OnOutputMessageReceived;
        private Listener defaultRequestHandler;

        public static Client Instance { get; private set; }


        public static void Init()
        {
            if (Instance != null)
            {
                Instance.Dispose();
            }
            Instance = new Client();
        }

        internal Client()
            :base(new IPEndPoint(IPAddress.Parse("127.0.0.1"),4318))
        {
            defaultRequestHandler = new Listener(DefaultRequestHandler);
            ConnectionEstablished += new EventHandler(OnConnected);
            listenForMessagesDelegate = new ListenForMessagesDelegate(ListenForMessages);
        }

        private void OnConnected(object sender, EventArgs args)
        {
            this.bufferOffset = 0;
            this.message = (byte[])null;
            this.messageIndex = 0;
            this.ifPendingReceive = false;
            this.ListenForMessages();
        }

        private void ListenForMessages()
        {
            if (socketArgs == null)
            {
                socketArgs = new SocketAsyncEventArgs();
                socketArgs.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveAsyncCompleted);
            }
            if (ifPendingReceive)
            {
                return;
            }
            ifPendingReceive = true;
            socketArgs.SetBuffer(buffer, bufferOffset, buffer.Length-bufferOffset);
            ReceiveAsync(socketArgs);
        }

        private async void ReceiveAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            ifPendingReceive = false;
            if (e.SocketError != SocketError.Success)
            {
                return;
            }
            if (e.BytesTransferred == 0)
            {
                using (await asyncLock.LockAsync())
                {
                    OnConnectionLostDelegate();
                }
            }
            else
            {
                OnReceiveData(e.Buffer, e.BytesTransferred);
                using (await asyncLock.LockAsync())
                {
                    listenForMessagesDelegate();
                }
            }
        }

        private void OnReceiveData(byte[] data, int bytes)
        {
            int length = bufferOffset + bytes;
            if (length > data.Length)
            {
                throw new Exception("Invaild Data Size");
            }
            bufferOffset = 0;
            int startIndex = 0;
            while (startIndex<length)
            {
                if (message == null)
                {
                    int position = length - startIndex;
                    if (position >= 4)
                    {
                        int int32 = BitConverter.ToInt32(buffer, startIndex);
                        int threshold = 524288;
                        if ((uint)int32 > (uint)threshold)
                        {
                            throw(new NotImplementedException());
                        }
                        startIndex += 4;
                        message = new byte[int32 + 4];
                        messageIndex = 0;
                    }
                    else
                    {
                        for (int index = 0; index < position; ++index)
                        {
                            buffer[index] = buffer[startIndex + index];
                        }
                        bufferOffset = position;
                        break;
                    }
                }
                else
                {
                    while (messageIndex<message.Length && startIndex < length)
                    {
                        byte[] _message = message;
                        int _messageIndex = this.messageIndex;
                        messageIndex += 1;
                        int index = messageIndex;
                        int number = data[startIndex];
                        message[index] = (byte)number;
                    }
                    if (messageIndex == message.Length)
                    {
                        OnMessageReceived(message);
                        message = null;
                    }
                }
            }
        }

        private void OnMessageReceived(byte[] message)
        {
            BinaryReader reader = new BinaryReader((Stream)new MemoryStream(message));
            uint number = reader.ReadUInt32();
            int count = message.Length - 4;
            char[] charArray = reader.ReadChars(count);
            Response response = new Response
            {
                Listener = (int)number,
                Messages = new List<string>()
            };
            string empty = string.Empty;
            for (int index = 0; index < charArray.Length; index++)
            {
                char ch = charArray[index];
                if (ch ==0)
                {
                    response.Messages.Add(empty);
                    empty = string.Empty;
                }
                else
                {
                    empty += ch.ToString();
                }
            }
            lock (responseLock)
            {
                responsesStack.Add(response);
            }
        }

        public void RouteMessages()
        {
            if (ifRoutingMessages)
            {
                return;
            }
            ifRoutingMessages = true;
            lock (responseLock)
            {
                responses.AddRange(responsesStack);
                responsesStack.Clear();
            }
            foreach (Response response in responses)
            {
                GetListener(response.Listener)?.Invoke(response.Messages);
            }
            responses.Clear();
            ifRoutingMessages = false;
        }


        private void DefaultRequestHandler(List<string> results)
        {
            if (results.Count<=0)
            {
                return;
            }
            if (results[0]=="L")
            {
                results.RemoveAt(0);
                //StateWatcher.Instance.OnReceived(results);
            }
            else if (results[0] == "O")
            {
                if (OnOutputMessageReceived == null)
                {
                    return;
                }
                for (int index = 0; index < results.Count; ++index)
                {
                    OnOutputMessageReceived(results[index]);
                }
            }
            else
            {
                if (!(results[0] == "Closing"))
                {
                    return;
                }
                CloseConnection();
            }
        }

        /// <summary>
        /// Listener Handlings
        /// </summary>
        /// <param name="listener"></param>
        /// <returns></returns>
        public int AddListener(Listener listener)
        {
            int num = listeners.IndexOf(listener);
            if (num < 0)
            {
                num = listeners.Count;
                listeners.Add(listener);
                listenersIndexMap[listener] = num;
            }
            return num;
        }

        public void RemoveListener(Listener listener)
        {
            if (listener == null || listenersIndexMap.TryGetValue(listener, out int index))
            {
                return;
            }
            listeners[index] = null;
            listenersIndexMap.Remove(listener);
        }

        public void RemoveListener(int index)
        {
            listenersIndexMap.Remove(GetListener(index));
        }

        private Listener GetListener(int index)
        {
            if (index < listeners.Count && index >= 0)
            {
                return listeners[index];
            }
            else
            {
                return DefaultRequestHandler;
            }
        }

        public bool Request(string message, Listener listener)
        {
            return Request(message, AddListener(listener));
        }

        public bool Request(string message, int senderId)
        {
            if (message == null)
            {
                return false;
            }
            bool flag = false;
            try
            {
                uint length = (uint)(message.Length + 1);
                MemoryStream stream = new MemoryStream((int)length + 8);
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(length);
                writer.Write((uint)senderId);
                foreach (var ch in message)
                {
                    writer.Write(ch);
                }
                writer.Write(char.MinValue);
                byte[] buffer = stream.GetBuffer();
                int position = (int)stream.Position;
                uint number = (uint)((ulong)position - 8UL);
                if ((int)length != (int)number)
                {
                    stream.Position = 0L;
                    writer.Write(number);
                }
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(this.SendAsyncCompleted);
                args.SetBuffer(buffer, 0, position);
                flag = SendAsync(args);
            }
            catch (Exception)
            {
                throw;
            }
            return flag;
        }

        private void SendAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                return;
            }
            //OnConnectionLostDelegate
        }
        
        public delegate void ListenForMessagesDelegate();


        internal class Response
        {
            public int Listener;
            public List<string> Messages;
        }

        public delegate void OutputMessageHandler(string outputMessage);
        public delegate void Listener(List<string> response);

    }

    public sealed class AsyncLock
    {
        private readonly System.Threading.SemaphoreSlim m_semaphore
          = new System.Threading.SemaphoreSlim(1, 1);
        private readonly Task<IDisposable> m_releaser;

        public AsyncLock()
        {
            m_releaser = Task.FromResult((IDisposable)new Releaser(this));
        }

        public Task<IDisposable> LockAsync()
        {
            var wait = m_semaphore.WaitAsync();
            return wait.IsCompleted ?
                    m_releaser :
                    wait.ContinueWith(
                      (_, state) => (IDisposable)state,
                      m_releaser.Result,
                      System.Threading.CancellationToken.None,
                      TaskContinuationOptions.ExecuteSynchronously,
                      TaskScheduler.Default
                    );
        }
        private sealed class Releaser : IDisposable
        {
            private readonly AsyncLock m_toRelease;
            internal Releaser(AsyncLock toRelease) { m_toRelease = toRelease; }
            public void Dispose() { m_toRelease.m_semaphore.Release(); }
        }
    }
}
