// Copyright (c) 2012, Event Store LLP
// All rights reserved.
//  
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
//  
// Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the following disclaimer in the
// documentation and/or other materials provided with the distribution.
// Neither the name of the Event Store LLP nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//  

using System;
using System.Threading.Tasks;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.System;
using EventStore.ClientAPI.Transport.Tcp;

namespace EventStore.ClientAPI.ClientOperations
{
    internal class ReadStreamEventsForwardOperation : IClientOperation
    {
        private readonly TaskCompletionSource<EventStreamSlice> _source;
        private ClientMessages.ReadStreamEventsForwardCompleted _result;

        private Guid _correlationId;
        private readonly object _corrIdLock = new object();

        private readonly string _stream;
        private readonly int _start;
        private readonly int _count;
        private readonly bool _resolveLinkTos;

        public Guid CorrelationId
        {
            get
            {
                lock(_corrIdLock)
                    return _correlationId;
            }
        }

        public ReadStreamEventsForwardOperation(TaskCompletionSource<EventStreamSlice> source,
                                                Guid corrId, 
                                                string stream, 
                                                int start, 
                                                int count,
                                                bool resolveLinkTos)
        {
            _source = source;

            _correlationId = corrId;
            _stream = stream;
            _start = start;
            _count = count;
            _resolveLinkTos = resolveLinkTos;
        }

        public void SetRetryId(Guid correlationId)
        {
            lock (_corrIdLock)
                _correlationId = correlationId;
        }

        public TcpPackage CreateNetworkPackage()
        {
            lock (_corrIdLock)
            {
                var dto = new ClientMessages.ReadStreamEventsForward(_stream, _start, _count, _resolveLinkTos);
                return new TcpPackage(TcpCommand.ReadStreamEventsForward, _correlationId, dto.Serialize());
            }
        }

        public InspectionResult InspectPackage(TcpPackage package)
        {
            try
            {
                if (package.Command != TcpCommand.ReadStreamEventsForwardCompleted)
                {
                    return new InspectionResult(InspectionDecision.NotifyError,
                        new CommandNotExpectedException(TcpCommand.ReadStreamEventsForwardCompleted.ToString(), 
                                                        package.Command.ToString()));
                }

                var data = package.Data;
                var dto = data.Deserialize<ClientMessages.ReadStreamEventsForwardCompleted>();
                _result = dto;

                switch ((RangeReadResult)dto.Result)
                {
                    case RangeReadResult.Success:
                        return new InspectionResult(InspectionDecision.Succeed);
                    case RangeReadResult.StreamDeleted:
                        return new InspectionResult(InspectionDecision.NotifyError, new StreamDeletedException());
                    case RangeReadResult.NoStream:
                        return new InspectionResult(InspectionDecision.NotifyError, new StreamDoesNotExistException());
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                return new InspectionResult(InspectionDecision.NotifyError, e);
            }
        }

        public void Complete()
        {
            if (_result != null)
                _source.SetResult(new EventStreamSlice(_stream, _start, _count, _result.Events));
            else
                _source.SetException(new NoResultException());
        }

        public void Fail(Exception exception)
        {
            _source.SetException(exception);
        }

        public override string ToString()
        {
            return string.Format("Stream: {0}, Start: {1}, Count: {2}, ResolveLinkTos: {3}, CorrelationId: {4}", 
                                 _stream, 
                                 _start, 
                                 _count, 
                                 _resolveLinkTos, 
                                 CorrelationId);
        }
    }

    internal class ReadStreamEventsBackwardOperation : IClientOperation
    {
        private readonly TaskCompletionSource<EventStreamSlice> _source;
        private ClientMessages.ReadStreamEventsBackwardCompleted _result;

        private Guid _correlationId;
        private readonly object _corrIdLock = new object();

        private readonly string _stream;
        private readonly int _start;
        private readonly int _count;
        private readonly bool _resolveLinkTos;

        public Guid CorrelationId
        {
            get
            {
                lock (_corrIdLock)
                    return _correlationId;
            }
        }

        public ReadStreamEventsBackwardOperation(TaskCompletionSource<EventStreamSlice> source,
                                                 Guid corrId,
                                                 string stream,
                                                 int start,
                                                 int count,
                                                 bool resolveLinkTos)
        {
            _source = source;

            _correlationId = corrId;
            _stream = stream;
            _start = start;
            _count = count;
            _resolveLinkTos = resolveLinkTos;
        }

        public void SetRetryId(Guid correlationId)
        {
            lock (_corrIdLock)
                _correlationId = correlationId;
        }

        public TcpPackage CreateNetworkPackage()
        {
            lock (_corrIdLock)
            {
                var dto = new ClientMessages.ReadStreamEventsBackward(_stream, _start, _count, _resolveLinkTos);
                return new TcpPackage(TcpCommand.ReadStreamEventsBackward, _correlationId, dto.Serialize());
            }
        }

        public InspectionResult InspectPackage(TcpPackage package)
        {
            try
            {
                if (package.Command != TcpCommand.ReadStreamEventsBackwardCompleted)
                {
                    return new InspectionResult(InspectionDecision.NotifyError,
                        new CommandNotExpectedException(TcpCommand.ReadStreamEventsBackwardCompleted.ToString(),
                                                        package.Command.ToString()));
                }

                var data = package.Data;
                var dto = data.Deserialize<ClientMessages.ReadStreamEventsBackwardCompleted>();
                _result = dto;

                switch ((RangeReadResult)dto.Result)
                {
                    case RangeReadResult.Success:
                        return new InspectionResult(InspectionDecision.Succeed);
                    case RangeReadResult.StreamDeleted:
                        return new InspectionResult(InspectionDecision.NotifyError, new StreamDeletedException());
                    case RangeReadResult.NoStream:
                        return new InspectionResult(InspectionDecision.NotifyError, new StreamDoesNotExistException());
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                return new InspectionResult(InspectionDecision.NotifyError, e);
            }
        }

        public void Complete()
        {
            if (_result != null)
                _source.SetResult(new EventStreamSlice(_stream, _start, _count, _result.Events));
            else
                _source.SetException(new NoResultException());
        }

        public void Fail(Exception exception)
        {
            _source.SetException(exception);
        }

        public override string ToString()
        {
            return string.Format("Stream: {0}, Start: {1}, Count: {2}, ResolveLinkTos: {3}, CorrelationId: {4}", 
                                 _stream,
                                 _start, 
                                 _count, 
                                 _resolveLinkTos, 
                                 CorrelationId);
        }
    }
}