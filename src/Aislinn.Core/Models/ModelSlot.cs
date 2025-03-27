namespace Aislinn.Core.Models
{
    public class ModelSlot
    {
        //C# type for casting the value
        //we may make this Type later, but string is easier to deal with for now
        public string SlotType { get { return Value?.GetType()?.FullName ?? "System.Null"; } }

        //value to be cast for comparison
        public object Value { get; set; } = null;

        //name so we have a key to reference in an action or rule
        public string Name { get; set; } = null;

        // Method to compare attribute values
        public ModelSlot() { }
    }
}