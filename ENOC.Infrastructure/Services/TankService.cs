using ENOC.Application.DTOs.Tank;
using ENOC.Application.Interfaces;
using ENOC.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ENOC.Infrastructure.Services;

public class TankService : ITankService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TankService> _logger;

    public TankService(IUnitOfWork unitOfWork, ILogger<TankService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TankResponse?> GetTankByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tank = await _unitOfWork.Repository<Domain.Entities.Tank>().GetByIdAsync(id, cancellationToken);
        if (tank == null)
        {
            return null;
        }

        var businessUnit = await _unitOfWork.Repository<BusinessUnit>().GetByIdAsync(tank.BusinessUnitId, cancellationToken);
        var files = (await _unitOfWork.Repository<TankFile>().GetAllAsync(cancellationToken))
            .Where(f => f.TankId == tank.Id)
            .ToList();

        var response = new TankResponse
        {
            Id = tank.Id,
            Name = tank.Name,
            TankNumber = tank.TankNumber,
            BusinessUnitId = tank.BusinessUnitId,
            BusinessUnitName = businessUnit?.Name ?? "Unknown",
            Location = tank.Location,
            ERG = tank.ERG
        };

        foreach (var file in files)
        {
            response.Files.Add(new TankFileResponse
            {
                Id = file.Id,
                TankId = file.TankId,
                TankName = tank.Name,
                FileName = file.FileName,
                FileExtension = file.FileExtension,
                FileType = file.FileType,
                FileSize = file.FileSize,
                UploadedAt = file.UploadedAt,
                ContentType = file.ContentType
            });
        }

        return response;
    }

    public async Task<IEnumerable<TankResponse>> GetAllTanksAsync(Guid? businessUnitId = null, CancellationToken cancellationToken = default)
    {
        var tanks = await _unitOfWork.Repository<Domain.Entities.Tank>().GetAllAsync(cancellationToken);

        if (businessUnitId.HasValue)
        {
            tanks = tanks.Where(t => t.BusinessUnitId == businessUnitId.Value);
        }

        // Filter out deleted tanks
        tanks = tanks.Where(t => !t.IsDeleted);

        var responses = new List<TankResponse>();
        foreach (var tank in tanks.OrderBy(t => t.TankNumber))
        {
            var response = await GetTankByIdAsync(tank.Id, cancellationToken);
            if (response != null)
            {
                responses.Add(response);
            }
        }

        return responses;
    }

    public async Task<IEnumerable<TankResponse>> SearchTanksAsync(string query, Guid? businessUnitId = null, CancellationToken cancellationToken = default)
    {
        var tanks = await _unitOfWork.Repository<Domain.Entities.Tank>().GetAllAsync(cancellationToken);

        // Filter out deleted tanks
        tanks = tanks.Where(t => !t.IsDeleted);

        // Apply business unit filter if provided
        if (businessUnitId.HasValue)
        {
            tanks = tanks.Where(t => t.BusinessUnitId == businessUnitId.Value);
        }

        // Search by tank name or tank number
        var searchQuery = query.ToLower().Trim();
        tanks = tanks.Where(t =>
            (t.Name != null && t.Name.ToLower().Contains(searchQuery)) ||
            t.TankNumber.ToString().Contains(searchQuery)
        );

        var responses = new List<TankResponse>();
        foreach (var tank in tanks.OrderBy(t => t.TankNumber))
        {
            var response = await GetTankByIdAsync(tank.Id, cancellationToken);
            if (response != null)
            {
                responses.Add(response);
            }
        }

        _logger.LogInformation("Tank search for '{Query}' returned {Count} results", query, responses.Count);

        return responses;
    }

    public async Task<TankFileResponse?> UploadTankFileAsync(UploadTankFileRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify tank exists
            var tank = await _unitOfWork.Repository<Domain.Entities.Tank>().GetByIdAsync(request.TankId, cancellationToken);
            if (tank == null)
            {
                _logger.LogWarning("Tank {TankId} not found for file upload", request.TankId);
                return null;
            }

            // Extract file extension from filename
            var fileExtension = Path.GetExtension(request.FileName);
            if (string.IsNullOrEmpty(fileExtension))
            {
                fileExtension = DetermineExtensionFromContentType(request.ContentType);
            }

            var tankFile = new TankFile
            {
                Id = Guid.NewGuid(),
                TankId = request.TankId,
                FileName = request.FileName,
                FileExtension = fileExtension,
                FileContent = request.FileContent,
                FileType = request.FileType,
                FileSize = request.FileContent.Length,
                ContentType = request.ContentType,
                UploadedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<TankFile>().AddAsync(tankFile, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("File {FileName} uploaded for tank {TankName} ({TankId})",
                tankFile.FileName, tank.Name, tank.Id);

            return new TankFileResponse
            {
                Id = tankFile.Id,
                TankId = tankFile.TankId,
                TankName = tank.Name,
                FileName = tankFile.FileName,
                FileExtension = tankFile.FileExtension,
                FileType = tankFile.FileType,
                FileSize = tankFile.FileSize,
                UploadedAt = tankFile.UploadedAt,
                ContentType = tankFile.ContentType
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file for tank {TankId}", request.TankId);
            throw;
        }
    }

    public async Task<(byte[] FileContent, string FileName, string ContentType)?> DownloadTankFileAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var file = await _unitOfWork.Repository<TankFile>().GetByIdAsync(fileId, cancellationToken);
        if (file == null)
        {
            _logger.LogWarning("Tank file {FileId} not found for download", fileId);
            return null;
        }

        _logger.LogInformation("File {FileName} downloaded for tank {TankId}", file.FileName, file.TankId);

        return (file.FileContent, file.FileName, file.ContentType);
    }

    public async Task<IEnumerable<TankFileResponse>> GetTankFilesAsync(Guid tankId, CancellationToken cancellationToken = default)
    {
        var tank = await _unitOfWork.Repository<Domain.Entities.Tank>().GetByIdAsync(tankId, cancellationToken);
        if (tank == null)
        {
            return Enumerable.Empty<TankFileResponse>();
        }

        var files = (await _unitOfWork.Repository<TankFile>().GetAllAsync(cancellationToken))
            .Where(f => f.TankId == tankId)
            .OrderByDescending(f => f.UploadedAt)
            .ToList();

        return files.Select(file => new TankFileResponse
        {
            Id = file.Id,
            TankId = file.TankId,
            TankName = tank.Name,
            FileName = file.FileName,
            FileExtension = file.FileExtension,
            FileType = file.FileType,
            FileSize = file.FileSize,
            UploadedAt = file.UploadedAt,
            ContentType = file.ContentType
        }).ToList();
    }

    public async Task<bool> DeleteTankFileAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var file = await _unitOfWork.Repository<TankFile>().GetByIdAsync(fileId, cancellationToken);
        if (file == null)
        {
            return false;
        }

        _unitOfWork.Repository<TankFile>().Remove(file);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tank file {FileName} ({FileId}) deleted", file.FileName, fileId);

        return true;
    }

    private string DetermineExtensionFromContentType(string contentType)
    {
        return contentType.ToLower() switch
        {
            "application/pdf" => ".pdf",
            "image/jpeg" => ".jpg",
            "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/bmp" => ".bmp",
            "image/webp" => ".webp",
            _ => ".bin"
        };
    }
}
