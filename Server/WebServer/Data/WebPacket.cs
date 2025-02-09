public class TestPacketReq
{
    // client -> server
    public string userId { get; set; }
    public string token { get; set; }

}

public class TestPacketRes
{
    // server -> client
    public bool success { get; set; }
}
