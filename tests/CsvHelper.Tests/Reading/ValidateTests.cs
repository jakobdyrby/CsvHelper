﻿// Copyright 2009-2022 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Configuration;
using Xunit;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace CsvHelper.Tests.Reading
{
	
	public class ValidateTests
	{
		[Fact]
		public void ValidateTest()
		{
			var config = new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				MissingFieldFound = null,
			};
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			using (var reader = new StreamReader(stream))
			using (var csv = new CsvReader(reader, config))
			{
				writer.WriteLine("Id,Name");
				writer.WriteLine(",one");
				writer.Flush();
				stream.Position = 0;

				csv.Context.RegisterClassMap<ValidateMap>();
				Assert.Throws<FieldValidationException>(() => csv.GetRecords<Test>().ToList());
			}
		}

		[Fact]
		public void LogInsteadTest()
		{
			var config = new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				MissingFieldFound = null,
			};
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			using (var reader = new StreamReader(stream))
			using (var csv = new CsvReader(reader, config))
			{
				writer.WriteLine("Id,Name");
				writer.WriteLine("1,");
				writer.Flush();
				stream.Position = 0;

				var logger = new StringBuilder();
				csv.Context.RegisterClassMap(new LogInsteadMap(logger));
				csv.GetRecords<Test>().ToList();

				var expected = new StringBuilder();
				expected.AppendLine("Field '' is not valid!");

				Assert.Equal(expected.ToString(), logger.ToString());
			}
		}

		[Fact]
		public void CustomExceptionTest()
		{
			var config = new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				MissingFieldFound = null,
			};
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			using (var reader = new StreamReader(stream))
			using (var csv = new CsvReader(reader, config))
			{
				writer.WriteLine("Id,Name");
				writer.WriteLine(",one");
				writer.Flush();
				stream.Position = 0;

				csv.Context.RegisterClassMap<CustomExceptionMap>();
				Assert.Throws<CustomException>(() => csv.GetRecords<Test>().ToList());
			}
		}

		private class Test
		{
			public int Id { get; set; }

			public string Name { get; set; }
		}

		private sealed class ValidateMap : ClassMap<Test>
		{
			public ValidateMap()
			{
				Map(m => m.Id).Validate(args => !string.IsNullOrEmpty(args.Field));
				Map(m => m.Name);
			}
		}

		private sealed class LogInsteadMap : ClassMap<Test>
		{
			public LogInsteadMap(StringBuilder logger)
			{
				Map(m => m.Id);
				Map(m => m.Name).Validate(args =>
			 {
				 var isValid = !string.IsNullOrEmpty(args.Field);
				 if (!isValid)
				 {
					 logger.AppendLine($"Field '{args.Field}' is not valid!");
				 }

				 return true;
			 });
			}
		}

		private sealed class CustomExceptionMap : ClassMap<Test>
		{
			public CustomExceptionMap()
			{
				Map(m => m.Id).Validate(field => throw new CustomException());
				Map(m => m.Name);
			}
		}

		private class CustomException : CsvHelperException
		{
		}
	}
}
