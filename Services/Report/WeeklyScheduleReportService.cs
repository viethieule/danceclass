using AutoMapper;
using AutoMapper.QueryableExtensions;
using DataAccess;
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
            List<ScheduleDetailDTO> dtos = await _dbContext.ScheduleDetails
                .Where(x => x.Date <= rq.End && x.Date >= rq.Start)
                .ProjectTo<ScheduleDetailDTO>(_mappingConfig, dest => dest.Schedule.Class, dest => dest.Schedule.Trainer)
                .ToListAsync();

            List<WeeklyScheduleDTO> listSession = dtos
                .GroupBy(s => new { s.TimeOfDay, s.Schedule.Branch, s.Schedule.StartTime })
                .Aggregate(new List<WeeklyScheduleDTO>(), (result, group) =>
                {
                    var dayGroups = group.GroupBy(s => s.Date.DayOfWeek);

                    if (dayGroups.Any(g => g.Count() > 1))
                    {
                        int numberSessions = group.Count();
                        int count = 0;
                        int skip = 0;

                        while (true)
                        {
                            var sessions = new List<ScheduleDetailDTO>();

                            foreach (IGrouping<DayOfWeek, ScheduleDetailDTO> dayGroup in dayGroups)
                            {
                                if (skip >= dayGroup.Count())
                                {
                                    continue;
                                }

                                ScheduleDetailDTO session = dayGroup.Skip(skip).FirstOrDefault();
                                if (session != null)
                                {
                                    sessions.Add(session);
                                }
                            }

                            result.Add(new WeeklyScheduleDTO 
                            {
                                Branch = group.Key.Branch, TimeOfDay = group.Key.TimeOfDay, TimeStart = group.Key.StartTime, Sessions = sessions 
                            });

                            count += sessions.Count();
                            if (count == numberSessions)
                            {
                                break;
                            }

                            skip++;
                        }
                    }
                    else
                    {
                        result.Add(new WeeklyScheduleDTO
                        {
                            Branch = group.Key.Branch, TimeOfDay = group.Key.TimeOfDay, TimeStart = group.Key.StartTime, Sessions = group.ToList() 
                        });
                    }

                    return result;
                })
                .OrderBy(dto => dto.TimeOfDay)
                    .ThenBy(dto => dto.Branch, new BranchComparer())
                    .ThenBy(dto => dto.TimeStart)
                .ToList();

            Assembly reportAssembly = Assembly.Load("Services");
            string fullAssemblyPath = string.Format("Services.Report.Template.WeeklySchedule.xlsx");

            using (Stream templateResource = reportAssembly.GetManifestResourceStream(fullAssemblyPath))
            {
                var workbook = new XSSFWorkbook(templateResource);
                var sheet = workbook.GetSheet("WeeklySchedule");

                var dateRow = sheet.GetRow(DATE_ROW);
                for (int i = 1; i <= 7; i++)
                {
                    dateRow.Cells[i].SetCellValue(rq.Start.AddDays(i - 1).ToString("dd/MM/yyyy"));
                }

                for (int i = 1; i < listSession.Count; i++)
                {
                    sheet.CopyRow(TIME_ROW_BEGIN, TIME_ROW_BEGIN + i);
                }

                for (int i = 0; i < listSession.Count; i++)
                {
                    WeeklyScheduleDTO dto = listSession[i];

                    IRow row = sheet.GetRow(TIME_ROW_BEGIN + i);
                    if (row == null) break;

                    ICell timeCell = row.GetCell(0);
                    timeCell.SetCellValue(dto.TimeStart.ToString(@"hh\:mm") + " - " + dto.TimeEnd.ToString(@"hh\:mm"));

                    foreach (ScheduleDetailDTO session in dto.Sessions)
                    {
                        DayOfWeek dayOfWeek = session.Date.DayOfWeek;
                        int cellIndex = dayOfWeek != DayOfWeek.Sunday ? (int)dayOfWeek : 7;

                        ICell sessionCell = row.GetCell(cellIndex);

                        IRichTextString formattedCellContent = BuildFormattedSessionCellContent(workbook, session);
                        sessionCell.SetCellValue(formattedCellContent);

                        XSSFCellStyle cellStyle = BuidlSessionCellStyle(workbook, session);
                        sessionCell.CellStyle = cellStyle;
                    }
                }

                using (MemoryStream exportData = new MemoryStream())
                {
                    workbook.Write(exportData);
                    string saveAsFileName = string.Format("Schedule-{0:d}.xlsx", DateTime.Now).Replace("/", "-");
                    var rs = new WeeklyScheduleReportRs
                    {
                        FileName = saveAsFileName,
                        ByteArray = exportData.ToArray()
                    };

                    return rs;
                }
            }
        }

        private XSSFCellStyle BuidlSessionCellStyle(XSSFWorkbook workbook, ScheduleDetailDTO session)
        {
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

            return cellStyle;
        }

        private IRichTextString BuildFormattedSessionCellContent(XSSFWorkbook workbook, ScheduleDetailDTO session)
        {
            string className = session.Schedule.Class.Name;
            string classBranch = className + " - " + session.Schedule.Branch;
            string song = "♪ " + session.Schedule.Song;
            string numberOfSessions = $"Buổi {session.SessionNo}/{session.Schedule.Sessions}";

            XSSFFont classBranchFont = (XSSFFont)workbook.CreateFont();
            classBranchFont.FontName = "Times New Roman";
            classBranchFont.FontHeightInPoints = 18;
            classBranchFont.IsBold = true;
            if (session.SessionNo == 1)
            {
                classBranchFont.SetColor(new XSSFColor(new byte[] { 192, 0, 0 }));
            }

            IFont songFont = workbook.CreateFont();
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

            return formattedCellContent;
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

        class BranchComparer : IComparer<string>
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
}
