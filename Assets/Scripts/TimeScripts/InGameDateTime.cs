using UnityEngine;

namespace Assets.Scripts.TimeScripts
{
    public struct InGameDateTime
    {
        private static readonly string[] MonthNames =
        {
            "March",
            "April",
            "May",
            "June",
            "July",
            "August",
            "September",
            "October",
            "November",
            "December",
            "January",
            "February",
        };

        private float FloatSeconds { get; set; }
        public int Seconds => (int)FloatSeconds;
        public int Minutes { get; private set; }
        public int Hours { get; private set; }
        public int Days { get; private set; }
        public int Months { get; private set; }
        public int Years { get; private set; }

        public InGameDateTime(float seconds, int minutes, int hours, int days, int months, int years)
        {
            var remainderMinutes = (int)(seconds / 60);
            FloatSeconds = seconds % 60;

            var remainderHours = (minutes + remainderMinutes) / 60;
            Minutes = (minutes + remainderMinutes) % 60;

            var remainderDays = (hours + remainderHours) / 24;
            Hours = (hours + remainderHours) % 24;

            var remainderMonths = (days + remainderDays) / 30;
            Days = (days + remainderDays) % 30;

            var remainderYears = (months + remainderMonths) / 12;
            Months = (months + remainderMonths) % 12;

            Years = years + remainderYears;
        }

        private string FormatDay()
        {
            var day = Days + 1;
            var lastDigit = day % 10;
            var suffix = "th";
            switch (lastDigit)
            {
                case 1:
                    suffix = "st";
                    break;
                case 2:
                    suffix = "nd";
                    break;
                case 3:
                    suffix = "rd";
                    break;
            }
            return day + suffix;
        }

        public string FormatDate()
        {
            return $"{FormatDay()} of {MonthNames[Months]}, {Years}";
        }

        public string FormatTime()
        {
            return $"{Hours}:{Minutes:00}:{Seconds:00}";
        }

        public InGameDateTime AddDelta(float inGameDelta)
        {
            return new InGameDateTime(FloatSeconds + inGameDelta, Minutes, Hours, Days, Months, Years);
        }

        public float GetSunInclination() //Inclination is from 70 in summer, down to 20 in winter
        {
            //month 4 should have 70 inclination
            //month 10 should have 20 inclination
            //Let's center at month 10, and add 50 linearly from there
            var totalMonths = Months + (Days + (Hours + (Minutes + (FloatSeconds / 60)) / 60f) / 24f) / 30f;
            var monthsCenteredAtWinter = (totalMonths + 2) % 12;
            monthsCenteredAtWinter = Mathf.Abs(6 - monthsCenteredAtWinter);
            return 20 + 50 * (monthsCenteredAtWinter / 6);
        }

        public Vector3 GetSunPosition()// 12:00:00 is noon
        {
            var sunInclination = GetSunInclination();

            var totalHours = Hours + (Minutes + (FloatSeconds / 60)) / 60f;
            var totalHoursCenteredAtNoon = (totalHours + 18) % 24;
            var totalHoursCenteredAtNoonScaledTo360 = totalHoursCenteredAtNoon * 15;

            var sinTime = Mathf.Sin(totalHoursCenteredAtNoonScaledTo360 * Mathf.Deg2Rad);
            var cosTime = Mathf.Cos(totalHoursCenteredAtNoonScaledTo360 * Mathf.Deg2Rad);

            var sinInclination = Mathf.Sin(sunInclination * Mathf.Deg2Rad);
            var cosInclination = Mathf.Cos(sunInclination * Mathf.Deg2Rad);

            return new Vector3(cosInclination * cosTime, cosInclination * sinTime, sinInclination);
        }

        public override bool Equals(object obj)
        {
            return obj is InGameDateTime b && this == b;
        }

        public static bool operator ==(InGameDateTime left, InGameDateTime right)
        {
            return left.FloatSeconds == right.FloatSeconds &&
                left.Minutes == right.Minutes &&
                left.Hours == right.Hours &&
                left.Days == right.Days &&
                left.Months == right.Months &&
                left.Years == right.Years;
        }

        public static bool operator !=(InGameDateTime left, InGameDateTime right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = FloatSeconds.GetHashCode();
                hash = hash * 13 + Minutes.GetHashCode();
                hash = hash * 13 + Hours.GetHashCode();
                hash = hash * 13 + Days.GetHashCode();
                hash = hash * 13 + Months.GetHashCode();
                hash = hash * 13 + Years.GetHashCode();
                return hash;
            }
        }
    }
}