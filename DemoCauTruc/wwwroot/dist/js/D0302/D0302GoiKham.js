// goiKham.js - Xử lý ngày tháng, phân trang, xuất báo cáo cho module Gói Khám

// ==================== ĐỊNH DẠNG NGÀY NHẬP ====================
function initDateInputFormatting() {
    const dateInputIds = ["ngayTuNgay", "ngayDenNgay"];

    dateInputIds.forEach(function (id) {
        const input = document.getElementById(id);
        if (!input) return;

        input.addEventListener("input", function () {
            let value = input.value.replace(/\D/g, "");
            let formatted = "";
            let selectionStart = input.selectionStart;

            if (value.length > 0) formatted += value.substring(0, 2);
            if (value.length >= 3) formatted += "-" + value.substring(2, 4);
            if (value.length >= 5) formatted += "-" + value.substring(4, 8);

            if (formatted !== input.value) {
                const prevLength = input.value.length;
                input.value = formatted;
                const newLength = formatted.length;
                const diff = newLength - prevLength;
                input.setSelectionRange(selectionStart + diff, selectionStart + diff);
            }
        });

        input.addEventListener("click", function () {
            const pos = input.selectionStart;
            if (pos <= 2) input.setSelectionRange(0, 2);
            else if (pos <= 5) input.setSelectionRange(3, 5);
            else input.setSelectionRange(6, 10);
        });

        input.addEventListener("keydown", function (e) {
            const pos = input.selectionStart;
            let val = input.value;

            if (e.key === "Backspace" && (pos === 3 || pos === 6)) {
                e.preventDefault();
                input.value = val.slice(0, pos - 1) + val.slice(pos);
                input.setSelectionRange(pos - 1, pos - 1);
            }
            if (e.key === "Delete" && (pos === 2 || pos === 5)) {
                e.preventDefault();
                input.value = val.slice(0, pos) + val.slice(pos + 1);
                input.setSelectionRange(pos, pos);
            }
        });
    });
}

// ==================== DATEPICKER ====================
function initDatePicker() {
    $('[id="ngayTuNgay"], [id="ngayDenNgay"]').datepicker({
        format: 'dd-mm-yyyy',
        autoclose: true,
        language: 'vi',
        todayHighlight: true,
        orientation: 'bottom auto',
        weekStart: 1
    });
}

// ==================== BIẾN GLOBAL PHÂN TRANG ====================
let currentPage = 1;
let pageSize = 10;
let totalRecords = 0;
let totalPages = 0;
let isInitialLoad = true; // Thêm biến để kiểm tra lần load đầu tiên

// ==================== ĐỊNH DẠNG NGÀY XUẤT RA BẢNG ====================
function formatDate(dateString) {
    if (!dateString) return '';
    const date = new Date(dateString);
    if (isNaN(date)) return dateString;
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    return `${day}-${month}-${year}`;
}

// ==================== CẬP NHẬT BẢNG ====================
function updateTable(response) {
    const tbody = $('.container_goiKham.right tbody');
    tbody.empty();

    console.log("Dữ liệu nhận được:", response);

    if (response.totalRecords !== undefined) {
        totalRecords = response.totalRecords;
        totalPages = response.totalPages;
        currentPage = response.currentPage || 1;
        $('#pageInfo').text(`Trang ${currentPage}/${totalPages} - Tổng ${totalRecords} bản ghi`);
        renderPagination();
    }

    let data = [];
    if (Array.isArray(response)) {
        data = response;
    } else if (response && response.data) {
        data = Array.isArray(response.data) ? response.data : [response.data];
    }

    if (data.length > 0) {
        data.forEach((item, index) => {
            const stt = (currentPage - 1) * pageSize + index + 1;
            const row = `
                <tr>
                    <td>${stt}</td>
                    <td>${item.maYTe || item.MaYTe || ''}</td>
                    <td style="text-align:left;">${item.hoTen || item.HoTen || 'Không rõ'}</td>
                    <td style="text-align:left;">${item.goiKham || item.GoiKham || 'Không rõ'}</td>
                    <td>${formatDate(item.ngayDangKy || item.NgayDangKy)}</td>
                    <td style="text-align:left;">${item.trangThaiThucHien || item.TrangThaiThucHien || 'Không rõ'}</td>
                    <td style="text-align:left;">${item.chiDinhConLai || item.ChiDinhConLai || 'Không rõ'}</td>
                    <td style="text-align:left;">${item.ghiChu || item.GhiChu || 'Không rõ'}</td>
                </tr>
            `;
            tbody.append(row);
        });
    } else {
        tbody.append('<tr><td colspan="8" class="text-center">Không có dữ liệu</td></tr>');
    }
}

// ==================== RENDER PHÂN TRANG ====================
function renderPagination() {
    const pagination = $('#pagination');
    pagination.empty();

    // Tổng số trang tối thiểu là 1
    const pages = Math.max(1, totalPages || Math.ceil(totalRecords / pageSize || 1));
    if (currentPage > pages) currentPage = pages;

    $('#pageInfo').text(`Trang ${currentPage}/${pages} - Tổng ${totalRecords} bản ghi`);

    // Nút Trước
    pagination.append(`
        <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
            <a class="page-link" href="#" data-page="${Math.max(1, currentPage - 1)}">Trước</a>
        </li>
    `);

    // Hiển thị tối đa 3 trang
    const visibleCount = 3;
    let startPage = Math.max(1, currentPage - 1);
    let endPage = Math.min(pages, startPage + visibleCount - 1);

    // Nếu ở gần cuối thì lùi startPage để vẫn đủ 3 trang
    if (endPage - startPage + 1 < visibleCount) {
        startPage = Math.max(1, endPage - visibleCount + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
        pagination.append(`
            <li class="page-item ${i === currentPage ? 'active' : ''}">
                <a class="page-link" href="#" data-page="${i}">${i}</a>
            </li>
        `);
    }

    // Nút Sau
    pagination.append(`
        <li class="page-item ${currentPage === pages ? 'disabled' : ''}">
            <a class="page-link" href="#" data-page="${Math.min(pages, currentPage + 1)}">Sau</a>
        </li>
    `);
}


//function renderPagination() {
//    const pagination = $('#pagination');
//    pagination.empty();

//    // đảm bảo ít nhất 1 trang để layout không bị mất chỗ
//    const pages = Math.max(1, totalPages || Math.ceil(totalRecords / pageSize || 1));
//    // nếu currentPage vượt quá (ví dụ do đổi pageSize), điều chỉnh
//    if (currentPage > pages) currentPage = pages;

//    // Cập nhật pageInfo luôn (giữ layout ổn định)
//    $('#pageInfo').text(`Trang ${currentPage}/${pages} - Tổng ${totalRecords} bản ghi`);

//    // Prev
//    pagination.append(`
//        <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
//            <a class="page-link" href="#" data-page="${Math.max(1, currentPage - 1)}">Trước</a>
//        </li>
//    `);

//    // Hiển thị khoảng trang quanh currentPage
//    const startPage = Math.max(1, currentPage - 2);
//    const endPage = Math.min(pages, currentPage + 2);

//    if (startPage > 1) {
//        pagination.append(`
//            <li class="page-item ${1 === currentPage ? 'active' : ''}">
//                <a class="page-link" href="#" data-page="1">1</a>
//            </li>
//        `);
//        if (startPage > 2) {
//            pagination.append('<li class="page-item disabled"><span class="page-link">...</span></li>');
//        }
//    }

//    for (let i = startPage; i <= endPage; i++) {
//        pagination.append(`
//            <li class="page-item ${i === currentPage ? 'active' : ''}">
//                <a class="page-link" href="#" data-page="${i}">${i}</a>
//            </li>
//        `);
//    }

//    if (endPage < pages) {
//        if (endPage < pages - 1) {
//            pagination.append('<li class="page-item disabled"><span class="page-link">...</span></li>');
//        }
//        pagination.append(`
//            <li class="page-item ${pages === currentPage ? 'active' : ''}">
//                <a class="page-link" href="#" data-page="${pages}">${pages}</a>
//            </li>
//        `);
//    }

//    // Next
//    pagination.append(`
//        <li class="page-item ${currentPage === pages ? 'disabled' : ''}">
//            <a class="page-link" href="#" data-page="${Math.min(pages, currentPage + 1)}">Sau</a>
//        </li>
//    `);
//}

// ==================== LỌC DỮ LIỆU ====================
function filterData(isPagination = false) {
    const tuNgay = $('#ngayTuNgay').val();
    const denNgay = $('#ngayDenNgay').val();

    if (!isPagination && (!tuNgay || !denNgay)) {
        toastr.error("Vui lòng chọn cả từ ngày và đến ngày");
        return;
    }

    if (!isPagination) {
        function parseDMY(s) {
            const p = s.split('-');
            return new Date(p[2], p[1] - 1, p[0]);
        }
        if (parseDMY(tuNgay) > parseDMY(denNgay)) {
            toastr.error("Từ ngày phải bé hơn đến ngày");
            return;
        }
    }

    // Hiển thị spinner và làm mờ bảng
    $('#loadingSpinner').show();
    $('.table-wrapper').css('opacity', '0.5');

    $.ajax({
        url: '/bao_cao_thuc_hien_theo_doi_goi_kham_benh/filter',
        type: 'POST',
        data: {
            tuNgay: tuNgay,
            denNgay: denNgay,
            IdChiNhanh: _idcn,
            page: currentPage,
            pageSize: pageSize
        },
        success: function (response) {
            if (response.success) {
                // Chỉ hiển thị thông báo khi không phải phân trang và là lần đầu tiên
                if (!isPagination && isInitialLoad) {
                    toastr.info(response.message);
                    isInitialLoad = false;
                }

                updateTable(response);
                // LƯU DỮ LIỆU HIỆN TẠI (chỉ là page hiện tại)
                window.filteredData = Array.isArray(response.data) ? response.data : (response.data ? [response.data] : []);
                // lưu meta (nếu server trả)
                totalRecords = response.totalRecords || totalRecords;
                totalPages = response.totalPages || totalPages;
                window.doanhNghiep = response.doanhNghiep || null;
            } else {
                toastr.error("Có lỗi khi lọc dữ liệu");
            }
        },
        complete: function () {
            // Ẩn spinner khi hoàn thành (dù thành công hay thất bại)
            $('#loadingSpinner').hide();
            $('.table-wrapper').css('opacity', '1');
        }
    });
}

// ==================== HÀM HỖ TRỢ LẤY TOÀN BỘ DỮ LIỆU (all pages) ====================
// Trả về Promise resolving mảng tất cả bản ghi theo filter (tuNgay, denNgay)
function ajaxFilterRequest(payload) {
    return new Promise((resolve, reject) => {
        $.ajax({
            url: '/bao_cao_thuc_hien_theo_doi_goi_kham_benh/filter',
            type: 'POST',
            data: payload,
            success: function (resp) { resolve(resp); },
            error: function (xhr, st, err) { reject(err || st || xhr); }
        });
    });
}

function fetchAllFilteredData(tuNgay, denNgay) {
    return new Promise((resolve, reject) => {
        // gọi page 1 trước để biết totalPages
        const basePayload = {
            tuNgay: tuNgay || '',
            denNgay: denNgay || '',
            IdChiNhanh: _idcn,
            page: 1,
            pageSize: pageSize // dùng pageSize hiện tại (server trả totalPages)
        };

        ajaxFilterRequest(basePayload).then(firstResp => {
            if (!firstResp || !firstResp.success) {
                reject(firstResp || 'Lỗi khi lấy dữ liệu trang 1');
                return;
            }
            const firstData = Array.isArray(firstResp.data) ? firstResp.data : (firstResp.data ? [firstResp.data] : []);
            const tp = firstResp.totalPages || 1;

            // Nếu chỉ 1 trang => xong
            if (tp <= 1) {
                resolve(firstData);
                return;
            }

            // build promises cho các trang còn lại
            const promises = [];
            for (let p = 2; p <= tp; p++) {
                const payload = {
                    tuNgay: tuNgay || '',
                    denNgay: denNgay || '',
                    IdChiNhanh: _idcn,
                    page: p,
                    pageSize: pageSize
                };
                promises.push(ajaxFilterRequest(payload));
            }

            Promise.all(promises)
                .then(results => {
                    // mỗi result là resp của server, lấy data của từng resp
                    const pagesData = results.map(r => Array.isArray(r.data) ? r.data : (r.data ? [r.data] : []));
                    const all = firstData.concat(...pagesData);
                    resolve(all);
                })
                .catch(err => {
                    reject(err);
                });
        }).catch(err => reject(err));
    });
}

// ==================== XUẤT EXCEL/PDF (sử dụng fetchAllFilteredData nếu cần) ====================
function validateExportDatesAndData() {
    const tuNgay = $('#ngayTuNgay').val();
    const denNgay = $('#ngayDenNgay').val();

    if (!tuNgay && !denNgay) {
        if (!window.filteredData || window.filteredData.length === 0) {
            toastr.error("Không có dữ liệu để xuất");
            return false;
        }
        return true;
    }
    if ((tuNgay && !denNgay) || (!tuNgay && denNgay)) {
        toastr.error("Vui lòng chọn cả từ ngày và đến ngày");
        return false;
    }

    function parseDMY(s) {
        const parts = s.split('-');
        return new Date(parts[2], parts[1] - 1, parts[0]);
    }
    if (parseDMY(tuNgay) > parseDMY(denNgay)) {
        toastr.error("Từ ngày phải nhỏ hơn hoặc bằng đến ngày");
        return false;
    }
    if (!window.filteredData || window.filteredData.length === 0) {
        toastr.error("Không có dữ liệu để xuất");
        return false;
    }
    return true;
}

// Helper để thực hiện gửi request export Excel khi đã có toàn bộ data
function doExportExcel(finalData, btn, originalHtml) {
    const requestData = {
        data: finalData,
        fromDate: $('#ngayTuNgay').val(),
        toDate: $('#ngayDenNgay').val(),
        doanhNghiep: window.doanhNghiep || null
    };

    $.ajax({
        url: '/bao_cao_thuc_hien_theo_doi_goi_kham_benh/export/excel',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(requestData),
        xhrFields: { responseType: 'blob' },
        success: function (data, status, xhr) {
            const contentType = xhr.getResponseHeader('content-type') || '';
            if (!contentType.includes('spreadsheet') && !contentType.includes('vnd.openxmlformats')) {
                toastr.error('Tệp trả về không phải Excel');
                return;
            }
            const blob = new Blob([data], { type: contentType });
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `BaoCaoGoiKham_${requestData.fromDate || 'all'}_den_${requestData.toDate || 'now'}.xlsx`;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);
        },
        error: function () {
            toastr.error('Lỗi khi tạo file Excel');
        },
        complete: function () {
            btn.html(originalHtml);
            btn.prop('disabled', false);
        }
    });
}

$('#btnExportExcelGoiKham').off('click').on('click', function (e) {
    e.preventDefault();
    if (!validateExportDatesAndData()) return;

    const btn = $(this);
    const originalHtml = btn.html();
    btn.html('<span class="spinner-border spinner-border-sm"></span> Đang tạo');
    btn.prop('disabled', true);

    const tu = $('#ngayTuNgay').val();
    const den = $('#ngayDenNgay').val();

    // nếu window.filteredData chưa chứa tất cả bản ghi => fetch all
    if (!window.filteredData || (totalRecords && window.filteredData.length < totalRecords)) {
        fetchAllFilteredData(tu, den)
            .then(allData => {
                // cập nhật window.filteredData để lần sau không cần fetch lại
                window.filteredData = allData;
                doExportExcel(allData, btn, originalHtml);
            })
            .catch(err => {
                console.error('Lỗi khi lấy toàn bộ dữ liệu để xuất:', err);
                toastr.error('Không thể lấy toàn bộ dữ liệu để xuất');
                btn.html(originalHtml);
                btn.prop('disabled', false);
            });
    } else {
        // đã có đủ dữ liệu
        doExportExcel(window.filteredData, btn, originalHtml);
    }
});

// PDF export
function doExportPdf(finalData, btnElem) {
    const requestData = {
        data: finalData,
        fromDate: $('#ngayTuNgay').val(),
        toDate: $('#ngayDenNgay').val(),
        doanhNghiep: window.doanhNghiep || null
    };

    fetch("/bao_cao_thuc_hien_theo_doi_goi_kham_benh/export/pdf", {
        method: "POST",
        headers: { 'Content-Type': 'application/json', 'Accept': 'application/pdf' },
        body: JSON.stringify(requestData)
    })
        .then(res => {
            if (!res.ok) throw new Error('Network response was not ok');
            return res.blob();
        })
        .then(blob => {
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement("a");
            a.href = url;
            a.download = "BaoCaoGoiKham.pdf";
            a.click();
            window.URL.revokeObjectURL(url);
        })
        .catch(error => {
            console.error("Error:", error);
            toastr.error("Có lỗi khi tạo PDF");
        })
        .finally(() => {
            btnElem.innerHTML = '<i class="bi bi-file-earmark-pdf"></i> Xuất PDF';
            btnElem.disabled = false;
        });
}

$('#btnExportPDFGoiKham').off('click').on('click', function (e) {
    e.preventDefault();
    if (!validateExportDatesAndData()) return;

    const btn = this;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Đang tạo';
    btn.disabled = true;

    const tu = $('#ngayTuNgay').val();
    const den = $('#ngayDenNgay').val();

    if (!window.filteredData || (totalRecords && window.filteredData.length < totalRecords)) {
        fetchAllFilteredData(tu, den)
            .then(allData => {
                window.filteredData = allData;
                doExportPdf(allData, btn);
            })
            .catch(err => {
                console.error('Lỗi khi lấy toàn bộ dữ liệu để xuất PDF:', err);
                toastr.error('Không thể lấy toàn bộ dữ liệu để xuất');
                btn.innerHTML = '<i class="bi bi-file-earmark-pdf"></i> Xuất PDF';
                btn.disabled = false;
            });
    } else {
        doExportPdf(window.filteredData, btn);
    }
});

// ==================== SỰ KIỆN GIAO DIỆN ====================
$(document).ready(function () {
    initDatePicker();
    initDateInputFormatting();

    $('#pageSizeSelect').change(function () {
        pageSize = parseInt($(this).val());
        currentPage = 1;
        filterData();
    });

    $(document).on('click', '.page-link', function (e) {
        e.preventDefault();
        const page = $(this).data('page');
        if (page >= 1 && page <= totalPages && page !== currentPage) {
            currentPage = page;
            filterData(true); // Truyền true để biết đây là phân trang
        }
    });

    $('#btnFilter').click(function (e) {
        e.preventDefault();
        currentPage = 1;
        isInitialLoad = true; // Đánh dấu là lần load đầu tiên
        filterData();
    });
});