namespace infrastructure.services.map.chunk
{
    public interface IChunkFactory
    {
        ChunkRenderer GetChunk();
        WaterRenderer GetWater();
    }
}