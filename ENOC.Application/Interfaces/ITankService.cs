using ENOC.Application.DTOs.Tank;

namespace ENOC.Application.Interfaces;

public interface ITankService
{
    Task<TankResponse?> GetTankByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TankResponse>> GetAllTanksAsync(Guid? businessUnitId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<TankResponse>> SearchTanksAsync(string query, Guid? businessUnitId = null, CancellationToken cancellationToken = default);
    Task<TankFileResponse?> UploadTankFileAsync(UploadTankFileRequest request, CancellationToken cancellationToken = default);
    Task<(byte[] FileContent, string FileName, string ContentType)?> DownloadTankFileAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TankFileResponse>> GetTankFilesAsync(Guid tankId, CancellationToken cancellationToken = default);
    Task<bool> DeleteTankFileAsync(Guid fileId, CancellationToken cancellationToken = default);
}
