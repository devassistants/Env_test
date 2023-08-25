using System.Text.Json.Serialization;

namespace MyTests
{
    public class MyStructure
    {

        [JsonPropertyName("my_name")]
        public string MyName { get; set; }

        [JsonPropertyName("my_value")]
        public int? MyValue { get; set; }

    }
}
