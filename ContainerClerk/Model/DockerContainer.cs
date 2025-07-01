namespace ContainerClerk.Model;

public class DockerContainer
{
    public string ID { get; set; }
    public string Image { get; set; }
    public string Command { get; set; }
    public string CreatedAt { get; set; }
    public string Status { get; set; }
    public string Ports { get; set; }
    public string Names { get; set; }
    public string State { get; set; }

    public bool GetState()
    {
        return State == "running";
    }
}
