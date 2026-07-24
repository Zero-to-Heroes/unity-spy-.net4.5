namespace HackF5.UnitySpy.Offsets
{
    using System;
    using System.Linq;

    public struct UnityVersion
    {
        public static readonly UnityVersion Version2018_4_10 = new UnityVersion(2018, 4, 10);
        public static readonly UnityVersion Version2019_4_5 = new UnityVersion(2019, 4, 5);
        public static readonly UnityVersion Version2020_3_13 = new UnityVersion(2020, 3, 13);

        public UnityVersion(int year, int versionWithinYear, int subversionWithinYear)
        {
            this.Year = year;
            this.VersionWithinYear = versionWithinYear;
            this.SubversionWithinYear = subversionWithinYear;
        }

        public int Year { get; }

        public int VersionWithinYear { get; }

        public int SubversionWithinYear { get; }

        public static bool operator ==(UnityVersion a, UnityVersion b) => a.Equals(b);

        public static bool operator !=(UnityVersion a, UnityVersion b) => !(a == b);

        public static UnityVersion Parse(string version)
        {
            if (version == null)
            {
                throw new ArgumentNullException("version paramenter cannot be null");
            }

            // Tolerates partial versions like "6000.3" (missing segments default to 0) and
            // non-numeric suffixes like "62f1".
            string[] versionSplit = version.Split('.');
            int year = ParseLeadingDigits(versionSplit[0]);
            int versionWithinYear = versionSplit.Length > 1 ? ParseLeadingDigits(versionSplit[1]) : 0;
            int subversionWithinYear = versionSplit.Length > 2 ? ParseLeadingDigits(versionSplit[2]) : 0;
            return new UnityVersion(year, versionWithinYear, subversionWithinYear);
        }

        private static int ParseLeadingDigits(string segment)
        {
            string digits = new string(segment.TakeWhile(char.IsDigit).ToArray());
            return digits.Length > 0 ? int.Parse(digits) : 0;
        }

        public int CompareTo(UnityVersion other)
        {
            if (this.Year != other.Year)
            {
                return this.Year.CompareTo(other.Year);
            }

            if (this.VersionWithinYear != other.VersionWithinYear)
            {
                return this.VersionWithinYear.CompareTo(other.VersionWithinYear);
            }

            return this.SubversionWithinYear.CompareTo(other.SubversionWithinYear);
        }

        public override bool Equals(object obj)
        {
            if (obj is UnityVersion other)
            {
                return other.Year == this.Year &&
                        other.VersionWithinYear == this.VersionWithinYear &&
                        other.SubversionWithinYear == this.SubversionWithinYear;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = (hash * 27) + this.Year.GetHashCode();
            hash = (hash * 23) + this.VersionWithinYear.GetHashCode();
            hash = (hash * 13) + this.SubversionWithinYear.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return this.Year + "." + this.VersionWithinYear + "." + this.SubversionWithinYear;
        }
    }
}