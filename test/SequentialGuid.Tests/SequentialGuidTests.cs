﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.Linq;
using Xunit;

namespace SequentialGuid.Tests;

public class SequentialGuidTests
{
	private const long EpochTicks = 621355968000000000;

	/// <summary>
	///     Properly sequenced Guid array
	/// </summary>
	private IReadOnlyList<Guid> SortedGuidList { get; } =
		new ReadOnlyCollection<Guid>(new List<Guid>
		{
			new("00000000-0000-0000-0000-000000000001"),
			new("00000000-0000-0000-0000-000000000100"),
			new("00000000-0000-0000-0000-000000010000"),
			new("00000000-0000-0000-0000-000001000000"),
			new("00000000-0000-0000-0000-000100000000"),
			new("00000000-0000-0000-0000-010000000000"),
			new("00000000-0000-0000-0001-000000000000"),
			new("00000000-0000-0000-0100-000000000000"),
			new("00000000-0000-0001-0000-000000000000"),
			new("00000000-0000-0100-0000-000000000000"),
			new("00000000-0001-0000-0000-000000000000"),
			new("00000000-0100-0000-0000-000000000000"),
			new("00000001-0000-0000-0000-000000000000"),
			new("00000100-0000-0000-0000-000000000000"),
			new("00010000-0000-0000-0000-000000000000"),
			new("01000000-0000-0000-0000-000000000000")
		});

	/// <summary>
	///     Properly sequenced SqlGuid array
	/// </summary>
	private IReadOnlyList<SqlGuid> SortedSqlGuidList { get; } =
		new ReadOnlyCollection<SqlGuid>(new List<SqlGuid>
		{
			new("01000000-0000-0000-0000-000000000000"),
			new("00010000-0000-0000-0000-000000000000"),
			new("00000100-0000-0000-0000-000000000000"),
			new("00000001-0000-0000-0000-000000000000"),
			new("00000000-0100-0000-0000-000000000000"),
			new("00000000-0001-0000-0000-000000000000"),
			new("00000000-0000-0100-0000-000000000000"),
			new("00000000-0000-0001-0000-000000000000"),
			new("00000000-0000-0000-0001-000000000000"),
			new("00000000-0000-0000-0100-000000000000"),
			new("00000000-0000-0000-0000-000000000001"),
			new("00000000-0000-0000-0000-000000000100"),
			new("00000000-0000-0000-0000-000000010000"),
			new("00000000-0000-0000-0000-000001000000"),
			new("00000000-0000-0000-0000-000100000000"),
			new("00000000-0000-0000-0000-010000000000")
		});

	[Fact]
	private void TestGuidSorting()
	{
		//Act
		var sortedList = SortedGuidList.OrderBy(x => x);
		//Assert
		Assert.True(SortedGuidList.SequenceEqual(sortedList));
	}

	[Fact]
	private void TestSqlGuidSorting()
	{
		//Act
		var sortedList = SortedSqlGuidList.OrderBy(x => x);
		//Assert
		Assert.True(SortedSqlGuidList.SequenceEqual(sortedList));
	}

	[Fact]
	private void TestSequentialGuidNewGuid()
	{
		//Arrange
		var generator = SequentialGuidGenerator.Instance;
		var items = Enumerable.Range(0, 25).Select(i => new { Id = generator.NewGuid(), Sort = i });
		//Act
		var sortedItems = items.OrderBy(x => x.Id).ToList();
		//Assert
		for (var i = 0; i < sortedItems.Count; i++)
		{
			Assert.Equal(i, sortedItems[i].Sort);
		}
	}

	[Fact]
	private void TestSequentialGuidNewSqlGuid()
	{
		//Arrange
		var generator = SequentialSqlGuidGenerator.Instance;
		var items = Enumerable.Range(0, 25).Select(i => new { Id = new SqlGuid(generator.NewGuid()), Sort = i });
		//Act
		var sortedItems = items.OrderBy(x => x.Id).ToList();
		//Assert
		for (var i = 0; i < sortedItems.Count; i++)
		{
			Assert.Equal(i, sortedItems[i].Sort);
		}
	}

	[Fact]
	private void TestLocalDateIsUtcInGuid()
	{
		var localNow = DateTime.Now;
		TestLocalDateIsUtcInGuidImpl(localNow,
			SequentialGuidGenerator.Instance.NewGuid(localNow));
	}

	[Fact]
	private void TestLocalDateIsUtcInSqlGuid()
	{
		var localNow = DateTime.Now;
		TestLocalDateIsUtcInGuidImpl(localNow,
			SequentialSqlGuidGenerator.Instance.NewGuid(localNow));
	}

	// ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
	private static void TestLocalDateIsUtcInGuidImpl(DateTime localNow,
		Guid id)
	{
		// Act
		var utcDate = id.ToDateTime().GetValueOrDefault();
		// Assert
		Assert.Equal(DateTimeKind.Utc, utcDate.Kind);
		Assert.Equal(localNow, utcDate.ToLocalTime());
	}

	[Fact]
	private void TestSqlGuidToGuid()
	{
		// Act
		var sqlList = SortedSqlGuidList.Select(g => g.ToGuid());
		// Assert
		Assert.True(SortedGuidList.SequenceEqual(sqlList));
	}

	[Fact]
	private void TestGuidToSqlGuid()
	{
		// Act
		var guidList = SortedGuidList.Select(g => g.ToSqlGuid());
		// Assert
		Assert.True(SortedSqlGuidList.SequenceEqual(guidList));
	}

	[Fact]
	private void TestGuidToDateTimeIsUtc()
	{
		//Arrange
		var expectedDateTime = DateTime.UtcNow;
		//Act
		var dateTime = SequentialGuidGenerator.Instance
			.NewGuid(expectedDateTime)
			.ToDateTime()
			.GetValueOrDefault();
		//Assert
		Assert.Equal(expectedDateTime.Ticks, dateTime.Ticks);
		Assert.Equal(expectedDateTime.Kind, dateTime.Kind);
	}

	[Fact]
	private void TestGuidToDateTimeForNonSequentialGuidReturnsNull()
	{
		//Arrange
		var guid = Guid.NewGuid();
		//Act
		var actual = guid.ToDateTime();
		//Assert
		Assert.Null(actual);
	}

	[Fact]
	private void TestUtcNowDoesNotThrowException()
	{
		SequentialGuidGenerator.Instance.NewGuid(DateTime.UtcNow);
	}

	[Fact]
	private void TestLocalNowDoesNotThrowException()
	{
		SequentialGuidGenerator.Instance.NewGuid(DateTime.Now);
	}

	[Fact]
	private void TestUnixEpochDoesNotThrowException()
	{
		SequentialGuidGenerator.Instance.NewGuid(EpochTicks);
	}

	[Fact]
	private void TestBetweenUnixEpochAndNowDoesNotThrowException()
	{
		SequentialGuidGenerator.Instance.NewGuid(
			new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
	}

	[Fact]
	private void TestDateTimeKindUnspecifiedThrowsArgumentException()
	{
		TestThrowsArgumentException(new DateTime(2000, 1, 1));
	}

	[Fact]
	private void TestAfterNowThrowsArgumentException()
	{
		TestThrowsArgumentException(DateTime.UtcNow.AddSeconds(1));
	}

	[Fact]
	private void TestAfterNowReturnsNullDateTime()
	{
		TestReturnsNullDateTime(DateTime.UtcNow.AddSeconds(1).Ticks);
	}

	[Fact]
	private void TestBeforeUnixEpochThrowsArgumentException()
	{
		TestThrowsArgumentException(
			new DateTime(EpochTicks - 1));
	}

	[Fact]
	private void TestBeforeUnixEpochReturnsNullDateTime()
	{
		TestReturnsNullDateTime(EpochTicks - 1);
	}

	// Test the internal mechanism that bypasses date validation
	private static void TestReturnsNullDateTime(long ticks)
	{
		//Arrange
		var guid = SequentialGuidGenerator.Instance.NewGuid(ticks);
		var sqlGuid = SequentialSqlGuidGenerator.Instance.NewGuid(ticks);
		//Act & Assert
		Assert.Null(guid.ToDateTime());
		Assert.Null(sqlGuid.ToDateTime());
	}

	private static void TestThrowsArgumentException(DateTime timestamp)
	{
		Assert.Throws<ArgumentException>(() =>
			SequentialGuidGenerator.Instance.NewGuid(timestamp));
	}

	[Fact]
	private void TestSqlGuidToDateTime()
	{
		//Arrange
		var expectedDateTime = DateTime.UtcNow;
		//Act
		var dateTime = SequentialSqlGuidGenerator.Instance.NewGuid(expectedDateTime).ToDateTime()
			.GetValueOrDefault();
		//Assert
		Assert.Equal(expectedDateTime.Ticks, dateTime.Ticks);
	}

	[Fact]
	private void TestGuidBigDateRange()
	{
		//Arrange
		var generator = SequentialGuidGenerator.Instance;
		var items = new List<Guid>();
		//Act
		for (var i = 1970; i < DateTime.Today.Year; i++)
		{
			items.Add(generator.NewGuid(new DateTime(i, 1, 1, 0, 0, 0,
				DateTimeKind.Local)));
		}
		//Assert
		Assert.True(items.SequenceEqual(items.OrderBy(x => x)));
	}

	[Fact]
	private void TestSqlGuidBigDateRange()
	{
		//Arrange
		var generator = SequentialSqlGuidGenerator.Instance;
		var items = new List<SqlGuid>();
		//Act
		for (var i = 1970; i < DateTime.Today.Year; i++)
		{
			items.Add(generator.NewGuid(new DateTime(i, 1, 1, 0, 0, 0,
				DateTimeKind.Utc)));
		}

		//Assert
		Assert.True(items.SequenceEqual(items.OrderBy(x => x)));
	}

	[Fact]
	private void TestSqlGuidGenerator()
	{
		// Arrange
		var now = DateTime.UtcNow;
		// Act
		var stamp = SequentialSqlGuidGenerator.Instance.NewSqlGuid(now).ToDateTime();
		// Assert
		Assert.Equal(now, stamp);
	}
}
