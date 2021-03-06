﻿using System;
using System.Diagnostics;
using System.Text;

namespace SterlingDB
{
    /// <summary>
    ///     Default logger (debug) for Sterling
    /// </summary>
    public class SterlingDefaultLogger
    {
        private readonly ISterlingDatabase _database;
        private readonly SterlingLogLevel _minimumLevel;
        private Guid _guid = Guid.Empty;

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="minimumLevel">Minimum level to debug</param>
        public SterlingDefaultLogger(ISterlingDatabase database, SterlingLogLevel minimumLevel)
        {
            _database = database;
            _minimumLevel = minimumLevel;

            if (Debugger.IsAttached) _guid = _database.LogManager.RegisterLogger(_Log);
        }

        /// <summary>
        ///     Detach the logger
        /// </summary>
        public void Detach()
        {
            if (!_guid.Equals(Guid.Empty)) _database.LogManager.UnhookLogger(_guid);
        }

        /// <summary>
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        private void _Log(SterlingLogLevel logLevel, string message, Exception exception)
        {
            if (!Debugger.IsAttached || (int) logLevel < (int) _minimumLevel) return;

            var sb = new StringBuilder(string.Format("{0}::Sterling::{1}::{2}",
                DateTime.Now,
                logLevel,
                message));

            var local = exception;

            while (local != null)
            {
                sb.Append(local);
                local = local.InnerException;
            }

            Debug.WriteLine(sb.ToString());
        }
    }
}