﻿using DataAccess.Enums;
using Services.Members;
using Services.Schedule;
using System;

namespace Services.Registration
{
    public class RegistrationDTO
    {
        public int Id { get; set; }
        public int ScheduleDetailId { get; set; }
        public ScheduleDetailDTO ScheduleDetail { get; set; }
        public int UserId { get; set; }
        public MemberDTO User { get; set; }
        public DateTime DateRegistered { get; set; }
        public RegistrationStatus Status { get; set; }
    }
}
