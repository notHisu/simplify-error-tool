using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorTool.Interfaces
{
    public interface IParcelService
    {
        Task<string?> GetParcelContent(long parcelId, string userName, string connectionName);
        Task<string?> ConfirmParcel(long parcelId, string userName, string connectionName);
        Task<string?> ProcessParcel(long parcelId, string mailBoxId);
    }
}
