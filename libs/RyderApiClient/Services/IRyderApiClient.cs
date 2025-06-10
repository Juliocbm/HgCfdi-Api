using Ryder.Api.Client.Models.Requests;
using Ryder.Api.Client.Models.Requests.Ryder.Api.Client.Models.Requests;
using Ryder.Api.Client.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ryder.Api.Client.Services
{
    public interface IRyderApiClient
    {
        Task<BaseResponse> GetDatosCartaPorteAsync(GetDatosCartaPorteRequest request, CancellationToken ct = default);
        Task<BaseResponse> GetCartaPorteAsync(GetCartaPorteRequest request, CancellationToken ct = default);
        Task<BaseResponse> GetViajesAsync(GetViajesRequest request, CancellationToken ct = default);
        Task<BaseResponse> UploadIngresoAsync(UploadIngresoRequest request, CancellationToken ct = default);
    }
}
