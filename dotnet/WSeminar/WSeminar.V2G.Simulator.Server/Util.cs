using Deedle;

namespace WSeminar.V2G.Simulator.Server;

public class BoxPair
{
    public DateTimeOffset Key;
    public double? Value { get; set; }

    public BoxPair(KeyValuePair<DateTimeOffset, double?> pair)
    {
        Key = pair.Key;
        Value = pair.Value;
    }
    
    public BoxPair(KeyValuePair<DateTimeOffset,OptionalValue< double?>> pair)
    {
        Key = pair.Key;
        Value = pair.Value.OrDefault(null);
    }
}