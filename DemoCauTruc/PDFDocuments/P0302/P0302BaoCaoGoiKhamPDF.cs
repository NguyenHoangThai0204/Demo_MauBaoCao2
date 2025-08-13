using DemoCauTruc.Models.M0302;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class P0302BaoCaoGoiKhamPDF : IDocument
{
    private readonly List<M0302GoiKhamSTO> _data;
    private readonly string _fromDate;
    private readonly string _toDate;
    private readonly M0302ThongTinDoanhNghiep _thongTinDoanhNghiep;

    public P0302BaoCaoGoiKhamPDF(List<M0302GoiKhamSTO> data, string fromDate, string toDate, M0302ThongTinDoanhNghiep doanhNghiep)
    {
        _data = data ?? new List<M0302GoiKhamSTO>();
        _thongTinDoanhNghiep = doanhNghiep ?? new M0302ThongTinDoanhNghiep
        {
            TenCSKCB = "Tên đơn vị",
            DiaChi = "",
            DienThoai = ""
        };

        if (string.IsNullOrEmpty(fromDate) || string.IsNullOrEmpty(toDate))
        {
            if (_data.Any())
            {
                _fromDate = _data.Min(x => x.NgayDangKy).ToString("dd-MM-yyyy");
                _toDate = _data.Max(x => x.NgayDangKy).ToString("dd-MM-yyyy");
            }
            else
            {
                _fromDate = DateTime.Now.ToString("dd-MM-yyyy");
                _toDate = DateTime.Now.ToString("dd-MM-yyyy");
            }
        }
        else
        {
            _fromDate = fromDate;
            _toDate = toDate;
        }
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(20);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(12).FontColor(Colors.Black));

            page.Content()
                .Column(column =>
                {
                    column.Item()
                        .Row(row =>
                        {
                            row.RelativeColumn(0.59f)
                                .Row(innerRow =>
                                {
                                    // Đặt kích thước ConstantColumn bằng với kích thước ảnh
                                    innerRow.ConstantColumn(70) // Thay đổi từ 70 xuống 65 để khớp với width ảnh
                                        .Column(logoColumn =>
                                        {
                                            logoColumn.Item()
                                                .Width(70)
                                                .Height(70)
                                                .Image("wwwroot/dist/img/logo.png", ImageScaling.FitArea);
                                        });

                                    innerRow.RelativeColumn()
                                        .PaddingLeft(2)
                                        .Column(infoColumn =>
                                        {
                                            infoColumn.Item().Text(_thongTinDoanhNghiep.TenCSKCB ?? "").Bold().FontSize(13);
                                            infoColumn.Item().Text($"Địa chỉ: {_thongTinDoanhNghiep.DiaChi ?? ""}").FontSize(11).WrapAnywhere(false);
                                            infoColumn.Item().Text($"Điện thoại: {_thongTinDoanhNghiep.DienThoai ?? ""}").FontSize(11);
                                            infoColumn.Item().Text($"Email: {_thongTinDoanhNghiep.Email ?? ""}").FontSize(11);
                                        });
                                });
                            row.RelativeColumn(0.4f)
                                .Column(nationalColumn =>
                                {
                                    nationalColumn.Item()
                                          .AlignRight()
                                          .Text("BÁO CÁO THỰC HIỆN THEO DÕI GÓI KHÁM BỆNH ")
                                          .FontFamily("Times New Roman")
                                          .FontSize(13)
                                          .Bold()
                                          .FontColor(Colors.Blue.Darken2); // Thêm dòng này để đổi màu xanh

                                    nationalColumn.Item()
                                        .AlignRight()
                                        .Text("Đơn vị thống kê")
                                        .FontSize(11)
                                        .FontFamily("Times New Roman");

                                    nationalColumn.Item()
                                         .AlignRight()
                                         .Text(text =>
                                         {
                                             text.DefaultTextStyle(TextStyle.Default.FontSize(10).SemiBold());

                                             if (_fromDate == _toDate)
                                                 text.Span($"Ngày: {_fromDate}");
                                             else
                                                 text.Span($"Từ ngày: {_fromDate} đến ngày: {_toDate}");
                                         });
                                });
                        });

                    // Bảng dữ liệu
                    column.Item()
                        .Table(table =>
                        {
                            // Định nghĩa cột
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.6f);
                                columns.RelativeColumn(2f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.7f);
                                columns.RelativeColumn(2f);
                            });

                            // Header
                            table.Header(header =>
                            {
                                void AddHeaderCell(string text)
                                {
                                    header.Cell()
                                        .Border(1)
                                        .BorderColor(Colors.Grey.Darken1)
                                        .Background(Colors.Grey.Lighten3)
                                        .PaddingVertical(2)
                                        .PaddingHorizontal(3)
                                        .AlignCenter()
                                        .AlignMiddle()
                                        .Text(text)
                                        .Bold()
                                        .FontSize(13);
                                }

                                AddHeaderCell("STT");
                                AddHeaderCell("Mã y tế");
                                AddHeaderCell("Họ và tên");
                                AddHeaderCell("Gói khám");
                                AddHeaderCell("Ngày đăng ký");
                                AddHeaderCell("Trạng thái thực hiện");
                                AddHeaderCell("Chỉ định còn lại");
                                AddHeaderCell("Ghi chú");
                            });

                            // Body (tbody)
                            int stt = 1;
                            foreach (var item in _data)
                            {
                                table.Cell().Element(c => CellStyle(c)).AlignCenter().Text(stt++);
                                table.Cell().Element(c => CellStyle(c)).AlignCenter().Text(item.MaYTe);
                                table.Cell().Element(c => CellStyle(c)).Text(item.HoTen);
                                table.Cell().Element(c => CellStyle(c)).Text(item.GoiKham);
                                table.Cell().Element(c => CellStyle(c)).AlignCenter().Text(item.NgayDangKy.ToString("dd-MM-yyyy"));
                                table.Cell().Element(c => CellStyle(c)).Text(item.TrangThaiThucHien);
                                table.Cell().Element(c => CellStyle(c)).Text(item.ChiDinhConLai);
                                table.Cell().Element(c => CellStyle(c)).Text(item.GhiChu);
                            }
                        });

                    column.Item().PaddingTop(10);

                    // Nhóm ngày tháng và chữ ký vào một Item duy nhất

                    column.Item().EnsureSpace(90).Column(group =>
                    {
                        // Ngày tháng
                        group.Item().AlignRight().PaddingRight(39)
                            .Text($"Ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}")
                            .FontSize(10).Italic();

                        // Bảng chữ ký
                        group.Item().PaddingTop(3).PaddingBottom(25)
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                void AddSignCell(string title, string note)
                                {
                                    table.Cell().Element(cell =>
                                        cell.Column(c =>
                                        {
                                            c.Item().AlignCenter().Text(title).Bold().FontSize(11);
                                            c.Item().AlignCenter().PaddingTop(3).Text(note).Italic().FontSize(9);
                                        }));
                                }

                                AddSignCell("THỦ TRƯỞNG ĐƠN VỊ", "(Ký, họ tên, đóng dấu)");
                                AddSignCell("THỦ QUỸ", "(Ký, họ tên)");
                                AddSignCell("KẾ TOÁN", "(Ký, họ tên)");
                                AddSignCell("NGƯỜI LẬP BẢNG", "(Ký, họ tên)");
                            });
                    });


                });

            // Footer
            page.Footer()
                       .AlignRight()
                        .Text(x =>
                        {
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
        });
    }

    // Style cho các ô dữ liệu (tbody)
    private IContainer CellStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Medium)
            .PaddingVertical(5)
            .PaddingHorizontal(3)
            .Background(Colors.White)
            .AlignMiddle() // canh giữa theo chiều cao
            .DefaultTextStyle(TextStyle.Default.FontSize(11));
    }

}
