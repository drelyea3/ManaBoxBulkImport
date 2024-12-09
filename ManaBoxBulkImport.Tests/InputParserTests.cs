using ManaBoxBulkImport;

namespace ManaBoxBulkImportTests
{
    public class InputParserTests
    {
        [Fact]
        public void CodeOnly()
        {
            AssertInput("67", "67", expectedIsJapanese: false, expectedIsFoil: false, 1);
            AssertInput("a16", "a16", expectedIsJapanese: false, expectedIsFoil: false, 1);
            AssertInput("*16", "*16", expectedIsJapanese: false, expectedIsFoil: false, 1);
            AssertInput("f16", "f16", expectedIsJapanese: false, expectedIsFoil: false, 1);
            AssertInput("j16", "j16", expectedIsJapanese: false, expectedIsFoil: false, 1);
        }

        [Fact]
        public void IsJapaneseOnly()
        {
            AssertInput("123j", "123", expectedIsJapanese: true, expectedIsFoil: false, 1);
        }

        [Fact]
        public void IsFoilOnly()
        {
            AssertInput("1f", "1", expectedIsJapanese: false, expectedIsFoil: true, 1);
        }

        [Fact]
        public void IsFoilAndIsJapanese()
        {
            AssertInput("15fj", "15", expectedIsJapanese: true, expectedIsFoil: true, 1);
            AssertInput("15jf", "15", expectedIsJapanese: true, expectedIsFoil: true, 1);
        }

        [Fact]
        public void Counts()
        {
            AssertInput("15f3", "15", expectedIsJapanese: false, expectedIsFoil: true, 3);
            AssertInput("15j3", "15", expectedIsJapanese: true, expectedIsFoil: false, 3);
            AssertInput("15fj3", "15", expectedIsJapanese: true, expectedIsFoil: true, 3);
            AssertInput("15f3j", "15", expectedIsJapanese: true, expectedIsFoil: true, 3);
            AssertInput("15 3", "15", expectedIsJapanese: false, expectedIsFoil: false, 3);
            AssertInput("15,3", "15", expectedIsJapanese: false, expectedIsFoil: false, 3);
        }

        [Fact]
        public void Separators()
        {
            AssertInput("42,f,j,12", "42", expectedIsJapanese: true, expectedIsFoil: true, 12);
            AssertInput("42 f j 12", "42", expectedIsJapanese: true, expectedIsFoil: true, 12);
            AssertInput("42, f j,,,  12   ", "42", expectedIsJapanese: true, expectedIsFoil: true, 12);
        }

        private void AssertInput(string input, string expectedCode, bool expectedIsJapanese, bool expectedIsFoil, int expectedCount)
        {
            InputParser.Parse(input, out var code, out var isJapanese, out var isFoil, out var count);
            Assert.Equal(expectedCode, code);
            Assert.Equal(expectedIsJapanese, isJapanese);
            Assert.Equal(expectedIsFoil, isFoil);
            Assert.Equal(expectedCount, count);
        }
    }
}