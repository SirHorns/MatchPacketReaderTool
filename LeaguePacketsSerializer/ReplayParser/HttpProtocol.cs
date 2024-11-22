using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LeaguePacketsSerializer.ReplayParser
{
    public abstract class HttpProtocolHandler
    {
        private HttpState _httpState = HttpState.Done;
        private readonly List<byte> _buffer = new();
        private long _bufferExpectedLength;
        
        private Regex RE_CONTENT_LEN = new("Content-Length: ([0-9]+)", RegexOptions.IgnoreCase);

        private byte[] HTTP_END = { 0x0D, 0x0A, 0x0D, 0x0A };
        
        
        public void Read(byte[] data, float time)
        {
            switch(_httpState)
            {
                case HttpState.GetBinary:
                    HandleGetBinary(data, time);
                    break;
                case HttpState.GetText:
                    HandleGetText(data, time);
                    break;
                case HttpState.Done:
                    HandleDone(data, time);
                    break;
                case HttpState.ContinueBinary:
                    HandleContinueBinary(data, time);
                    break;
                case HttpState.ContinueText:
                    HandleContinueText(data, time);
                    break;
            }
        }

        private void HandleGetBinary(byte[] data, float time)
        {
            HandleBinaryPacket(data, time);
            _httpState = HttpState.Done;
        }

        private void HandleGetText(byte[] data, float time)
        {
            HandleTextPacket(Encoding.UTF8.GetString(data), time);
            _httpState = HttpState.Done;
        }

        private void HandleContinueBinary(byte[] data, float time)
        {
            _buffer.AddRange(data);
            if(_buffer.Count > _bufferExpectedLength)
            {
                throw new IOException("Buffer overrun!");
            }
            else if(_buffer.Count == _bufferExpectedLength)
            {
                HandleBinaryPacket(_buffer.ToArray(), time);
                _httpState = HttpState.Done;
                _buffer.Clear();
                _bufferExpectedLength = 0;
            }
        }

        private void HandleContinueText(byte[] data, float time)
        {
            _buffer.AddRange(data);
            if (_buffer.Count > _bufferExpectedLength)
            {
                throw new IOException("Buffer overrun!");
            }
            
            if (_buffer.Count == _bufferExpectedLength)
            {
                HandleTextPacket(Encoding.UTF8.GetString(_buffer.ToArray()), time);
                _httpState = HttpState.Done;
                _buffer.Clear();
                _bufferExpectedLength = 0;
            }
        }
        
        
        private void HandleDone(byte[] data, float time)
        {
            var req = Encoding.UTF8.GetString(data).Split(' ');

            var httpReq = req[0];
            var replayReq = req[1];
            
            switch (httpReq)
            {
                case "HTTP":
                    HandleHttp(data, time);
                    break;
                case "GET":
                    Get(replayReq);
                    break;
                case "POST":
                    Console.WriteLine(replayReq);
                    _httpState = HttpState.GetText;
                    break;
                case "HEAD":
                case "OPTIONS":
                    break;
                default:
                    Console.WriteLine(httpReq);
                    Console.WriteLine("Bad Chunk?");
                    break;
            }
        }

        private void Get(string request)
        {
            // /observer-mode/rest/consumer/<api-call>/
            if (request.Equals("\r\n"))
            {
                _httpState = HttpState.Done;
                return;
            }
            var api = request.Split("/");
            switch (api[4])
            {
                case "version":
                case "getGameMetaData":
                case "getLastChunkInfo":
                case "getKeyFrame":
                    _httpState = HttpState.GetText;
                    break;
                case "getGameDataChunk":
                    _httpState = HttpState.GetBinary;
                    break;
                default:
                    Console.WriteLine(request);
                    break;
            }
        }
        
        private void HandleHttp(byte[] data, float time)
        {
            using var stream = new MemoryStream(data);
            int index = 0;
            int matchCount = 0;
            for(; index < data.Length && matchCount != 4; index ++)
            {
                if(data[index] == HTTP_END[matchCount])
                {
                    matchCount++;
                }
                else
                {
                    matchCount = 0;
                }
            }
            if(matchCount != 4)
            {
                throw new IOException("Failed to find http end in stream!");
            }

            using var binary = new BinaryReader(stream, Encoding.UTF8, true);
            var http = Encoding.UTF8.GetString(binary.ReadExactBytes(index));
            var contentLengthMatch = RE_CONTENT_LEN.Match(http);
            if(!contentLengthMatch.Success)
            {
                return;
            }
            var contentLength = long.Parse(contentLengthMatch.Groups[1].Value);
            var content = binary.ReadExactBytes((int)binary.BytesLeft());
            if (!http.Contains("application/octet-stream"))
            {
                if (content.Length < contentLength)
                {
                    _buffer.AddRange(content);
                    _bufferExpectedLength = contentLength;
                    _httpState = HttpState.ContinueText;
                }
                else
                {
                    HandleTextPacket(Encoding.UTF8.GetString(content), time);
                }
            }
            else
            {
                if (content.Length < contentLength)
                {
                    _buffer.AddRange(content);
                    _bufferExpectedLength = contentLength;
                    _httpState = HttpState.ContinueBinary;
                }
                else
                { 
                    HandleBinaryPacket(content, time);
                }
            }
        }
        
        
        
        public virtual void HandleTextPacket(string data, float time) { }

        public virtual void HandleBinaryPacket(byte[] data, float time) { }
    }
}
