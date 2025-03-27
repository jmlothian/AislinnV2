using System;
using System.Threading.Tasks;
using Aislinn.Core.Models;
using Aislinn.ChunkStorage;

namespace Aislinn.Core.Extensions
{
    public static class ChunkExtensions
    {
        // Add extension methods to Chunk for getting slot values as chunks
        public static Chunk GetChunkFromSlot(this Chunk chunk, string slotName)
        {
            if (chunk == null || string.IsNullOrEmpty(slotName))
                return null;

            if (!chunk.Slots.TryGetValue(slotName, out var slot) || slot == null)
                return null;

            // Handle direct Chunk values
            if (slot.Value is Chunk chunkValue)
                return chunkValue;

            // Handle Guid values
            if (slot.Value is Guid guidValue)
                return guidValue.ToChunk();

            // Handle string values that can be parsed as Guid
            if (slot.Value is string guidString && Guid.TryParse(guidString, out Guid parsedGuid))
                return parsedGuid.ToChunk();

            return null;
        }

        public static async Task<Chunk> GetChunkFromSlotAsync(this Chunk chunk, string slotName, string collectionId = null)
        {
            if (chunk == null || string.IsNullOrEmpty(slotName))
                return null;

            if (!chunk.Slots.TryGetValue(slotName, out var slot) || slot == null)
                return null;

            // Handle direct Chunk values
            if (slot.Value is Chunk chunkValue)
                return chunkValue;

            // Handle Guid values
            if (slot.Value is Guid guidValue)
                return await guidValue.ToChunkAsync(collectionId);

            // Handle string values that can be parsed as Guid
            if (slot.Value is string guidString && Guid.TryParse(guidString, out Guid parsedGuid))
                return await parsedGuid.ToChunkAsync(collectionId);

            return null;
        }

        // Helper to set a chunk reference in a slot
        public static void SetChunkReference(this Chunk chunk, string slotName, Chunk referencedChunk)
        {
            if (chunk == null || string.IsNullOrEmpty(slotName) || referencedChunk == null)
                return;

            chunk.Slots[slotName] = new ModelSlot
            {
                Name = slotName,
                Value = referencedChunk.ID // Store the GUID, not the chunk itself
            };
        }
    }
}