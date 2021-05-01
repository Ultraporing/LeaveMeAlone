
public class Rootobject
{
    public object[] custom_ips { get; set; }
    public object[] blacklist { get; set; }
    public object[] config { get; set; }
    public Friend[] friends { get; set; }
    public string token { get; set; }
}

public class Friend
{
    public string name { get; set; }
    public string ip { get; set; }
    public bool enabled { get; set; }
}
