using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentQueue.Domain.Commons
{
    public sealed class Shift
    {
        public static readonly Shift Morning = new Shift("Morning Shift", new TimeOnly(06, 00), new TimeOnly(13, 59));
        public static readonly Shift Afternoon = new Shift("Afternoon Shift", new TimeOnly(14, 00), new TimeOnly(21, 59));
        public static readonly Shift Night = new Shift("Night Shift", new TimeOnly(22, 00), new TimeOnly(05, 59));
        public static readonly Shift Overflow = new Shift("Overflow Shift", new TimeOnly(00, 00), new TimeOnly(23, 59));
        public string Name { get; }

        public Time Time { get; }
        public Shift(string name, TimeOnly start, TimeOnly end) 
        {
            Name = name;
            Time = new Time(start, end);
        }

        public override string ToString() => Name;


        private static Shift[] Schedule = { Shift.Morning, Shift.Afternoon, Shift.Night, Shift.Overflow };
        public static Shift? GetShiftBySchedule(DateTime schedule)
        {
            var timeOnly = TimeOnly.FromDateTime(schedule);

            var shift = Schedule.FirstOrDefault(s =>
            (s.Time.Start <= timeOnly && s.Time.End > timeOnly) ||
            (s.Time.Start > s.Time.End && (timeOnly >= s.Time.Start || timeOnly < s.Time.End)) &&
            s.Name != Shift.Overflow.Name);

            return shift;
        }
    }
}
