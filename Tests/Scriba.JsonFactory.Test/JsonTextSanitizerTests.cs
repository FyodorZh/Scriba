using System.IO;
using System.Text;
using Scriba.JsonFactory.Utils;

namespace Scriba.JsonFactory.Test;

public sealed class JsonTextSanitizerTests
{
    private StringWriter _output = null!;
    private JsonTextSanitizer _filter = null!;

    [SetUp]
    public void SetUp()
    {
        _output = new StringWriter();
        _filter = new JsonTextSanitizer();
        _filter.Init(_output);
    }

    [TearDown]
    public void TearDown()
    {
        _filter.Init(null);
        _filter.Dispose();
        _output.Dispose();
    }

    [Test]
    public void WriteChar_SafeChar_PassesThrough()
    {
        _filter.Write('a');
        Assert.That(_output.ToString(), Is.EqualTo("a"));
    }

    [Test]
    public void WriteChar_DoubleQuote_ReplacedWithSingleQuote()
    {
        _filter.Write('"');
        Assert.That(_output.ToString(), Is.EqualTo("'"));
    }

    [Test]
    public void WriteChar_Backslash_ReplacedWithForwardSlash()
    {
        _filter.Write('\\');
        Assert.That(_output.ToString(), Is.EqualTo("/"));
    }

    [Test]
    public void WriteChar_ControlChar_BackslashEscaped()
    {
        _filter.Write('\n');
        Assert.That(_output.ToString(), Is.EqualTo("\\\n"));
    }

    [Test]
    public void WriteChar_Tab_BackslashEscaped()
    {
        _filter.Write('\t');
        Assert.That(_output.ToString(), Is.EqualTo("\\\t"));
    }

    [Test]
    public void WriteChar_CarriageReturn_BackslashEscaped()
    {
        _filter.Write('\r');
        Assert.That(_output.ToString(), Is.EqualTo("\\\r"));
    }

    [Test]
    public void WriteChar_NullChar_BackslashEscaped()
    {
        _filter.Write('\0');
        Assert.That(_output.ToString(), Is.EqualTo("\\\0"));
    }

    [Test]
    public void WriteChar_DelimiterZeroX1F_BackslashEscaped()
    {
        _filter.Write('\x1f');
        Assert.That(_output.ToString(), Is.EqualTo("\\\x1f"));
    }

    [Test]
    public void WriteChar_MultipleChars_AllTransformed()
    {
        _filter.Write('"');
        _filter.Write('a');
        _filter.Write('\\');
        _filter.Write('\n');
        _filter.Write('b');
        Assert.That(_output.ToString(), Is.EqualTo("'a/\\\nb"));
    }

    [Test]
    public void WriteString_Null_NoOutput()
    {
        const string? nullStr = null;
        _filter.Write(nullStr!);
        Assert.That(_output.ToString(), Is.EqualTo(""));
    }

    [Test]
    public void WriteString_Empty_NoOutput()
    {
        _filter.Write("");
        Assert.That(_output.ToString(), Is.EqualTo(""));
    }

    [Test]
    public void WriteString_SafeString_PassesThrough()
    {
        _filter.Write("hello world");
        Assert.That(_output.ToString(), Is.EqualTo("hello world"));
    }

    [Test]
    public void WriteString_WithDoubleQuote_Replaced()
    {
        _filter.Write("say \"hello\"");
        Assert.That(_output.ToString(), Is.EqualTo("say 'hello'"));
    }

    [Test]
    public void WriteString_WithBackslash_Replaced()
    {
        _filter.Write("a\\b\\c");
        Assert.That(_output.ToString(), Is.EqualTo("a/b/c"));
    }

    [Test]
    public void WriteString_WithControlChars_Escaped()
    {
        _filter.Write("line1\nline2\rline3");
        Assert.That(_output.ToString(), Is.EqualTo("line1\\\nline2\\\rline3"));
    }

    [Test]
    public void WriteString_MixedContent_AllTransformed()
    {
        _filter.Write("a\"b\\c\nd\t");
        Assert.That(_output.ToString(), Is.EqualTo("a'b/c\\\nd\\\t"));
    }

    [Test]
    public void WriteString_AllSpecialChars()
    {
        _filter.Write("\"\\\n\r\t");
        Assert.That(_output.ToString(), Is.EqualTo("'/\\\n\\\r\\\t"));
    }

    [Test]
    public void WriteString_LongSafeString_WrittenInOneBatch()
    {
        var input = new string('x', 512);
        _filter.Write(input);
        Assert.That(_output.ToString(), Is.EqualTo(input));
    }

    [Test]
    public void WriteString_LongStringWithEscapes_BatchesCorrectly()
    {
        var input = new string('a', 255) + '"' + new string('b', 255);
        var expected = new string('a', 255) + "'" + new string('b', 255);
        _filter.Write(input);
        Assert.That(_output.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void WriteCharArray_SafeChars_PassesThrough()
    {
        _filter.Write(new[] { 'a', 'b', 'c' }, 0, 3);
        Assert.That(_output.ToString(), Is.EqualTo("abc"));
    }

    [Test]
    public void WriteCharArray_WithEscapeChars_Transformed()
    {
        _filter.Write(new[] { '"', 'a', '\\', '\n' }, 0, 4);
        Assert.That(_output.ToString(), Is.EqualTo("'a/\\\n"));
    }

    [Test]
    public void WriteCharArray_WithOffset_OnlyProcessesSegment()
    {
        _filter.Write(new[] { 'x', '"', 'y', '\\', 'z' }, 1, 3);
        Assert.That(_output.ToString(), Is.EqualTo("'y/"));
    }

    [Test]
    public void WriteCharArray_LongArray_BatchesCorrectly()
    {
        var arr = new char[300];
        for (int i = 0; i < 300; i++)
            arr[i] = 'a';
        arr[100] = '"';
        arr[200] = '\\';

        _filter.Write(arr, 0, 300);

        var result = _output.ToString();
        Assert.That(result[100], Is.EqualTo('\''));
        Assert.That(result[200], Is.EqualTo('/'));
        Assert.That(result.Length, Is.EqualTo(300));
    }

    [Test]
    public void MultipleWriteCalls_AccumulateCorrectly()
    {
        _filter.Write("hello ");
        _filter.Write('"');
        _filter.Write("world");
        _filter.Write("\\");
        _filter.Write('!');

        Assert.That(_output.ToString(), Is.EqualTo("hello 'world/!"));
    }

    [Test]
    public void InitWithNullWriter_WritesAreNoOps()
    {
        var filter = new JsonTextSanitizer();
        filter.Init(null);

        filter.Write('a');
        filter.Write("hello");
        filter.Write(new[] { 'x', 'y' }, 0, 2);

        // No exception, no output — nothing to assert beyond no crash
    }

    [Test]
    public void Init_NullResetsWriter()
    {
        _filter.Init(null);
        _filter.Write('a');

        var secondOutput = new StringWriter();
        _filter.Init(secondOutput);
        _filter.Write('b');

        Assert.That(secondOutput.ToString(), Is.EqualTo("b"));
    }

    [Test]
    public void Encoding_DelegatesToCoreWriter()
    {
        using var stream = new MemoryStream();
        using var streamWriter = new StreamWriter(stream, Encoding.UTF32);
        _filter.Init(streamWriter);

        Assert.That(_filter.Encoding, Is.EqualTo(Encoding.UTF32));
    }

    [Test]
    public void Encoding_FallbackToUtf8()
    {
        _filter.Init(null);
        Assert.That(_filter.Encoding, Is.EqualTo(Encoding.UTF8));
    }
}
