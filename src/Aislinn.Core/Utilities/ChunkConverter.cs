using System;
using System.Reflection;
using System.ComponentModel;
using System.Globalization;
using Aislinn.Core.Models;
using Aislinn.ChunkStorage;

namespace Aislinn.Core.Converters
{
    // TypeConverter to allow implicit conversion from Guid to Chunk in property setters
    public class ChunkTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(Guid) || sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
                return null;

            if (value is Guid guidValue)
                return guidValue.ToChunk();

            if (value is string stringValue && Guid.TryParse(stringValue, out Guid guid))
                return guid.ToChunk();

            return base.ConvertFrom(context, culture, value);
        }
    }

    // Attribute to apply to Chunk properties for auto-conversion
    [AttributeUsage(AttributeTargets.Property)]
    public class ChunkReferenceAttribute : Attribute
    {
        public string CollectionId { get; set; }

        public ChunkReferenceAttribute(string collectionId = null)
        {
            CollectionId = collectionId;
        }
    }

    // Extension for ModelSlot to work with chunk references
    public static class ModelSlotExtensions
    {
        public static Chunk GetChunk(this ModelSlot slot)
        {
            if (slot == null || slot.Value == null)
                return null;

            // Handle direct Chunk values
            if (slot.Value is Chunk chunk)
                return chunk;

            // Handle Guid values
            if (slot.Value is Guid guid)
                return guid.ToChunk();

            // Handle string values that can be parsed as Guid
            if (slot.Value is string guidString && Guid.TryParse(guidString, out Guid parsedGuid))
                return parsedGuid.ToChunk();

            return null;
        }

        public static async Task<Chunk> GetChunkAsync(this ModelSlot slot, string collectionId = null)
        {
            if (slot == null || slot.Value == null)
                return null;

            // Handle direct Chunk values
            if (slot.Value is Chunk chunk)
                return chunk;

            // Handle Guid values
            if (slot.Value is Guid guid)
                return await guid.ToChunkAsync(collectionId);

            // Handle string values that can be parsed as Guid
            if (slot.Value is string guidString && Guid.TryParse(guidString, out Guid parsedGuid))
                return await parsedGuid.ToChunkAsync(collectionId);

            return null;
        }
    }
}