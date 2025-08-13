using DemoCauTruc.Controllers.C0302;
using DemoCauTruc.Models.M0302;
using static DemoCauTruc.Controllers.C0302.C0302GoiKhamController;

namespace DemoCauTruc.Services.C0302
{
    public interface IC0302GoiKhamService
    {
        Task<(bool Success, string Message, object Data, object DoanhNghiep, int TotalRecords, int TotalPages, int CurrentPage)>
        FilterByDayAsync(string tuNgay, string denNgay, long IDChiNhanh, int page = 1, int pageSize = 10);
        Task<byte[]> ExportBaoCaoGoiKhamPdfAsync(ExportRequest request, ISession session);

        Task<byte[]> ExportBaoCaoGoiKhamExcelAsync(ExportRequest request, ISession session);
    }
}
