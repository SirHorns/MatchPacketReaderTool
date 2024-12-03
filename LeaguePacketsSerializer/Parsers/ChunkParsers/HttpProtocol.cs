using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using LeaguePacketsSerializer.Enums;

namespace LeaguePacketsSerializer.Parsers.ChunkParsers;

public abstract class HttpProtocolHandler
    {
        private HttpState _httpState = HttpState.Done;
        public RequestTypes CurrentRequest { get; private set; } = RequestTypes.NONE;

        private readonly List<byte> _buffer = new();
        private long _bufferExpectedLength;
        
        private Regex RE_CONTENT_LEN = new("Content-Length: ([0-9]+)", RegexOptions.IgnoreCase);

        private byte[] HTTP_END = { 0x0D, 0x0A, 0x0D, 0x0A };


        public List<Section> Sections { get; } = new();
        protected Section CurrentSection { get; private set; }

        
        
        protected void ReadSegment(DataSegment segment)
        {
            var data = segment.Data; 
            var time = segment.Time;
            
            switch(_httpState)
            {
                case HttpState.GetBinary:
                    HandleGetBinary(data);
                    break;
                case HttpState.GetText:
                    HandleGetText(data);
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
                default:
                    Console.WriteLine("Skipped segment");
                    break;
            }
        }

        
        
        private void HandleDone(byte[] data, float time)
        {
            CurrentSection = new Section();
            
            var req = Encoding.UTF8.GetString(data).Split(' ');

            var httpReq = req[0];
            var replayReq = req[1];

            CurrentSection.Http = replayReq;
            CurrentSection.Data = data;
            CurrentSection.Time = time;
            
            switch (httpReq)
            {
                case "HTTP":
                    Http(data);
                    break;
                case "GET":
                    Get(replayReq);
                    break;
                case "POST":
                    Post(replayReq);
                    break;
                case "HEAD":
                    Head();
                    break;
                case "OPTIONS":
                    Options();
                    break;
                default:
                    Console.WriteLine(httpReq);
                    Console.WriteLine("[BAD_REQ]");
                    break;
            }
        }

        private void HandleGetText(byte[] data)
        {
            HandleTextPacket(data);
            SetHttpState(HttpState.Done);
            Sections.Add(CurrentSection);
        }
        
        private void HandleGetBinary(byte[] data)
        {
            HandleBinaryPacket(data);
            SetHttpState(HttpState.Done);
            Sections.Add(CurrentSection);
        }
        
        private void Get(string request)
        {
            // /observer-mode/rest/consumer/<api-call>/
            if (request.Equals("\r\n"))
            {
                SetHttpState(HttpState.Done);
                Sections.Add(CurrentSection);
                return;
            }
            var api = request.Split("/");
            switch (api[4])
            {
                case "version":
                    SetCurrentRequest(RequestTypes.VERSION);
                    SetHttpState(HttpState.GetText);
                    CurrentSection = new VersionSection()
                    {
                        
                    }.Copy(CurrentSection, type: RequestTypes.VERSION);
                    break;
                case "getGameMetaData":
                    SetCurrentRequest(RequestTypes.GAME_META_DATA);
                    SetHttpState(HttpState.GetText);
                    CurrentSection = new MetaDataSection()
                    {
                        MatchId = int.Parse(CurrentSection.Http.Split("/")[^2])
                    }.Copy(CurrentSection, type: RequestTypes.GAME_META_DATA);
                    break;
                case "getLastChunkInfo":
                    SetCurrentRequest(RequestTypes.LAST_CHUNK_INFO);
                    SetHttpState(HttpState.GetText);
                    CurrentSection = new LastChunkInfoSection()
                    {
                        Unknown = int.Parse(CurrentSection.Http.Split("/")[^2])
                    }.Copy(CurrentSection, type: RequestTypes.LAST_CHUNK_INFO);
                    break;
                case "getKeyFrame":
                    SetCurrentRequest(RequestTypes.KEY_FRAME);
                    SetHttpState(HttpState.GetText);
                    CurrentSection = new KeyFrameSection()
                    {
                        ID = int.Parse(CurrentSection.Http.Split("/")[^2])
                    }.Copy(CurrentSection, type: RequestTypes.KEY_FRAME);
                    break;
                case "getGameDataChunk":
                    SetCurrentRequest(RequestTypes.GAME_DATA_CHUNK);
                    SetHttpState(HttpState.GetBinary);
                    var id = int.Parse(CurrentSection.Http.Split("/")[^2]);
                    CurrentSection = new GameDataSection
                    {
                        ID = id,
                        Chunk = new Chunk()
                        {
                            ID = id
                        }
                    }.Copy(CurrentSection, type: RequestTypes.GAME_DATA_CHUNK);
                    break;
                default:
                    CurrentSection.Type = RequestTypes.NONE;
                    Console.WriteLine(request);
                    break;
            }
        }

        private void Post(string request)
        {
            Console.WriteLine(request);
            SetHttpState(HttpState.GetText);
        }
        
        private void SetHttpState(HttpState state)
        {
            _httpState = state;
        }

        private void SetCurrentRequest(RequestTypes type)
        {
            CurrentRequest = type;
        }
        
        //
        
        protected virtual void HandleTextPacket(byte[] data) { }

        protected virtual void HandleBinaryPacket(byte[] data) { }
        
        // Haven't seen these triggered yet
        
        private void Http(byte[] data)
        {
            using var stream = new MemoryStream(data);
            var index = 0;
            var matchCount = 0;
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
                    SetHttpState(HttpState.ContinueText);
                }
                else
                {
                    HandleTextPacket(content);
                }
            }
            else
            {
                if (content.Length < contentLength)
                {
                    _buffer.AddRange(content);
                    _bufferExpectedLength = contentLength;
                    SetHttpState(HttpState.ContinueBinary);
                }
                else
                { 
                    HandleBinaryPacket(content);
                }
            }
        }
        
        private void Head(){}
        
        private void Options(){}
        
        private void HandleContinueBinary(byte[] data, float time)
        {
            _buffer.AddRange(data);
            if(_buffer.Count > _bufferExpectedLength)
            {
                throw new IOException("Buffer overrun!");
            }

            if (_buffer.Count != _bufferExpectedLength)
            {
                return;
            }
            
            HandleBinaryPacket(_buffer.ToArray());
            SetHttpState(HttpState.Done);
            Sections.Add(CurrentSection);
            _buffer.Clear();
            _bufferExpectedLength = 0;
        }
        
        private void HandleContinueText(byte[] data, float time)
        {
            _buffer.AddRange(data);
            if (_buffer.Count > _bufferExpectedLength)
            {
                throw new IOException("Buffer overrun!");
            }

            if (_buffer.Count != _bufferExpectedLength)
            {
                return;
            }
            
            HandleTextPacket(_buffer.ToArray());
            SetHttpState(HttpState.Done);
            Sections.Add(CurrentSection);
            _buffer.Clear();
            _bufferExpectedLength = 0;
        }
    }