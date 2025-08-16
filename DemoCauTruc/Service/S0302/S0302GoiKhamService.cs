using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using DemoCauTruc.Context;
using DemoCauTruc.Controllers.C0302;
using DemoCauTruc.Models.M0302;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using static DemoCauTruc.Controllers.C0302.C0302GoiKhamController;

namespace DemoCauTruc.Services.C0302
{
    public class S0302GoiKhamService : IC0302GoiKhamService
    {
        private readonly AppDbContext _dbService;
        private readonly ILogger<S0302GoiKhamService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public S0302GoiKhamService(AppDbContext dbService, ILogger<S0302GoiKhamService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _dbService = dbService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool Success, string Message, object Data, object DoanhNghiep, int TotalRecords, int TotalPages, int CurrentPage)>
        FilterByDayAsync(string tuNgay, string denNgay, long IDChiNhanh, int page = 1, int pageSize = 10)
        {
            var doanhNghiep = await _dbService.ThongTinDoanhNghieps
                .Where(d => d.IDChiNhanh == IDChiNhanh)
                .Select(d => new
                {
                    d.ID,
                    d.MaCSKCB,
                    d.TenCSKCB,
                    d.DiaChi,
                    d.DienThoai,
                    d.Email
                })
                .FirstOrDefaultAsync();

            var session = _httpContextAccessor.HttpContext?.Session;
            session?.SetString("DoanhNghiepInfo", JsonConvert.SerializeObject(doanhNghiep));
            _logger.LogInformation("Doanh Nghiep Info: {@DoanhNghiep}", doanhNghiep);

            if (doanhNghiep == null)
            {
                _logger.LogWarning("No doanh nghiep found for ChiNhanh ID: {IdChiNhanh}", IDChiNhanh);
                return (false, "Không tìm thấy thông tin doanh nghiệp.", null, null, 0, 0, page);
            }

            var allData = await _dbService.GoiKhamSTOs
                .FromSqlRaw("EXEC S0302_DSGoiKhamBenhLocTheoNgay @TuNgay, @DenNgay, @IdChiNhanh",
                    new SqlParameter("@TuNgay", tuNgay),
                    new SqlParameter("@DenNgay", denNgay),
                    new SqlParameter("@IdChiNhanh", IDChiNhanh))
                .AsNoTracking()
                .ToListAsync();

            var totalRecords = allData.Count;
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            var pagedData = allData.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            string message = pagedData.Any()
                ? $"Tìm thấy {totalRecords} kết quả từ {tuNgay} đến {denNgay}."
                : $"Không tìm thấy kết quả nào từ {tuNgay} đến {denNgay}.";

            var sessionData = new
            {
                Data = allData,
                FromDate = tuNgay,
                ToDate = denNgay
            };
            session?.SetString("FilteredData", JsonConvert.SerializeObject(sessionData));

            return (true, message, pagedData, doanhNghiep, totalRecords, totalPages, page);
        }

        private M0302ThongTinDoanhNghiep GetDoanhNghiepFromRequestOrSession(ExportRequest request, ISession session)
        {
            M0302ThongTinDoanhNghiep doanhNghiepObj = null;
            try
            {
                if (request.DoanhNghiep != null)
                {
                    var json = JsonConvert.SerializeObject(request.DoanhNghiep);
                    doanhNghiepObj = JsonConvert.DeserializeObject<M0302ThongTinDoanhNghiep>(json);
                }

                if (doanhNghiepObj == null)
                {
                    var doanhNghiepJson = session.GetString("DoanhNghiepInfo");
                    if (!string.IsNullOrEmpty(doanhNghiepJson))
                    {
                        doanhNghiepObj = JsonConvert.DeserializeObject<M0302ThongTinDoanhNghiep>(doanhNghiepJson);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi parse doanh nghiep từ request hoặc session");
            }

            return doanhNghiepObj ?? new M0302ThongTinDoanhNghiep
            {
                TenCSKCB = "Tên đơn vị",
                DiaChi = "",
                DienThoai = ""
            };
        }
        public async Task<byte[]> ExportBaoCaoGoiKhamPdfAsync(ExportRequest request, ISession session)
        {
            var doanhNghiepObj = GetDoanhNghiepFromRequestOrSession(request, session);

            var data = request.Data ?? new List<M0302GoiKhamSTO>();
            var document = new P0302BaoCaoGoiKhamPDF(data, request.FromDate, request.ToDate, doanhNghiepObj);

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }
        public async Task<byte[]> ExportBaoCaoGoiKhamExcelAsync(ExportRequest request, ISession session)
        {
            var doanhNghiepObj = GetDoanhNghiepFromRequestOrSession(request, session);

            var data = request.Data ?? new List<M0302GoiKhamSTO>();
            var fromDate = request.FromDate;
            var toDate = request.ToDate;

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Báo cáo Gói khám");

            worksheet.Style.Font.FontName = "Times New Roman";
            worksheet.Style.Font.FontSize = 11;

            // Logo
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "dist", "img", "logo.png");
            if (File.Exists(imagePath))
            {
                var image = worksheet.AddPicture(imagePath)
                                     .MoveTo(worksheet.Cell("A1"))
                                     .WithPlacement(XLPicturePlacement.FreeFloating);
                image.Width = 100;
                image.Height = 60;
            }

            // Thông tin doanh nghiệp
            worksheet.Cell("B1").Value = doanhNghiepObj.TenCSKCB ?? "BỆNH VIỆN";
            worksheet.Cell("B1").Style.Font.Bold = true;
            worksheet.Cell("B1").Style.Font.FontSize = 12;

            worksheet.Cell("B2").Value = $"Địa chỉ: {doanhNghiepObj.DiaChi ?? ""}";
            worksheet.Cell("B2").Style.Font.FontSize = 10;

            worksheet.Cell("B3").Value = $"Điện thoại: {doanhNghiepObj.DienThoai ?? ""}";
            worksheet.Cell("B3").Style.Font.FontSize = 10;

            worksheet.Cell("B4").Value = $"Email: {doanhNghiepObj.Email ?? ""}";
            worksheet.Cell("B4").Style.Font.FontSize = 10;

            worksheet.Range("E1:H1").Merge();
            worksheet.Cell("E1").Value = "BÁO CÁO THỰC HIỆN THEO DÕI GÓI KHÁM BỆNH";
            worksheet.Cell("E1").Style.Font.Bold = true;
            worksheet.Cell("E1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell("E1").Style.Font.FontSize = 12;

            worksheet.Range("E2:H2").Merge();
            worksheet.Cell("E2").Value = "Đơn vị thống kê";
            worksheet.Cell("E2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell("E2").Style.Font.FontSize = 10;

            worksheet.Range("E3:H3").Merge();
            worksheet.Cell("E3").Value = $"Từ ngày: {fromDate} đến ngày: {toDate}";
            worksheet.Cell("E3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell("E3").Style.Font.FontSize = 10;

            // Kích thước cột
            worksheet.Column("A").Width = 12;
            worksheet.Column("B").Width = 25;
            worksheet.Column("C").Width = 35;
            worksheet.Column("D").Width = 25;
            worksheet.Column("E").Width = 18;
            worksheet.Column("F").Width = 20;
            worksheet.Column("G").Width = 20;
            worksheet.Column("H").Width = 30;

            int currentRow = 4;

            currentRow += 2;

            // Header bảng
            worksheet.Cell(currentRow, 1).Value = "STT";
            worksheet.Cell(currentRow, 2).Value = "Mã y tế";
            worksheet.Cell(currentRow, 3).Value = "Họ tên";
            worksheet.Cell(currentRow, 4).Value = "Gói khám";
            worksheet.Cell(currentRow, 5).Value = "Ngày đăng ký";
            worksheet.Cell(currentRow, 6).Value = "Trạng thái";
            worksheet.Cell(currentRow, 7).Value = "Chỉ định còn lại";
            worksheet.Cell(currentRow, 8).Value = "Ghi chú";

            var headerRange = worksheet.Range(currentRow, 1, currentRow, 8);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            currentRow++;

            int stt = 1;
            foreach (var item in data)
            {
                worksheet.Cell(currentRow, 1).Value = stt++;
                worksheet.Cell(currentRow, 2).Value = item.MaYTe;
                worksheet.Cell(currentRow, 3).Value = item.HoTen;
                worksheet.Cell(currentRow, 4).Value = item.GoiKham;
                worksheet.Cell(currentRow, 5).Value = item.NgayDangKy.ToString("dd-MM-yyyy");
                worksheet.Cell(currentRow, 6).Value = item.TrangThaiThucHien;
                worksheet.Cell(currentRow, 7).Value = item.ChiDinhConLai;
                worksheet.Cell(currentRow, 8).Value = item.GhiChu;

                var dataRange = worksheet.Range(currentRow, 1, currentRow, 8);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                currentRow++;
            }

            worksheet.Cell(currentRow + 1, 8).Value = $"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}";
            worksheet.Cell(currentRow + 1, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(currentRow + 1, 8).Style.Font.FontSize = 10;

            currentRow += 3;
            worksheet.Cell(currentRow, 2).Value = "THỦ TRƯỞNG ĐƠN VỊ";
            worksheet.Cell(currentRow, 4).Value = "THỦ QUỸ";
            worksheet.Cell(currentRow, 6).Value = "KẾ TOÁN";
            worksheet.Cell(currentRow, 8).Value = "NGƯỜI LẬP BẢNG";

            var signHeaderRange = worksheet.Range(currentRow, 2, currentRow, 8);
            signHeaderRange.Style.Font.Bold = true;
            signHeaderRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            currentRow++;

            worksheet.Cell(currentRow, 2).Value = "(Ký, họ tên, đóng dấu)";
            worksheet.Cell(currentRow, 4).Value = "(Ký, họ tên)";
            worksheet.Cell(currentRow, 6).Value = "(Ký, họ tên)";
            worksheet.Cell(currentRow, 8).Value = "(Ký, họ tên)";

            var signNoteRange = worksheet.Range(currentRow, 2, currentRow, 8);
            signNoteRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            signNoteRange.Style.Font.Italic = true;
            signNoteRange.Style.Font.FontSize = 9;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        
    }
}
