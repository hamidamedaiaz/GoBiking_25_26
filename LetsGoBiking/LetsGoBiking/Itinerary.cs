using System.Collections.Generic;
using System.Runtime.Serialization;

[DataContract]
public class Itinerary
{
    [DataMember]
    public bool UseBike { get; set; }

    [DataMember]
    public Station DepartureStation { get; set; }

    [DataMember]
    public Station ArrivalStation { get; set; }

    [DataMember]
    public double TotalDuration { get; set; } // en secondes

    [DataMember]
    public List<Step> Steps { get; set; }
}

[DataContract]
public class Step
{
    [DataMember]
    public string Instruction { get; set; } // "Marcher jusqu'à la station X"

    [DataMember]
    public double Duration { get; set; }

    [DataMember]
    public string Mode { get; set; } // "walk" ou "bike"
}