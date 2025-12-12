
using System.Text.Json;

namespace Scriba.JsonFactory.Test;

public class Tests
{
    private readonly string _tests = """
{}
{"x": 1}
{"x": "y"}
{"x": []}
{"x": [1, "2"]}
{"x": {}}
{"x": {"y": true, "z": []}}
{"x": false}
{"x": 1, "y": "y", "z": [{}]}
"""; 
    
    [Test]
    public void TestJsonCtor()
    {
        foreach (var s in _tests.Split('\n'))
            Check(s);
    }
    
    private void Check(IJsonObject json, string expected)
    {
        json.Serialize(out var str);
        Assert.That(str, Is.EqualTo(expected));
    }

    private void Check(string jsonEtalon)
    {
        var jsonDocument = JsonDocument.Parse(jsonEtalon);
        var obj = new JsonObject();
        Clone(jsonDocument.RootElement, obj);
        Check(obj, jsonEtalon);
    }

    private void Clone(System.Text.Json.JsonElement from, IJsonObject to)
    {
        foreach (var el in from.EnumerateObject())
        {
            switch (el.Value.ValueKind)
            {
                case JsonValueKind.Undefined:
                    throw new InvalidOperationException();
                case JsonValueKind.Object:
                    Clone(el.Value, to.AddObject(el.Name));
                    break;
                case JsonValueKind.Array:
                    Clone(el.Value, to.AddArray(el.Name));
                    break;
                case JsonValueKind.String:
                    to.AddElement(el.Name, el.Value.GetString()!);
                    break;
                case JsonValueKind.Number:
                    to.AddElement(el.Name, el.Value.GetDouble());
                    break;
                case JsonValueKind.True:
                    to.AddElement(el.Name, true);
                    break;
                case JsonValueKind.False:
                    to.AddElement(el.Name, false);
                    break;
                case JsonValueKind.Null:
                    //to.AddElement(el.Name, null);
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void Clone(System.Text.Json.JsonElement from, IJsonArray to)
    {
        foreach (var el in from.EnumerateArray())
        {
            switch (el.ValueKind)
            {
                case JsonValueKind.Undefined:
                    throw new InvalidOperationException();
                case JsonValueKind.Object:
                    Clone(el, to.AddObject());
                    break;
                case JsonValueKind.Array:
                    Clone(el, to.AddArray());
                    break;
                case JsonValueKind.String:
                    to.AddElement(el.GetString()!);
                    break;
                case JsonValueKind.Number:
                    to.AddElement(el.GetDouble());
                    break;
                case JsonValueKind.True:
                    to.AddElement(true);
                    break;
                case JsonValueKind.False:
                    to.AddElement(false);
                    break;
                case JsonValueKind.Null:
                    //to.AddElement(el.Name, null);
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}