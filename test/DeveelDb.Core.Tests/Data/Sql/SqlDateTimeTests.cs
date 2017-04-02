using System;

using Xunit;

namespace Deveel.Data.Sql {
	public static class SqlDateTimeTests {
		[Theory]
		[InlineData(2012, 12, 01, 07, 16, 22, 556, 2, 0)]
		public static void FromFullForm(int year, int month, int day, int hour, int minute, int second, int millis, int offsetHour,
			int offsetMinute) {
			var offset = new SqlDayToSecond(offsetHour, offsetMinute, 0);
			var date = new SqlDateTime(year, month, day, hour, minute, second, millis, offset);

			Assert.Equal(year, date.Year);
			Assert.Equal(month, date.Month);
			Assert.Equal(day, date.Day);
			Assert.Equal(hour, date.Hour);
			Assert.Equal(minute, date.Minute);
			Assert.Equal(second, date.Second);
			Assert.Equal(millis, date.Millisecond);
			Assert.Equal(offset, date.Offset);
		}

		[Theory]
		[InlineData(2012, 12, 01, 07, 16, 22, 556)]
		public static void FromMediumForm(int year, int month, int day, int hour, int minute, int second, int millis) {
			var date = new SqlDateTime(year, month, day, hour, minute, second, millis);

			Assert.Equal(year, date.Year);
			Assert.Equal(month, date.Month);
			Assert.Equal(day, date.Day);
			Assert.Equal(hour, date.Hour);
			Assert.Equal(minute, date.Minute);
			Assert.Equal(second, date.Second);
			Assert.Equal(millis, date.Millisecond);
			Assert.False(date.Offset.IsNull);
			Assert.Equal(0, date.Offset.Hours);
			Assert.Equal(0, date.Offset.Minutes);
		}

		[Theory]
		[InlineData(2012, 12, 01, 07, 16, 22, 556, 8, 0)]
		public static void FullFormToBytes(int year, int month, int day, int hour, int minute, int second, int millis,
			int offsetHour,
			int offsetMinute) {
			var offset = new SqlDayToSecond(offsetHour, offsetMinute, 0);
			var date = new SqlDateTime(year, month, day, hour, minute, second, millis, offset);

			var bytes = date.ToByteArray(true);

			Assert.NotNull(bytes);
			Assert.Equal(13, bytes.Length);

			var back = new SqlDateTime(bytes);

			Assert.Equal(date, back);
		}

		[Theory]
		[InlineData(2012, 12, 01, 07, 16, 22, 556)]
		public static void MediumFormToBytes(int year, int month, int day, int hour, int minute, int second, int millis) {
			var date = new SqlDateTime(year, month, day, hour, minute, second, millis);

			var bytes = date.ToByteArray();

			Assert.NotNull(bytes);
			Assert.Equal(11, bytes.Length);

			var back = new SqlDateTime(bytes);

			Assert.Equal(date, back);
		}

		[Theory]
		[InlineData("2014-04-11T02:19:13.334 +02:30", 2014, 04, 11, 02, 19, 13, 334, 02, 30, true)]
		public static void TryParseFull(string s, int year, int month, int day, int hour, int minute, int second, int millis,
			int offsetHour,
			int offsetMinute, bool expected) {
			SqlDateTime date;
			Assert.Equal(expected, SqlDateTime.TryParse(s,out date));

			Assert.False(date.IsNull);
			Assert.Equal(year, date.Year);
			Assert.Equal(month, date.Month);
			Assert.Equal(day, date.Day);
			Assert.Equal(hour, date.Hour);
			Assert.Equal(minute, date.Minute);
			Assert.Equal(second, date.Second);
			Assert.Equal(millis, date.Millisecond);
			Assert.Equal(offsetHour, date.Offset.Hours);
			Assert.Equal(offsetMinute, date.Offset.Minutes);
		}

		[Theory]
		[InlineData("2011-02-15", "2012-03-22", false)]
		public static void DatesEqual(string d1, string d2, bool expected) {
			var date1 = SqlDateTime.Parse(d1);
			var date2 = SqlDateTime.Parse(d2);

			Assert.Equal(expected, date1.Equals(date2));
		}
	}
}

