﻿using Microsoft.Extensions.Logging;

namespace Tests.UnitTests.SUT
{
    public static class TestLoggerFactory
    {
        private static ILoggerFactory _loggerFactory;

        static TestLoggerFactory()
        {
            _loggerFactory = LoggerFactory.Create(options => options.AddConsole());
        }

        public static ILogger<T> GetLogger<T>()
        {
            return _loggerFactory.CreateLogger<T>();
        }
    }
}
