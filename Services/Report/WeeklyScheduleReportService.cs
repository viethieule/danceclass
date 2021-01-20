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
            DateTime start = rq.Start;
            DateTime end = rq.End;

            var sessionGroups = (await _dbContext.ScheduleDetails
                .Where(x => x.Date <= rq.End && x.Date >= rq.Start)
                .ProjectTo<ScheduleDetailDTO>(_mappingConfig, dest => dest.Schedule.Class, dest => dest.Schedule.Trainer)
                .ToListAsync())
                .GroupBy(s => s.Schedule.StartTime)
                .ToDictionary(g => g.Key, g => g.OrderBy(s => s.Schedule.Branch, new BranchComparer()).ToList());

            var dtos = new List<WeeklyScheduleDTO>();
            foreach (var kv in sessionGroups)
            {
                var dayGroups = kv.Value.GroupBy(s => s.Date.DayOfWeek).Select(g => new { Day = g.Key, Sessions = g.OrderBy(s => s.Schedule.Branch, new BranchComparer()) });
                if (dayGroups.Any(g => g.Sessions.Count() > 1))
                {
                    int numberSessions = kv.Value.Count();
                    int count = 0;
                    while (true)
                    {
                        var sessions = new List<ScheduleDetailDTO>();

                        foreach (var dayGroup in dayGroups)
                        {
                            var session = dayGroup.Sessions.FirstOrDefault(d => d.IsAdded == false);
                            if (session != null)
                            {
                                sessions.Add(session);
                                session.IsAdded = true;
                            }
                        }

                        WeeklyScheduleDTO dto = new WeeklyScheduleDTO
                        {
                            TimeStart = kv.Key, Sessions = sessions
                        };

                        dtos.Add(dto);

                        count += sessions.Count();
                        if (count == numberSessions)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    WeeklyScheduleDTO dto = new WeeklyScheduleDTO
                    {
                        TimeStart = kv.Key,
                        Sessions = kv.Value
                    };

                    dtos.Add(dto);
                }
            }

            //var test2 = dtos
            //    .GroupBy(
            //        d => d.TimeStart, 
            //        d => d, 
            //        (key, dto) => new WeeklyScheduleDTOWrapper
            //        {
            //            FirstTimeStartOccurence = key,
            //            Data = dto
            //                .GroupBy(s => s.TimeStart)
            //                .Select(s => new AnotherWeeklyScheduleDTO
            //                {
            //                    Time = s.Key,
            //                    Count = s.Count(),
            //                    Sessions = s.ToList()
            //                })
            //                .OrderBy(s => s.Time)
            //                .ToList()
            //        },
            //        new TimeStartEqualityComparer())
            //    .OrderBy(d => d.FirstTimeStartOccurence)
            //    .ToList();

            //var list = new List<WeeklyScheduleDTO>();
            //foreach (WeeklyScheduleDTOWrapper group in test2)
            //{
            //    int totalWs = group.Data.Sum(d => d.Count);
            //    var subList = new List<WeeklyScheduleDTO>();
            //    int skip = 0;
            //    while (true)
            //    {
            //        foreach (AnotherWeeklyScheduleDTO item in group.Data)
            //        {
            //            if (skip < item.Sessions.Count)
            //            {
            //                WeeklyScheduleDTO ws = item.Sessions.Skip(skip).FirstOrDefault();
            //                if (ws != null)
            //                {
            //                    subList.Add(ws);
            //                }
            //            }
            //        }

            //        if (subList.Count == totalWs)
            //        {
            //            break;
            //        }

            //        skip++;
            //    }

            //    list.AddRange(subList);
            //}

            var list = dtos.OrderBy(d => d.TimeStart).ToList();

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

                for (int i = 1; i < list.Count; i++)
                {
                    sheet.CopyRow(TIME_ROW_BEGIN, TIME_ROW_BEGIN + i);
                }

                for (int i = 0; i < list.Count; i++)
                {
                    WeeklyScheduleDTO dto = list[i];

                    IRow row = sheet.GetRow(TIME_ROW_BEGIN + i);
                    if (row == null) break;

                    ICell timeCell = row.GetCell(0);
                    timeCell.SetCellValue(dto.TimeStart.ToString(@"hh\:mm") + " - " + dto.TimeEnd.ToString(@"hh\:mm"));

                    foreach (ScheduleDetailDTO session in dto.Sessions)
                    {
                        DayOfWeek dayOfWeek = session.Date.DayOfWeek;
                        int cellIndex = dayOfWeek != DayOfWeek.Sunday ? (int)dayOfWeek : 7;

                        ICell sessionCell = row.GetCell(cellIndex);

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

        class WeeklyScheduleDTO
        {
            public TimeSpan TimeStart { get; set; }
            public TimeSpan TimeEnd
            {
                get
                {
                    return TimeStart.Add(new TimeSpan(1, 0, 0));
                }
            }
            public List<ScheduleDetailDTO> Sessions { get; set; }
        }

        class AnotherWeeklyScheduleDTO
        {
            public TimeSpan Time { get; set; }
            public int Count { get; set; }
            public List<WeeklyScheduleDTO> Sessions { get; set; }
        }

        class WeeklyScheduleDTOWrapper
        {
            public TimeSpan FirstTimeStartOccurence { get; set; }
            public List<AnotherWeeklyScheduleDTO> Data { get; set; }
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

        class TimeStartEqualityComparer : IEqualityComparer<TimeSpan>
        {
            public bool Equals(TimeSpan x, TimeSpan y)
            {
                return (x.Hours < 12 && y.Hours < 12) ||
                    (x.Hours == 12 && y.Hours == 12) ||
                    (x.Hours > 12 && y.Hours > 12 && x.Hours < 17 && y.Hours < 17) ||
                    (x.Hours >= 17 && y.Hours >= 17);
            }

            public int GetHashCode(TimeSpan time)
            {
                if (time == null) return 0;

                int hash = 17;
                int hours = time.Hours;
                if (hours < 12) hash = hash * 23 + TimesOfDay.Beforenoon.GetHashCode();
                else if (hours == 12) hash = hash * 23 + TimesOfDay.Noon.GetHashCode();
                else if (hours > 12 && hours < 17) hash = hash * 23 + TimesOfDay.Afternoon.GetHashCode();
                else if (hours >= 17) hash = hash * 23 + TimesOfDay.Evening.GetHashCode();

                return hash;
            }
        }

        enum TimesOfDay
        {
            Beforenoon = 8,
            Noon = 12,
            Afternoon = 13,
            Evening = 17
        }
    }
}
