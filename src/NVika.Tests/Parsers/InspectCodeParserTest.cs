﻿using NVika.Parsers;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Xunit;

namespace NVika.Tests.Parsers
{
    public class InspectCodeParserTest
    {
        public static IEnumerable<object[]> ParseXmlData
        {
            get
            {
                XDocument doc = XDocument.Parse(GetEmbeddedResourceContent("inspectcodereport.xml"));
                return new[] { new object[] { doc } };
            }
        }

        private static string GetEmbeddedResourceContent(string resourceName)
        {
            using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Concat("NVika.Tests.Data.", resourceName)))
            {
                using (StreamReader resourceStreamReader = new StreamReader(resourceStream))
                {
                    return resourceStreamReader.ReadToEnd();
                }
            }
        }

        [Fact]
        public void Name()
        {
            // arrange
            var parser = new InspectCodeParser(new MockFileSystem());

            // act
            var name = parser.Name;

            // assert
            Assert.Equal("InspectCode", name);
        }

        [Theory]
        [InlineData("<!-- Generated by InspectCode 8.2.1000.4527 --><root></root>", true)]
        [InlineData("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<!-- Generated by InspectCode 8.2.1000.4527 --><root></root>", true)]
        [InlineData("<Report ToolsVersion=\"8.2\"></Report>", false)]
        [InlineData("<IssueTypes></IssueTypes>", false)]
        public void CanParse(string xmlContent, bool expectedResult)
        {
            // arrange
            var doc = XDocument.Parse(xmlContent);
            var parser = new InspectCodeParser(new MockFileSystem());

            // act
            var result = parser.CanParse(doc);

            // assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData("ParseXmlData")]
        public void Parse(XDocument report)
        {
            // arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"NVika\BuildServers\AppVeyor.cs", new MockFileData(GetEmbeddedResourceContent("AppVeyor.txt")) },
                { @"NVika\BuildServers\LocalBuildServer.cs", new MockFileData(GetEmbeddedResourceContent("LocalBuildServer.txt")) },
                { @"NVika\ParseReportCommand.cs", new MockFileData(GetEmbeddedResourceContent("ParseReportCommand.txt")) },
                { @"NVika\Parsers\InspectCodeParser.cs", new MockFileData(GetEmbeddedResourceContent("InspectCodeParser.txt")) },
                { @"NVika\Program.cs", new MockFileData(GetEmbeddedResourceContent("Program.txt")) },
            });
            var parser = new InspectCodeParser(fileSystem);

            // act
            var result = parser.Parse(report);

            // assert
            Assert.Equal(41, result.Count());

            var issue = result.First();
            Assert.Equal("Constraints Violations", issue.Category);
            Assert.Equal("Inconsistent Naming", issue.Description);
            Assert.Equal(@"NVika\BuildServers\AppVeyor.cs", issue.FilePath);
            Assert.Null(issue.HelpUri);
            Assert.Equal(15u, issue.Line);
            Assert.Equal("Name '_appVeyorAPIUrl' does not match rule 'Instance fields (private)'. Suggested name is '_appVeyorApiUrl'.", issue.Message);
            Assert.Equal("InconsistentNaming", issue.Name);
            Assert.Equal(32u, issue.Offset.Start);
            Assert.Equal(47u, issue.Offset.End);
            Assert.Equal("NVika", issue.Project);
            Assert.Equal(IssueSeverity.Warning, issue.Severity);
            Assert.Equal("InspectCode", issue.Source);

            issue = result.Skip(7).First();
            Assert.Equal("Common Practices and Code Improvements", issue.Category);
            Assert.Equal("Convert local variable or field to constant: Private accessibility", issue.Description);
            Assert.Equal(@"NVika\BuildServers\LocalBuildServer.cs", issue.FilePath);
            Assert.Null(issue.HelpUri);
            Assert.Equal(9u, issue.Line);
            Assert.Equal("Convert to constant", issue.Message);
            Assert.Equal("ConvertToConstant.Local", issue.Name);
            Assert.Equal(30u, issue.Offset.Start);
            Assert.Equal(42u, issue.Offset.End);
            Assert.Equal("NVika", issue.Project);
            Assert.Equal(IssueSeverity.Suggestion, issue.Severity);
            Assert.Equal("InspectCode", issue.Source);

            issue = result.Skip(21).First();
            Assert.Equal("Redundancies in Code", issue.Category);
            Assert.Equal("Redundant 'this.' qualifier", issue.Description);
            Assert.Equal(@"NVika\ParseReportCommand.cs", issue.FilePath);
            Assert.Null(issue.HelpUri);
            Assert.Equal(33u, issue.Line);
            Assert.Equal("Qualifier 'this.' is redundant", issue.Message);
            Assert.Equal("RedundantThisQualifier", issue.Name);
            Assert.Equal(12u, issue.Offset.Start);
            Assert.Equal(17u, issue.Offset.End);
            Assert.Equal("NVika", issue.Project);
            Assert.Equal(IssueSeverity.Warning, issue.Severity);
            Assert.Equal("InspectCode", issue.Source);

            issue = result.Skip(33).First();
            Assert.Equal("Language Usage Opportunities", issue.Category);
            Assert.Equal("Loop can be converted into LINQ-expression", issue.Description);
            Assert.Equal(@"NVika\Parsers\InspectCodeParser.cs", issue.FilePath);
            Assert.Equal("http://confluence.jetbrains.net/display/ReSharper/Loop+can+be+converted+into+a+LINQ+expression", issue.HelpUri.AbsoluteUri);
            Assert.Equal(27u, issue.Line);
            Assert.Equal("Loop can be converted into LINQ-expression", issue.Message);
            Assert.Equal("LoopCanBeConvertedToQuery", issue.Name);
            Assert.Equal(12u, issue.Offset.Start);
            Assert.Equal(19u, issue.Offset.End);
            Assert.Equal("NVika", issue.Project);
            Assert.Equal(IssueSeverity.Suggestion, issue.Severity);
            Assert.Equal("InspectCode", issue.Source);

            issue = result.Skip(38).First();
            Assert.Equal("Potential Code Quality Issues", issue.Category);
            Assert.Equal("Auto-implemented property accessor is never used: Private accessibility", issue.Description);
            Assert.Equal(@"NVika\Parsers\InspectCodeParser.cs", issue.FilePath);
            Assert.Null(issue.HelpUri);
            Assert.Equal(1u, issue.Line);
            Assert.Equal("Auto-implemented property accessor is never used", issue.Message);
            Assert.Equal("UnusedAutoPropertyAccessor.Local", issue.Name);
            Assert.Equal(0u, issue.Offset.Start);
            Assert.Equal(5u, issue.Offset.End);
            Assert.Equal("NVika", issue.Project);
            Assert.Equal(IssueSeverity.Error, issue.Severity);
            Assert.Equal("InspectCode", issue.Source);

            issue = result.Skip(39).First();
            Assert.Equal("Redundancies in Code", issue.Category);
            Assert.Equal("Redundant 'case' label", issue.Description);
            Assert.Equal(@"NVika\Parsers\InspectCodeParser.cs", issue.FilePath);
            Assert.Null(issue.HelpUri);
            Assert.Null(issue.Line);
            Assert.Equal("Redundant case label", issue.Message);
            Assert.Equal("RedundantCaseLabel", issue.Name);
            Assert.Null(issue.Offset);
            Assert.Equal("NVika", issue.Project);
            Assert.Equal(IssueSeverity.Hint, issue.Severity);
            Assert.Equal("InspectCode", issue.Source);

            issue = result.Last();
            Assert.Equal("Common Practices and Code Improvements", issue.Category);
            Assert.Equal("Parameter type can be IEnumerable<T>: Non-private accessibility", issue.Description);
            Assert.Equal(@"NVika\Program.cs", issue.FilePath);
            Assert.Null(issue.HelpUri);
            Assert.Equal(35u, issue.Line);
            Assert.Equal("Parameter can be of type 'IEnumerable<string>'", issue.Message);
            Assert.Equal("ParameterTypeCanBeEnumerable.Global", issue.Name);
            Assert.NotNull(issue.Offset);
            Assert.Equal(25u, issue.Offset.Start);
            Assert.Equal(33u, issue.Offset.End);
            Assert.Equal("NVika", issue.Project);
            Assert.Equal(IssueSeverity.Suggestion, issue.Severity);
            Assert.Equal("InspectCode", issue.Source);
        }
    }
}
