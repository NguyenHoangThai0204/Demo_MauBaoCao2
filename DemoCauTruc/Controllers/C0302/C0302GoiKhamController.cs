


using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using DemoCauTruc.Context;
using DemoCauTruc.Models.M0302;
using DemoCauTruc.Services.C0302;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuestPDF.Fluent;

namespace DemoCauTruc.Controllers.C0302
{
    [Route("bao_cao_thuc_hien_theo_doi_goi_kham_benh")]
    public class C0302GoiKhamController : Controller
    {
        private readonly IC0302GoiKhamService _service;

        public C0302GoiKhamController(IC0302GoiKhamService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View("~/Views/V0302/V0302GoiKham/Index.cshtml");
        }

        [HttpPost("filter")]
        public async Task<IActionResult> FilterByDay(string tuNgay, string denNgay, long IdChiNhanh, int page = 1, int pageSize = 10)
        {
            var result = await _service.FilterByDayAsync(tuNgay, denNgay, IdChiNhanh, page, pageSize);

            if (!result.Success)
            {
                return Json(new { success = false, message = result.Message });
            }

            return Json(new
            {
                success = true,
                message = result.Message,
                data = result.Data,
                totalRecords = result.TotalRecords,
                totalPages = result.TotalPages,
                currentPage = result.CurrentPage,
                doanhNghiep = result.DoanhNghiep
            });
        }
        public class ExportRequest
        {
            public List<M0302GoiKhamSTO> Data { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public M0302ThongTinDoanhNghiep DoanhNghiep { get; set; }
        }
        [HttpPost("export/pdf")]
        public async Task<IActionResult> ExportToPDF([FromBody] ExportRequest request)
        {
            var pdfBytes = await _service.ExportBaoCaoGoiKhamPdfAsync(request, HttpContext.Session);

            string fileName = $"BaoCaoGoiKham_{request.FromDate ?? "all"}_den_{request.ToDate ?? "now"}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        [HttpPost("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromBody] ExportRequest request)
        {
            var excelBytes = await _service.ExportBaoCaoGoiKhamExcelAsync(request, HttpContext.Session);

            string fileName = $"BaoCaoGoiKham_{request.FromDate ?? "all"}_den_{request.ToDate ?? "now"}.xlsx";
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}


