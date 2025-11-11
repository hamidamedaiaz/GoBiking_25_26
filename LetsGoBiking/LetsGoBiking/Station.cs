using System.Runtime.Serialization;

[DataContract]
public class Station
{
    [DataMember(Name = "number")]
    public int Number { get; set; }

    [DataMember(Name = "name")]
    public string Name { get; set; }

    [DataMember(Name = "address")]
    public string Address { get; set; }

    [DataMember(Name = "position")]
    public Position Position { get; set; }

    [DataMember(Name = "available_bikes")]
    public int AvailableBikes { get; set; }

    [DataMember(Name = "available_bike_stands")]
    public int AvailableBikeStands { get; set; }
}

[DataContract]
public class Position
{
    [DataMember(Name = "lat")]
    public double Lat { get; set; }

    [DataMember(Name = "lng")]
    public double Lng { get; set; }
}