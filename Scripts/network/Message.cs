using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace network
{
    public class Message : IDisposable
    {
        private const int MaxLength = 10 * 1024 * 1024;
        private const int StringLengthThreshold = 32700;
        private const short StringBigMarker = -2;
        
        private readonly MemoryStream _stream;
        private readonly BinaryWriter _writer;
        private readonly BinaryReader _reader;
        
        private readonly ushort _actionId;

        public ServerToClientId ServerToClientId => (ServerToClientId)_actionId;
        public ClientToServerId ClientToServerId => (ClientToServerId)_actionId;

        public Message(Enum action)
        {
            _stream = new MemoryStream();
            _writer = new BinaryWriter(_stream);
            _actionId = Convert.ToUInt16(action);
            WriteActionId();
        }

        public Message(byte[] data)
        {
            _stream = new MemoryStream(data);
            _reader = new BinaryReader(_stream);
            _actionId = ReadActionId();
        }

        private void WriteActionId()
        {
            _writer.Write(BitConverter.GetBytes(_actionId).ReverseIfLittleEndian());
        }

        private ushort ReadActionId()
        {
            var bytes = _reader.ReadBytes(2).ReverseIfLittleEndian();
            return BitConverter.ToUInt16(bytes, 0);
        }

        #region Add Methods
        public Message AddBool(bool value)
        {
            _writer.Write(value);
            return this;
        }

        public Message AddByte(byte value)
        {
            _writer.Write(value);
            return this;
        }

        public Message AddShort(short value)
        {
            _writer.Write(value.ToNetworkBytes());
            return this;
        }

        public Message AddUShort(ushort value)
        {
            _writer.Write(value.ToNetworkBytes());
            return this;
        }

        public Message AddInt(int value)
        {
            _writer.Write(value.ToNetworkBytes());
            return this;
        }

        public Message AddUInt(uint value)
        {
            _writer.Write(value.ToNetworkBytes());
            return this;
        }

        public Message AddLong(long value)
        {
            _writer.Write(value.ToNetworkBytes());
            return this;
        }

        public Message AddFloat(float value)
        {
            _writer.Write(value.ToNetworkBytes());
            return this;
        }

        public Message AddDouble(double value)
        {
            _writer.Write(value.ToNetworkBytes());
            return this;
        }

        public Message AddString(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            if (bytes.Length > MaxLength)
                throw new ArgumentException($"String exceeds max length ({MaxLength} bytes)");

            if (bytes.Length > StringLengthThreshold)
            {
                AddShort(StringBigMarker);
                AddInt(bytes.Length);
            }
            else
            {
                AddShort((short)bytes.Length);
            }

            _writer.Write(bytes);
            return this;
        }

        public Message AddVector2(Vector2 value)
        {
            AddFloat(value.x);
            AddFloat(value.y);
            return this;
        }

        public Message AddVector3(Vector3 value)
        {
            AddFloat(value.x);
            AddFloat(value.y);
            AddFloat(value.z);
            return this;
        }

        public Message AddQuaternion(Quaternion value)
        {
            AddFloat(value.x);
            AddFloat(value.y);
            AddFloat(value.z);
            AddFloat(value.w);
            return this;
        }
        #endregion

        #region Read Methods
        public bool GetBool() => _reader.ReadBoolean();
        public byte GetByte() => _reader.ReadByte();
        public short GetShort() => _reader.ReadInt16().FromNetworkBytes();
        public ushort GetUShort() => _reader.ReadUInt16().FromNetworkBytes();
        public int GetInt() => _reader.ReadInt32().FromNetworkBytes();
        public uint GetUInt() => _reader.ReadUInt32().FromNetworkBytes();
        public long GetLong() => _reader.ReadInt64().FromNetworkBytes();
        public float GetFloat() => _reader.ReadSingle().FromNetworkBytes();
        public double GetDouble() => _reader.ReadDouble().FromNetworkBytes();

        public string GetString()
        {
            var length = GetShort();
            if (length == StringBigMarker)
            {
                length = (short)GetInt();
            }

            if (length <= 0)
                throw new InvalidDataException($"Invalid string length: {length}");

            return Encoding.UTF8.GetString(_reader.ReadBytes(length));
        }

        public Vector2 GetVector2() => new Vector2(GetFloat(), GetFloat());
        public Vector3 GetVector3() => new Vector3(GetFloat(), GetFloat(), GetFloat());
        public Quaternion GetQuaternion() => new Quaternion(GetFloat(), GetFloat(), GetFloat(), GetFloat());
        #endregion

        public byte[] ToArray()
        {
            _writer.Flush();
            return _stream.ToArray();
        }

        public void Dispose()
        {
            _writer?.Dispose();
            _reader?.Dispose();
            _stream?.Dispose();
        }
    }

    // Расширения для работы с endianness
    public static class ByteExtensions
    {
        public static byte[] ReverseIfLittleEndian(this byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        public static byte[] ToNetworkBytes(this short value) => 
            BitConverter.GetBytes(value).ReverseIfLittleEndian();

        public static short FromNetworkBytes(this short value) => 
            BitConverter.ToInt16(BitConverter.GetBytes(value).ReverseIfLittleEndian(), 0);
        
        public static byte[] ToNetworkBytes(this ushort value) => 
            BitConverter.GetBytes(value).ReverseIfLittleEndian();
        public static ushort FromNetworkBytes(this ushort value) => 
            BitConverter.ToUInt16(BitConverter.GetBytes(value).ReverseIfLittleEndian(), 0);
        
        public static byte[] ToNetworkBytes(this int value) => 
            BitConverter.GetBytes(value).ReverseIfLittleEndian();
        public static int FromNetworkBytes(this int value) => 
            BitConverter.ToInt32(BitConverter.GetBytes(value).ReverseIfLittleEndian(), 0);
        
        public static byte[] ToNetworkBytes(this uint value) => 
            BitConverter.GetBytes(value).ReverseIfLittleEndian();
        public static uint FromNetworkBytes(this uint value) => 
            BitConverter.ToUInt32(BitConverter.GetBytes(value).ReverseIfLittleEndian(), 0);
        
        public static byte[] ToNetworkBytes(this long value) => 
            BitConverter.GetBytes(value).ReverseIfLittleEndian();
        public static long FromNetworkBytes(this long value) => 
            BitConverter.ToInt64(BitConverter.GetBytes(value).ReverseIfLittleEndian(), 0);
        
        public static byte[] ToNetworkBytes(this float value) => 
            BitConverter.GetBytes(value).ReverseIfLittleEndian();
        public static float FromNetworkBytes(this float value) => 
            BitConverter.ToSingle(BitConverter.GetBytes(value).ReverseIfLittleEndian(), 0);

        public static byte[] ToNetworkBytes(this double value) => 
            BitConverter.GetBytes(value).ReverseIfLittleEndian();
        public static double FromNetworkBytes(this double value) => 
            BitConverter.ToDouble(BitConverter.GetBytes(value).ReverseIfLittleEndian(), 0);
        
    }
}