using AutoMapper;
using AutoMapper.QueryableExtensions;
using DataAccess;
using ExcelLibrary.SpreadSheet;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Services.Common;
using Services.Schedule;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Services.Report
{
    public interface IWeeklyScheduleReportService
    {
        Task<WeeklyScheduleReportRs> Run(WeeklyScheduleReportRq rq);
    }

    public class WeeklyScheduleReportService : BaseService, IWeeklyScheduleReportService
    {
        private IConfigurationProvider _mappingConfig;
        private const int DATE_ROW = 1;
        private const int TIME_ROW_BEGIN = 2;

        public WeeklyScheduleReportService(DanceClassDbContext dbContext, IMapper mapper, IConfigurationProvider mappingConfig) : base(dbContext, mapper)
        {
            _mappingConfig = mappingConfig;
        }

        public async Task<WeeklyScheduleReportRs> Run(WeeklyScheduleReportRq rq)
        {
            DateTime start = rq.Start;
            DateTime end = rq.End;

            var sessionGroups = (await _dbContext.ScheduleDetails
                .Where(x => x.Date <= rq.End && x.Date >= rq.Start)
                .ProjectTo<ScheduleDetailDTO>(_mappingConfig, dest => dest.Schedule.Class, dest => dest.Schedule.Trainer)
                .ToListAsync())
                .GroupBy(s => s.Schedule.StartTime)
                .ToDictionary(g => g.Key, g => g.OrderBy(s => s.Schedule.Branch, new BranchComparer()).ToList());

            Assembly reportAssembly = Assembly.Load("Services");
            string fullAssemblyPath = string.Format("Services.Report.Template.WeeklySchedule.xlsx");

            using (Stream templateResource = reportAssembly.GetManifestResourceStream(fullAssemblyPath))
            {
                //string filename = "C:\\schedule.xls";
                var workbook = new XSSFWorkbook(templateResource);
                var sheet = workbook.GetSheet("WeeklySchedule");

                var dateRow = sheet.GetRow(DATE_ROW);
                for (int i = 1; i <= 7; i++)
                {
                    dateRow.Cells[i].SetCellValue(rq.Start.AddDays(i - 1).ToString("dd/MM/yyyy"));
                }

                var timeRowIndex = TIME_ROW_BEGIN;
                while (true)
                {
                    var timeRow = sheet.GetRow(timeRowIndex);
                    if (timeRow == null)
                    {
                        break;
                    }

                    var timeCell = timeRow.GetCell(0);
                    if (timeCell == null)
                    {
                        break;
                    }

                    if (string.IsNullOrEmpty(timeCell.StringCellValue))
                    {
                        break;
                    }

                    TimeSpan startTime = TimeSpan.Parse(timeCell.StringCellValue.Split(new string[] { " - " }, StringSplitOptions.None)[0]);

                    timeRowIndex++;
                    if (!sessionGroups.ContainsKey(startTime))
                    {
                        continue;
                    }

                    List<ScheduleDetailDTO> sessions = sessionGroups[startTime].Where(s => s.IsAddedToSheet == false).ToList();
                    for (int i = 1; i <= 7; i++)
                    {
                        var session = sessions.FirstOrDefault(s => (int)s.Date.DayOfWeek == i || (i == 7 && (int)s.Date.DayOfWeek == 0));
                        if (session == null)
                        {
                            continue;
                        }

                        var sessionCell = timeRow.GetCell(i);
                        string className = session.Schedule.Class.Name;
                        string classBranch = className + " - " + session.Schedule.Branch;
                        string song = "♪ " + session.Schedule.Song;
                        string numberOfSessions = $"Buổi {session.SessionNo}/{session.Schedule.Sessions}";

                        var classBranchFont = (XSSFFont)workbook.CreateFont();
                        classBranchFont.FontName = "Times New Roman";
                        classBranchFont.FontHeightInPoints = 18;
                        classBranchFont.IsBold = true;
                        if (session.SessionNo == 1)
                        {
                            classBranchFont.SetColor(new XSSFColor(new byte[] { 192, 0, 0 }));
                        }

                        var songFont = workbook.CreateFont();
                        songFont.FontName = "Times New Roman";
                        songFont.FontHeightInPoints = 18;
                        songFont.IsBold = false;

                        IRichTextString formattedCellContent;
                        bool isYoga = string.Equals(className, "Yoga", StringComparison.CurrentCultureIgnoreCase);
                        bool isCardioDance = string.Equals(className, "Cardio Dance", StringComparison.CurrentCultureIgnoreCase);
                        if (isYoga || isCardioDance)
                        {
                            if (isYoga)
                            {
                                if (classBranch.Contains("Q3"))
                                {
                                    classBranch += ", LVS";
                                }
                                else if (classBranch.Contains("LVS"))
                                {
                                    classBranch += ", Q3";
                                }
                            }
                            else
                            {
                                classBranch = className;
                            }

                            formattedCellContent = new XSSFRichTextString(classBranch);
                            formattedCellContent.ApplyFont(0, classBranch.Length, classBranchFont);
                        }
                        else
                        {
                            formattedCellContent = new XSSFRichTextString(classBranch + "\n" + song + "\n" + numberOfSessions);
                            formattedCellContent.ApplyFont(0, classBranch.Length, classBranchFont);
                            formattedCellContent.ApplyFont(classBranch.Length + 1, (classBranch + "\n" + song).Length, songFont);
                            formattedCellContent.ApplyFont((classBranch + "\n" + song).Length + 1, (classBranch + "\n" + song + "\n" + numberOfSessions).Length, classBranchFont);
                        }

                        sessionCell.SetCellValue(formattedCellContent);

                        var cellStyle = (XSSFCellStyle)workbook.CreateCellStyle();

                        cellStyle.WrapText = true;
                        cellStyle.Alignment = HorizontalAlignment.Center;
                        cellStyle.VerticalAlignment = VerticalAlignment.Center;
                        cellStyle.FillForegroundXSSFColor = GetBackgroundColor(session.Schedule.Branch);
                        cellStyle.FillPattern = FillPattern.SolidForeground;
                        cellStyle.BorderBottom = BorderStyle.Thin;
                        cellStyle.BorderLeft = BorderStyle.Thin;
                        cellStyle.BorderTop = BorderStyle.Thin;
                        cellStyle.BorderRight = BorderStyle.Thin;
                       
                        sessionCell.CellStyle = cellStyle;

                        session.IsAddedToSheet = true;
                    }
                }

                using (MemoryStream exportData = new MemoryStream())
                {
                    workbook.Write(exportData);
                    string saveAsFileName = string.Format("Schdule-{0:d}.xlsx", DateTime.Now).Replace("/", "-");
                    var rs = new WeeklyScheduleReportRs();
                    rs.FileName = saveAsFileName;
                    rs.ByteArray = exportData.ToArray();
                    return rs;
                }
            }
        }

        public XSSFColor GetBackgroundColor(string branch)
        {
            byte[] rgb;
            if (branch == "Q3")
            {
                rgb = new byte[] { 155, 194, 230 };
            }
            else if (branch == "LVS")
            {
                rgb = new byte[] { 255, 217, 102 };
            }
            else
            {
                rgb = new byte[] { 234, 209, 220 };
            }
            return new XSSFColor(rgb);
        }
    }

    public class BranchComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y) return 0;

            if (x == "LVS") return -1;
            if (y == "LVS") return 1;

            if (x == "Q3") return -1;
            if (y == "Q3") return 1;

            return 1;
        }
    }
}
