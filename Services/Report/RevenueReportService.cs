using AutoMapper;
using DataAccess;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Services.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Report
{
    public interface IRevenueReportService
    {
        Task<RevenueReportRs> Run(RevenueReportRq rq);
    }

    public class RevenueReportService : BaseService, IRevenueReportService
    {
        static readonly string ApplicationName = "MistakeDance";
        static readonly string SpreadsheetId = "1D2ZqUtFElFmObNLAoie8gP_b8fLZlGliFsvcgm6RTPo";

        public RevenueReportService(DanceClassDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
        {
        }

        public async Task<RevenueReportRs> Run(RevenueReportRq rq)
        {
            // Create Google Sheets API service.
            using (var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = rq.Credential,
                ApplicationName = ApplicationName,
            }))
            {
                //// Create sheet
                //Spreadsheet spreadsheet = new Spreadsheet();
                //spreadsheet.Properties = new SpreadsheetProperties() { Title = "Revenue report" };

                //CreateRequest request = service.Spreadsheets.Create(spreadsheet);
                //request.Fields = "spreadsheetId";
                //spreadsheet = await request.ExecuteAsync();

                int sheetId = GetSheetId(service, SpreadsheetId, "Sheet1");

                var requests = new BatchUpdateSpreadsheetRequest { Requests = new List<Request>() };

                var clearAllRequest = new Request { UpdateCells = new UpdateCellsRequest { Range = new GridRange { SheetId = sheetId }, Fields = "userEnteredValue" } };

                requests.Requests.Add(clearAllRequest);

                GridCoordinate gc = new GridCoordinate
                {
                    ColumnIndex = 0,
                    RowIndex = 0,
                    SheetId = sheetId
                };

                var updateRequest = new Request { UpdateCells = new UpdateCellsRequest { Start = gc, Fields = "*" } };

                var listRowData = new List<RowData>(); ;

                var header = new RowData
                {
                    Values = new List<CellData>
                    {
                        new CellData { UserEnteredValue = new ExtendedValue { StringValue = "Hội viên" }, UserEnteredFormat = new CellFormat { TextFormat = new TextFormat { Bold = true } } },
                        new CellData { UserEnteredValue = new ExtendedValue { StringValue = "Số điện thoại" }, UserEnteredFormat = new CellFormat { TextFormat = new TextFormat { Bold = true } } },
                        new CellData { UserEnteredValue = new ExtendedValue { StringValue = "Gói đăng ký (buổi)" }, UserEnteredFormat = new CellFormat { TextFormat = new TextFormat { Bold = true } } },
                        new CellData { UserEnteredValue = new ExtendedValue { StringValue = "Số buổi còn lại" }, UserEnteredFormat = new CellFormat { TextFormat = new TextFormat { Bold = true } } },
                        new CellData { UserEnteredValue = new ExtendedValue { StringValue = "Ngày đăng ký" }, UserEnteredFormat = new CellFormat { TextFormat = new TextFormat { Bold = true } } },
                        new CellData { UserEnteredValue = new ExtendedValue { StringValue = "Chi nhánh đăng ký" }, UserEnteredFormat = new CellFormat { TextFormat = new TextFormat { Bold = true } } },
                        new CellData { UserEnteredValue = new ExtendedValue { StringValue = "Giá" }, UserEnteredFormat = new CellFormat { TextFormat = new TextFormat { Bold = true } } },
                    }
                };
                listRowData.Add(header);

                DateTime start = rq.Start.Date;
                DateTime end = rq.End.Date.AddDays(1).AddSeconds(-1);

                var query = _dbContext.Packages.Where(x => x.CreatedDate >= start && x.CreatedDate <= end);
                if (rq.BranchIds != null && rq.BranchIds.Count > 0)
                {
                    query = query
                        .Where(x =>
                            (x.RegisteredBranch != null && rq.BranchIds.Contains(x.RegisteredBranch.Id)) ||
                            (x.User != null && x.User.RegisteredBranch != null && rq.BranchIds.Contains(x.User.RegisteredBranch.Id)));
                }

                var packages = await query.ToListAsync();

                foreach (DataAccess.Entities.Package package in packages)
                {
                    string registeredBranch = string.Empty;
                    if (package.RegisteredBranch != null)
                    {
                        registeredBranch = package.RegisteredBranch.Name;
                    }
                    else if (package.User != null && package.User.RegisteredBranch != null)
                    {
                        registeredBranch = package.User.RegisteredBranch.Name;
                    }

                    var rowData = new RowData
                    {
                        Values = new List<CellData>
                        {
                            new CellData { UserEnteredValue = new ExtendedValue { StringValue = package.User != null ? package.User.FullName : "Imported" } },
                            new CellData { UserEnteredValue = new ExtendedValue { StringValue = package.User != null ? package.User.PhoneNumber : "Imported phone" } },
                            new CellData { UserEnteredValue = new ExtendedValue { NumberValue = package.NumberOfSessions } },
                            new CellData { UserEnteredValue = new ExtendedValue { NumberValue = package.RemainingSessions } },
                            new CellData { UserEnteredValue = new ExtendedValue { NumberValue = package.CreatedDate.ToOADate() }, UserEnteredFormat = new CellFormat { NumberFormat = new NumberFormat { Type = "DATE", Pattern = "dd-MM-yyyy" } } },
                            new CellData { UserEnteredValue = new ExtendedValue { StringValue = registeredBranch }, UserEnteredFormat = new CellFormat { TextFormat = new TextFormat { ForegroundColor = GenerateBranchTextColor(registeredBranch) } } },
                            new CellData { UserEnteredValue = new ExtendedValue { NumberValue = package.Price }, UserEnteredFormat = new CellFormat { NumberFormat = new NumberFormat { Type = "CURRENCY", Pattern = "#,##" } } },
                        }
                    };
                    listRowData.Add(rowData);
                }

                var footer = new RowData
                {
                    Values = new List<CellData>
                    {
                        new CellData { UserEnteredValue = new ExtendedValue { StringValue = string.Empty } },
                        new CellData { UserEnteredValue = new ExtendedValue { StringValue = string.Empty } },
                        new CellData { UserEnteredValue = new ExtendedValue { StringValue = string.Empty } },
                        new CellData { UserEnteredValue = new ExtendedValue { StringValue = string.Empty } },
                        new CellData { UserEnteredValue = new ExtendedValue { StringValue = string.Empty } },
                        new CellData { UserEnteredValue = new ExtendedValue { StringValue = "Tổng cộng" }, UserEnteredFormat = new CellFormat { TextFormat = new TextFormat { Bold = true } } },
                        new CellData { UserEnteredValue = new ExtendedValue { FormulaValue = "=SUM(INDIRECT(ADDRESS(1,COLUMN())&\":\"&ADDRESS(ROW()-1,COLUMN())))" }, UserEnteredFormat = new CellFormat { NumberFormat = new NumberFormat { Type = "CURRENCY", Pattern = "#,##" }, TextFormat = new TextFormat { Bold = true } } },
                    }
                };
                listRowData.Add(footer);

                updateRequest.UpdateCells.Rows = listRowData;

                requests.Requests.Add(updateRequest);

                if (rq.OrderBy == (int)RevenueReportRq.OrderByValues.Branch &&
                    (rq.BranchIds == null || (rq.BranchIds != null && rq.BranchIds.Count > 1)))
                {
                    var sortRequest = new Request
                    {
                        SortRange = new SortRangeRequest
                        {
                            Range = new GridRange { StartRowIndex = 1, EndRowIndex = packages.Count + 1, SheetId = sheetId },
                            SortSpecs = new List<SortSpec>
                            {
                                new SortSpec
                                {
                                    DimensionIndex = 5,
                                    SortOrder = "ASCENDING"
                                }
                            }
                        },
                    };

                    requests.Requests.Add(sortRequest);
                }

                await service.Spreadsheets.BatchUpdate(requests, SpreadsheetId).ExecuteAsync();

                return new RevenueReportRs
                {
                    Url = $@"https://docs.google.com/spreadsheets/d/{SpreadsheetId}/edit"
                };
            }
        }

        private Color GenerateBranchTextColor(string registeredBranch)
        {
            registeredBranch = registeredBranch.ToLower();
            if (registeredBranch == "Lê Văn Sỹ".ToLower())
            {
                // Green
                return new Color() { Red = 52, Green = 168, Blue = 83 };
            }

            if (registeredBranch == "Quận 3".ToLower())
            {
                // Orange
                return new Color() { Red = 255, Green = 109, Blue = 1 };
            }

            if (registeredBranch == "Phú Nhuận".ToLower())
            {
                // Blue
                return new Color() { Red = 66, Green = 133, Blue = 244 };
            }

            // Black
            return new Color() { Red = 0, Green = 0, Blue = 0 };
        }

        private int GetSheetId(SheetsService service, string spreadSheetId, string spreadSheetName)
        {
            var spreadsheet = service.Spreadsheets.Get(spreadSheetId).Execute();
            var sheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == spreadSheetName);
            int sheetId = (int)sheet.Properties.SheetId;
            return sheetId;
        }
    }
}
