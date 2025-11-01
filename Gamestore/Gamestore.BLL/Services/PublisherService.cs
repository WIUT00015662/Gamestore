using Gamestore.BLL.DTOs.Publisher;
using Gamestore.BLL.Mapping;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Services;

public class PublisherService(IUnitOfWork unitOfWork, ILogger<PublisherService> logger) : IPublisherService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<PublisherService> _logger = logger;

    public async Task<PublisherResponse> CreatePublisherAsync(CreatePublisherRequest request)
    {
        _logger.LogInformation("Creating publisher: {CompanyName}", request.Publisher.CompanyName);
        var existing = await _unitOfWork.Publishers.GetByCompanyNameAsync(request.Publisher.CompanyName);
        if (existing is not null)
        {
            throw new EntityAlreadyExistsException(nameof(Publisher), nameof(Publisher.CompanyName), request.Publisher.CompanyName);
        }

        var publisher = new Publisher
        {
            Id = Guid.NewGuid(),
            CompanyName = request.Publisher.CompanyName,
            HomePage = request.Publisher.HomePage,
            Description = request.Publisher.Description,
        };

        await _unitOfWork.Publishers.AddAsync(publisher);
        await _unitOfWork.SaveChangesAsync();

        return publisher.ToResponse();
    }

    public async Task<PublisherResponse> GetPublisherByCompanyNameAsync(string companyName)
    {
        var publisher = await _unitOfWork.Publishers.GetByCompanyNameAsync(companyName)
            ?? throw new EntityNotFoundException(nameof(Publisher), companyName);

        return publisher.ToResponse();
    }

    public async Task<IEnumerable<PublisherResponse>> GetAllPublishersAsync()
    {
        var publishers = await _unitOfWork.Publishers.GetAllAsync();
        return publishers.ToResponse();
    }

    public async Task<PublisherResponse> GetPublisherByGameKeyAsync(string key)
    {
        var publisher = await _unitOfWork.Publishers.GetByGameKeyAsync(key)
            ?? throw new EntityNotFoundException(nameof(Publisher), key);

        return publisher.ToResponse();
    }

    public async Task UpdatePublisherAsync(UpdatePublisherRequest request)
    {
        _logger.LogInformation("Updating publisher with id {PublisherId}", request.Publisher.Id);
        var publisher = await _unitOfWork.Publishers.GetByIdAsync(request.Publisher.Id)
            ?? throw new EntityNotFoundException(nameof(Publisher), request.Publisher.Id);

        var existingByName = await _unitOfWork.Publishers.GetByCompanyNameAsync(request.Publisher.CompanyName);
        if (existingByName is not null && existingByName.Id != publisher.Id)
        {
            throw new EntityAlreadyExistsException(nameof(Publisher), nameof(Publisher.CompanyName), request.Publisher.CompanyName);
        }

        publisher.CompanyName = request.Publisher.CompanyName;
        publisher.HomePage = request.Publisher.HomePage;
        publisher.Description = request.Publisher.Description;

        _unitOfWork.Publishers.Update(publisher);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeletePublisherAsync(Guid id)
    {
        _logger.LogInformation("Deleting publisher with id {PublisherId}", id);
        var publisher = await _unitOfWork.Publishers.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Publisher), id);

        _unitOfWork.Publishers.Delete(publisher);
        await _unitOfWork.SaveChangesAsync();
    }
}
