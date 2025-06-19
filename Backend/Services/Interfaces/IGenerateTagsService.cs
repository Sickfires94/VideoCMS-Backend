namespace Backend.Services.Interfaces
{
    public interface IGenerateTagsService
    {
        public Task<List<string>> GenerateTags(string title, string description);
    }
}
