﻿using AutoMapper;
using DataAccess;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Services.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Google.Apis.Sheets.v4.SpreadsheetsResource;

namespace Services.Report
{
    public interface IRevenueReportService
    {
        Task<RevenueReportRs> Run(RevenueReportRq rq);
    }

    public class RevenueReportService : BaseService, IRevenueReportService
    {
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "Mistake";

        public RevenueReportService(DanceClassDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
        {
        }

        public async Task<RevenueReportRs> Run(RevenueReportRq rq)
        {
            UserCredential credential;

            string outPutDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            string path = Path.Combine(outPutDirectory, "Report\\Credential\\credentials.json").Replace(@"file:\", string.Empty);
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/", Assembly.GetExecutingAssembly().GetName().Name);

                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Sheets API service.
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            Spreadsheet spreadsheet = new Spreadsheet();
            spreadsheet.Properties = new SpreadsheetProperties() { Title = "Revenue report" };

            CreateRequest request = service.Spreadsheets.Create(spreadsheet);
            request.Fields = "spreadsheetId";
            spreadsheet = await request.ExecuteAsync();

            var requests = new BatchUpdateSpreadsheetRequest { Requests = new List<Request>() };
            
            GridCoordinate gc = new GridCoordinate
            {
                ColumnIndex = 0,
                RowIndex = 0,
                SheetId = GetSheetId(service, spreadsheet.SpreadsheetId, "Sheet1")
            };

            var updateRequest = new Request { UpdateCells = new UpdateCellsRequest { Start = gc, Fields = "*" } };

            var listRowData = new List<RowData>();;

            var header = new RowData
            {
                Values = new List<CellData>
                {
                    new CellData { UserEnteredValue = new ExtendedValue { StringValue = "Hội viên" }, UserEnteredFormat = new CellFormat { TextFormat = new TextFormat { Bold = true } } },
                    new CellData { UserEnteredValue = new ExtendedValue { StringValue = "Số điện thoại" }, UserEnteredFormat = new CellFormat { TextFormat = new TextFormat { Bold = true } } },
                    new CellData { UserEnteredValue = new ExtendedValue { StringValue = "Gói đăng ký (buổi)" }, UserEnteredFormat = new CellFormat { TextFormat = new TextFormat { Bold = true } } },
                    new CellData { UserEnteredValue = new ExtendedValue { StringValue = "Số buổi còn lại" }, UserEnteredFormat = new CellFormat { TextFormat = new TextFormat { Bold = true } } },
                    new CellData { UserEnteredValue = new ExtendedValue { StringValue = "Ngày đăng ký" }, UserEnteredFormat = new CellFormat { TextFormat = new TextFormat { Bold = true } } },
                    new CellData { UserEnteredValue = new ExtendedValue { StringValue = "Giá" }, UserEnteredFormat = new CellFormat { TextFormat = new TextFormat { Bold = true } } },
                }
            };
            listRowData.Add(header);

            var packages = await _dbContext.Packages.ToListAsync();
                //.Where(x => x.CreatedDate <= rq.End && x.CreatedDate >= rq.Start);

            foreach (DataAccess.Entities.Package package in packages)
            {
                var rowData = new RowData
                {
                    Values = new List<CellData>
                    {
                        new CellData { UserEnteredValue = new ExtendedValue { StringValue = package.User != null ? package.User.FullName : "Imported" } },
                        new CellData { UserEnteredValue = new ExtendedValue { StringValue = package.User != null ? package.User.PhoneNumber : "Imported phone" } },
                        new CellData { UserEnteredValue = new ExtendedValue { NumberValue = package.NumberOfSessions } },
                        new CellData { UserEnteredValue = new ExtendedValue { NumberValue = package.RemainingSessions } },
                        new CellData { UserEnteredValue = new ExtendedValue { NumberValue = package.CreatedDate.ToOADate() }, UserEnteredFormat = new CellFormat { NumberFormat = new NumberFormat { Type = "DATE", Pattern = "dd-MM-yyyy" } } },
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
                    new CellData { UserEnteredValue = new ExtendedValue { StringValue = "Tổng cộng" } },
                    new CellData { UserEnteredValue = new ExtendedValue { FormulaValue = "=SUM(INDIRECT(ADDRESS(1,COLUMN())&\":\"&ADDRESS(ROW()-1,COLUMN())))" }, UserEnteredFormat = new CellFormat { NumberFormat = new NumberFormat { Type = "CURRENCY", Pattern = "#,##" } } },
                }
            };
            listRowData.Add(footer);

            updateRequest.UpdateCells.Rows = listRowData;

            requests.Requests.Add(updateRequest);
            await service.Spreadsheets.BatchUpdate(requests, spreadsheet.SpreadsheetId).ExecuteAsync();

            return new RevenueReportRs
            {
                Url = $@"https://docs.google.com/spreadsheets/d/{spreadsheet.SpreadsheetId}/edit"
            };
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