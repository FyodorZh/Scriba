using System;
using NUnit.Framework;
using Scriba.Consumers;

namespace Scriba.Test
{
    public class Test
    {
        [Test]
        public void TestLogger()
        {
            Logger logger = new Logger();
            logger.LogTime = false;
            InMemoryLogConsumer consumer = new InMemoryLogConsumer();
            logger.AddConsumer(consumer);
            
            Check(logger, consumer, _commonTests);
            Check(logger, consumer, _loggerOnlyTests);
        }
        
        [Test]
        public void TestLoggerWrapper()
        {
            Logger logger = new Logger();
            logger.LogTime = false;
            InMemoryLogConsumer consumer = new InMemoryLogConsumer();
            logger.AddConsumer(consumer);
            var wrapper = logger.Wrap();
            
            Check(wrapper, consumer, _commonTests);
            
            logger.Tags.Add("core");
            Check(wrapper, consumer, _wrapperOnlyTests);
        }

        private void Check(ILogger logger, InMemoryLogConsumer consumer, (Action<ILogger> cmd, string? expected)[] commands)
        {
            foreach (var pair in commands)
            {
                pair.cmd.Invoke(logger);
                var list = consumer.TakeAll();
                var expected = pair.expected?.Replace('\'', '"');
                if (expected == null)
                {
                    Assert.That(list, Is.Empty);
                }
                else if (!expected.Contains(';'))
                {
                    Assert.That(list.Length, Is.EqualTo(1), expected);
                    Assert.That(list[0], Is.EqualTo(expected));
                }
                else
                {
                    var results = expected.Split(';');
                    Assert.That(list.Length, Is.EqualTo(results.Length));
                    for (int i = 0; i < results.Length; i++)
                    {
                        Assert.That(list[i], Is.EqualTo(results[i]));
                    }
                }
            }
        }
        
        private readonly (Action<ILogger> cmd, string? expected)[] _commonTests =
        {
            (l => l.d("Hello"), "{'severity': 'DEBUG', 'msg': 'Hello'}"),
            (l => l.i("Hello"), "{'severity': 'INFO', 'msg': 'Hello'}"),
            (l => l.w("Hello"), "{'severity': 'WARN', 'msg': 'Hello'}"),
            (l => l.e("Hello"), "{'severity': 'ERROR', 'msg': 'Hello'}"),
            (l => l.e("Hello {0}", "world"), "{'severity': 'ERROR', 'msg': 'Hello world', '0': 'world'}"),
            (l => l.e("Hello {1} {0}", "world", "big"), "{'severity': 'ERROR', 'msg': 'Hello big world', '1': 'big', '0': 'world'}"),
            (l => l.e("Hello {param}", "world"), "{'severity': 'ERROR', 'msg': 'Hello world', 'param': 'world'}"),
            (l => l.e("Hello {param} {0}", "big", "world"), "{'severity': 'ERROR', 'msg': 'Hello big big', 'param': 'big', '0': 'big'}"),
            (l => l.e("Hello {param} {1}", "big", "world"), "{'severity': 'ERROR', 'msg': 'Hello big world', 'param': 'big', '1': 'world'}"),
            (l => l.Tags.Add("tg"), null),
            (l => l.i("Hello"), "{'severity': 'INFO', 'msg': 'Hello', 'tags_list': ['tg']}"),
            (l => l.Tags.Add("tag", "value"), null),
            (l => l.i("Hello"), "{'severity': 'INFO', 'msg': 'Hello', 'tags_list': ['tg', {'tag': 'value'}]}"),
            (l => l.Tags.Remove("tag"), null),
            (l => l.Tags.Remove("tg"), null),
        };
        
        private readonly (Action<ILogger> cmd, string? expected)[] _loggerOnlyTests =
        {
            (l => l.Tags.Add("tag", "value"), null),
            (l => l.Tags.Add("tag", "value2"), null),
            (l => l.i("Hello"), "{'severity': 'INFO', 'msg': 'Hello', 'tags_list': [{'tag': 'value'}]}"),
        };
        
        private readonly (Action<ILogger> cmd, string? expected)[] _wrapperOnlyTests =
        {
            (l => l.d("Hello"), "{'severity': 'DEBUG', 'msg': 'Hello', 'tags_list': ['core']}"),
            (l => l.i("Hello"), "{'severity': 'INFO', 'msg': 'Hello', 'tags_list': ['core']}"),
            (l => l.w("Hello"), "{'severity': 'WARN', 'msg': 'Hello', 'tags_list': ['core']}"),
            (l => l.Tags.Add("tag", "value"), null),
            (l => l.w("Hello"), "{'severity': 'WARN', 'msg': 'Hello', 'tags_list': ['core', {'tag': 'value'}]}"),
            (l => l.Tags.Add("core", "override"), null),
            (l => l.w("Hello"), "{'severity': 'WARN', 'msg': 'Hello', 'tags_list': ['core', {'tag': 'value'}, {'core': 'override'}]}"),
            (l => l.Tags.Remove("core"), null),
            (l => l.w("Hello"), "{'severity': 'WARN', 'msg': 'Hello', 'tags_list': ['core', {'tag': 'value'}]}"),
            (l => l.Tags.Remove("tag"), null),
            (l => l.e("Hello"), "{'severity': 'ERROR', 'msg': 'Hello', 'tags_list': ['core']}"),
        };
    }
}
