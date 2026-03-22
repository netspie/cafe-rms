namespace CafeRMS.Api;

public class Tag
{
    public int Id { get; init; }
    public string Name { get; set; }
    public string ImageUrl { get; set; }

    public Tag(string name, string imageUrl)
    {
        Name = name;
        ImageUrl = imageUrl;
    }
}