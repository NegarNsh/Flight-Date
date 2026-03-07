using System; // We need this line at the top to use DateTime!
using Unity; 


[System.Serializable]
public class Flight
{
    public string flightID;
    public int level;
    public string origin;
    public string destination;
    public float basePrice;
   

    public int departureDay;
    public string startTimeText;
    public int durationHours;

    // --- The Calculated Data ---
    public DateTime exactDeparture;
    public DateTime exactArrival;

}
