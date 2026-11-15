using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ZoneConnection
{
    public TeleportZone from;
    public TeleportZone to;
    public bool bidirectional = true;
}

[CreateAssetMenu(fileName = "WorldMap", menuName = "VR Historia/World Map")]
public class WorldMap : ScriptableObject
{
    public List<TeleportZone> allZones = new List<TeleportZone>();
    public List<ZoneConnection> connections = new List<ZoneConnection>();
    public TeleportZone startingZone;

    public List<TeleportZone> GetUnlockedZones()
    {
        var result = new List<TeleportZone>();
        foreach (var zone in allZones)
        {
            if (zone != null && zone.isUnlocked)
                result.Add(zone);
        }
        return result;
    }

    public List<TeleportZone> GetConnectedZones(TeleportZone zone)
    {
        var result = new List<TeleportZone>();
        foreach (var conn in connections)
        {
            if (conn.from == zone && conn.to != null)
                result.Add(conn.to);
            else if (conn.bidirectional && conn.to == zone && conn.from != null)
                result.Add(conn.from);
        }
        return result;
    }
}
