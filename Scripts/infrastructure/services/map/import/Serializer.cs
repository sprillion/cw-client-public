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
                    writer.Write(block.Rotation);
                }
            }
        }

        // Десериализация
        public static List<ExportedChunk> Deserialize(byte[] data)
        {
            var result = new List<ExportedChunk>();
            foreach (var chunk in DeserializeLazy(data))
                result.Add(chunk);
            return result;
        }

        // Ленивая десериализация — читает по одному чанку за раз.
        // Позволяет вставлять await UniTask.Yield() между чанками на WebGL.
        // yield return нельзя использовать внутри try/catch, поэтому чтение
        // одного чанка вынесено в отдельный метод ReadNextChunk.
        public static IEnumerable<ExportedChunk> DeserializeLazy(byte[] data)
        {
            var memoryStream = new MemoryStream(data);
            var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
            var reader = new BinaryReader(gzipStream);

            try
            {
                while (true)
                {
                    var chunk = ReadNextChunk(reader);
                    if (chunk == null) yield break;
                    yield return chunk;
                }
            }
            finally
            {
                reader.Dispose();
                gzipStream.Dispose();
                memoryStream.Dispose();
            }
        }

        private static ExportedChunk ReadNextChunk(BinaryReader reader)
        {
            try
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
                        Value = reader.ReadInt32(),
                        Rotation = (short)reader.ReadInt32()
                    });
                }

                return chunk;
            }
            catch (EndOfStreamException)
            {
                return null;
            }
        }
    }
}