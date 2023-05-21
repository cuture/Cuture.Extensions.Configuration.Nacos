using System.Text.Json;

namespace GenericHostSample;

public class SampleOptions
{
    public bool[] BoolArrayProperty { get; set; }

    public bool BoolProperty { get; set; }

    public int[] IntArrayProperty { get; set; }

    public int IntProperty { get; set; }

    public PropertyObject[] ObjectArrayProperty { get; set; }

    public PropertyObject ObjectProperty { get; set; }

    public string[] StringArrayProperty { get; set; }

    public string StringProperty { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true });
    }

    public class PropertyObject
    {
        public string Value { get; set; }
    }
}
