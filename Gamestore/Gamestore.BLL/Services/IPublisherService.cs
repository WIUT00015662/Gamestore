using Gamestore.BLL.DTOs.Publisher;

namespace Gamestore.BLL.Services;

public interface IPublisherService
{
    Task<PublisherResponse> CreatePublisherAsync(CreatePublisherRequest request);

    Task<PublisherResponse> GetPublisherByCompanyNameAsync(string companyName);

    Task<IEnumerable<PublisherResponse>> GetAllPublishersAsync();

    Task<PublisherResponse> GetPublisherByGameKeyAsync(string key);

    Task UpdatePublisherAsync(UpdatePublisherRequest request);

    Task DeletePublisherAsync(Guid id);
}
