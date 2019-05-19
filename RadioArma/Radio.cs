using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RadioArma
{
    public class Radio : IDisposable
    {
        public string Url
        {
            get;
            private set;
        }
        public bool Running
        {
            get
            {
                return _running;
            }
            set
            {
                _running = value;
                if (!_running && runningTask != null)
                    runningTask.Wait();
            }
        }
        bool _running;
        Task runningTask;
        

        public event EventHandler<StreamUpdateEventArgs> OnStreamUpdate;

        public static event EventHandler<MessageLogEventArgs> OnMessageLogged;
        
        public Radio(string Url)
        {
            this.Url = Url;
        }

        public void Start(string pluginsPath = null)
        {
            Running = true;
            runningTask = Task.Run(() => GetHttpStream());
        }

        void GetHttpStream()
        {
            do
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(Url);
                    request.Headers.Add("icy-metadata", "1");
                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        //get the position of metadata
                        var metaInt = Convert.ToInt32(response.GetResponseHeader("icy-metaint"));
                        using (var socketStream = response.GetResponseStream())
                        {
                            var buffer = new byte[32768];
                            var metadataLength = 0;
                            var streamPosition = 0;
                            var bufferPosition = 0;
                            var readBytes = 0;
                            var metadataSb = new StringBuilder();

                            while (Running)
                            {
                                if (bufferPosition >= readBytes)
                                {
                                    readBytes = socketStream.Read(buffer, 0, buffer.Length);
                                    bufferPosition = 0;
                                }
                                if (readBytes <= 0)
                                {
                                    Log("Stream over", this);
                                    break;
                                }

                                if (metadataLength == 0)
                                {
                                    if (streamPosition + readBytes - bufferPosition <= metaInt)
                                    {
                                        streamPosition += readBytes - bufferPosition;
                                        ProcessStreamData(buffer, ref bufferPosition, readBytes - bufferPosition);
                                        continue;
                                    }

                                    ProcessStreamData(buffer, ref bufferPosition, metaInt - streamPosition);
                                    metadataLength = Convert.ToInt32(buffer[bufferPosition++]) * 16;
                                    //check if there's any metadata, otherwise skip to next block
                                    if (metadataLength == 0)
                                    {
                                        streamPosition = Math.Min(readBytes - bufferPosition, metaInt);
                                        ProcessStreamData(buffer, ref bufferPosition, streamPosition);
                                        continue;
                                    }
                                }

                                //get the metadata and reset the position
                                while (bufferPosition < readBytes)
                                {
                                    metadataSb.Append(Convert.ToChar(buffer[bufferPosition++]));
                                    metadataLength--;
                                    if (metadataLength == 0)
                                    {
                                        metadataSb.Clear();
                                        streamPosition = Math.Min(readBytes - bufferPosition, metaInt);
                                        ProcessStreamData(buffer, ref bufferPosition, streamPosition);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (IOException ex)
                {
                    Log(string.Format("Handled IOException, reconnecting. Details:\n{0}\n{1}", ex.Message, ex.StackTrace), this);
                }
                catch (SocketException ex)
                {
                    Log(string.Format("Handled SocketException, reconnecting. Details:\n{0}\n{1}", ex.Message, ex.StackTrace), this);
                }
                catch (WebException ex)
                {
                    Log(string.Format("Handled WebException, reconnecting. Details:\n{0}\n{1}", ex.Message, ex.StackTrace), this);
                }
            } while (Running);
        }

        void ProcessStreamData(byte[] buffer, ref int offset, int length)
        {
            if (length < 1)
                return;
            if (OnStreamUpdate != null)
            {
                var data = new byte[length];
                Buffer.BlockCopy(buffer, offset, data, 0, length);
                OnStreamUpdate(this, new StreamUpdateEventArgs(data));
            }
            offset += length;
        }

        public static void Log(string Log, object sender)
        {
            if (OnMessageLogged != null)
                OnMessageLogged(sender, new MessageLogEventArgs(Log));
        }

        IntPtr _disposed = IntPtr.Zero;
        public void Dispose()
        {
            // Thread-safe single disposal
            if (Interlocked.Exchange(ref _disposed, (IntPtr)1) != IntPtr.Zero)
                return;

            Running = false;
            OnMessageLogged = null;
        }

        public void Stop()
        {
            Dispose();
        }
    }

    public class StreamUpdateEventArgs : EventArgs
    {
        public byte[] Data { get; private set; }

        public StreamUpdateEventArgs(byte[] Data)
        {
            this.Data = Data;
        }
    }

    public class MessageLogEventArgs : EventArgs
    {
        public string Message { get; private set; }

        public MessageLogEventArgs(string Message)
        {
            this.Message = Message;
        }
    }

}
