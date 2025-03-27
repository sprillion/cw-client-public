using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace infrastructure.services.map.import
{
    public static class Serializer
    {
        public static void Serialize(List<ExportedChunk> data, string filePath)
        {
            using var fileStream = System.IO.File.Create(filePath);
            using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
            using var writer = new BinaryWriter(gzipStream);

            foreach (var chunk in data)
            {
                // Запись основных значений
                writer.Write(chunk.Position.X);
                writer.Write(chunk.Position.Y);


                // Запись вложенных объектов
                writer.Write(chunk.Blocks.Count);
                foreach (var block in chunk.Blocks)
                {
                    writer.Write((byte)block.BlockType);
                    writer.Write(block.Position.X);
                    writer.Write(block.Position.Y);
                    writer.Write(block.Position.Z);
                    writer.Write(block.Value);
                }
            }
        }

        // Десериализация
        public static List<ExportedChunk> Deserialize(byte[] data)
        {
            using var memoryStream = new MemoryStream(data);
            using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
            using var reader = new BinaryReader(gzipStream);
            var result = new List<ExportedChunk>();

            try
            {
                while (true)
                {
                    var chunk = new ExportedChunk()
                    {
                        Position = new Vector2Short(reader.ReadInt16(), reader.ReadInt16()),
                        Blocks = new List<Block>()
                    };

                    var subCount = reader.ReadInt32();

                    for (int i = 0; i < subCount; i++)
                    {
                        chunk.Blocks.Add(new Block()
                        {
                            BlockType = (BlockType)reader.ReadByte(),
                            Position = new Vector3Short(reader.ReadInt16(), reader.ReadInt16(),
                                reader.ReadInt16()),
                            Value = reader.ReadInt32()
                        });
                    }

                    result.Add(chunk);
                }
            }
            catch (EndOfStreamException)
            {
                return result;
            }
        }
    }
}