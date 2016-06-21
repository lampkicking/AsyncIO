using System;
using System.Net;
using System.Net.Sockets;

namespace AsyncIO.DotNet
{
    class NativeSocket : AsyncSocket
    {
        private readonly Socket _mSocket;

        private CompletionPort _mCompletionPort;
        private object _mState;

        private readonly SocketAsyncEventArgs _mInSocketAsyncEventArgs;
        private readonly SocketAsyncEventArgs _mOutSocketAsyncEventArgs;

        public NativeSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
            : base(addressFamily, socketType, protocolType)
        {
            _mSocket = new Socket(addressFamily, socketType, protocolType);

            _mInSocketAsyncEventArgs = new SocketAsyncEventArgs();
            _mInSocketAsyncEventArgs.Completed += OnAsyncCompleted;

            _mOutSocketAsyncEventArgs = new SocketAsyncEventArgs();
            _mOutSocketAsyncEventArgs.Completed += OnAsyncCompleted;
        }

        public override IPEndPoint LocalEndPoint  => (IPEndPoint)_mSocket.LocalEndPoint;

        public override IPEndPoint RemoteEndPoint => (IPEndPoint)_mSocket.RemoteEndPoint;

        private void OnAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            OperationType operationType;

            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Accept:       operationType = OperationType.Accept;       break;
                case SocketAsyncOperation.Connect:      operationType = OperationType.Connect;      break;
                case SocketAsyncOperation.Receive:      operationType = OperationType.Receive;      break;
                case SocketAsyncOperation.Send:         operationType = OperationType.Send;         break;
                case SocketAsyncOperation.Disconnect:   operationType = OperationType.Disconnect;   break;
                default: throw new ArgumentOutOfRangeException();
            }

            var completionStatus = new CompletionStatus(this, _mState, operationType, e.SocketError, e.BytesTransferred);

            _mCompletionPort.Queue(ref completionStatus);
        }

        internal void SetCompletionPort(CompletionPort completionPort, object state)
        {
            _mCompletionPort = completionPort;
            _mState = state;
        }

        public override void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue)
        {
            _mSocket.SetSocketOption(optionLevel, optionName, optionValue);
        }

        public override void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
        {
            _mSocket.SetSocketOption(optionLevel, optionName, optionValue);
        }

        public override void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue)
        {
            _mSocket.SetSocketOption(optionLevel, optionName, optionValue);
        }

        public override void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
        {
            _mSocket.SetSocketOption(optionLevel, optionName, optionValue);
        }

        public override object GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName)
        {
            return _mSocket.GetSocketOption(optionLevel, optionName);
        }

        public override void GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
        {
            _mSocket.GetSocketOption(optionLevel, optionName, optionValue);
        }

        public override byte[] GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionLength)
        {
            return _mSocket.GetSocketOption(optionLevel, optionName, optionLength);
        }


        public override int IOControl(IOControlCode ioControlCode, byte[] optionInValue, byte[] optionOutValue)
        {
            return _mSocket.IOControl(ioControlCode, optionInValue, optionOutValue);
        }

        public override void Dispose()
        {
            (_mSocket as IDisposable).Dispose();
        }

        public override void Bind(IPEndPoint localEndPoint)
        {
            _mSocket.Bind(localEndPoint);
        }

        public override void Listen(int backlog)
        {
            _mSocket.Listen(backlog);
        }

        public override void Connect(IPEndPoint endPoint)
        {
            _mOutSocketAsyncEventArgs.RemoteEndPoint = endPoint;

            if (!_mSocket.ConnectAsync(_mOutSocketAsyncEventArgs))
            {
                var completionStatus = new CompletionStatus(this, _mState, OperationType.Connect, SocketError.Success, 0);

                _mCompletionPort.Queue(ref completionStatus);
            }
        }

        public override void Accept(AsyncSocket socket)
        {
            var nativeSocket = (NativeSocket)socket;

            _mInSocketAsyncEventArgs.AcceptSocket = nativeSocket._mSocket;

            if (!_mSocket.AcceptAsync(_mInSocketAsyncEventArgs))
            {
                var completionStatus = new CompletionStatus(this, _mState, OperationType.Accept, SocketError.Success, 0);

                _mCompletionPort.Queue(ref completionStatus);
            }
        }

        public override void Send(byte[] buffer, int offset, int count, SocketFlags flags)
        {
            if (_mOutSocketAsyncEventArgs.Buffer != buffer)
            {
                _mOutSocketAsyncEventArgs.SetBuffer(buffer, offset, count);
            }
            else if (_mOutSocketAsyncEventArgs.Offset != offset || _mInSocketAsyncEventArgs.Count != count)
            {
                _mOutSocketAsyncEventArgs.SetBuffer(offset, count);
            }

            if (!_mSocket.SendAsync(_mOutSocketAsyncEventArgs))
            {
                var completionStatus = new CompletionStatus(this, _mState, OperationType.Send, _mOutSocketAsyncEventArgs.SocketError,
                    _mOutSocketAsyncEventArgs.BytesTransferred);

                _mCompletionPort.Queue(ref completionStatus);
            }
        }

        public override void Receive(byte[] buffer, int offset, int count, SocketFlags flags)
        {
            _mInSocketAsyncEventArgs.AcceptSocket = null;

            if (_mInSocketAsyncEventArgs.Buffer != buffer)
            {
                _mInSocketAsyncEventArgs.SetBuffer(buffer, offset, count);
            }
            else if (_mInSocketAsyncEventArgs.Offset != offset || _mInSocketAsyncEventArgs.Count != count)
            {
                _mInSocketAsyncEventArgs.SetBuffer(offset, count);
            }

            if (!_mSocket.ReceiveAsync(_mInSocketAsyncEventArgs))
            {
                var completionStatus = new CompletionStatus(this, _mState, OperationType.Receive, _mInSocketAsyncEventArgs.SocketError,
                    _mInSocketAsyncEventArgs.BytesTransferred);

                _mCompletionPort.Queue(ref completionStatus);
            }
        }
    }
}
