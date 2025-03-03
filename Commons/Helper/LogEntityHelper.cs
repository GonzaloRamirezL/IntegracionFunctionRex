using Common.Entity;
using Common.Enum;
using Common.ViewModels;
using Helpers;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Helper
{
    public static class LogEntityHelper
    {
        private static void Log(LogEntity logEntity)
        {
            bool execute = false;
            bool.TryParse(ConfigurationHelper.Value("Execute"), out execute);
            //if (!execute)
            //{
                LogHelper.Log($"{logEntity.LogType} {logEntity.Event}: {logEntity.Item} {logEntity.Identifier} - {logEntity.Message}");
            //}
        }

        public static LogEntity LogEntity(RexExecutionVM rexExecution, string logEvent, string identifier, string message, string logType, string item)
        {
            identifier = Regex.Replace(identifier, @"[^a-zA-Z0-9\s]", "");
            var logEntity = new LogEntity()
            {
                PartitionKey = rexExecution.CompanyName,
                RowKey = $"{rexExecution.ExecutionDateTime}_{identifier}_{Guid.NewGuid()}",
                LogType = logType,
                Event = logEvent,
                Item = item,
                Identifier = identifier,
                Message = message,
                Timestamp = DateTime.UtcNow

            };

            Log(logEntity);

            return logEntity;
        }
        public static LogEntity Group(RexExecutionVM rexExecution, string logEvent, string identifier, string message, string logType)
        {   
            var logEntity = new LogEntity()
            {
                PartitionKey = rexExecution.CompanyName,
                RowKey = $"{rexExecution.ExecutionDateTime}_{identifier}_{Guid.NewGuid()}",
                LogType = logType,
                Event = logEvent,
                Item = LogItem.GROUP,
                Identifier = identifier,
                Message = message,
                Timestamp = DateTime.UtcNow

            };

            Log(logEntity);

            return logEntity;
        }

        public static LogEntity User(RexExecutionVM rexExecution, string logEvent, string identifier, string message, string logType)
        {
            var logEntity = new LogEntity()
            {
                PartitionKey = rexExecution.CompanyName,
                RowKey = $"{rexExecution.ExecutionDateTime}_{identifier}_{Guid.NewGuid()}",
                LogType = logType,
                Event = logEvent,
                Item = LogItem.USER,
                Identifier = identifier,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            Log(logEntity);
            return logEntity ;
        }

        public static LogEntity Position(RexExecutionVM rexExecution, string logEvent, string identifier, string message, string logType)
        {
            var logEntity = new LogEntity()
            {
                PartitionKey = rexExecution.CompanyName,
                RowKey = $"{rexExecution.ExecutionDateTime}_{identifier}_{Guid.NewGuid()}",
                LogType = logType,
                Event = logEvent,
                Item = LogItem.POSITION,
                Identifier = identifier,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            Log(logEntity);
            return logEntity;
        }

        public static LogEntity Concept(RexExecutionVM rexExecution, string logEvent, string identifier, string message, string logType, string logItem)
        {
            var logEntity = new LogEntity()
            {
                PartitionKey = rexExecution.CompanyName,
                RowKey = $"{rexExecution.ExecutionDateTime}_{identifier}_{Guid.NewGuid()}",
                LogType = logType,
                Event = logEvent,
                Item = logItem,
                Identifier = identifier,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            Log(logEntity);
            return logEntity;
        }

        public static LogEntity Absence(RexExecutionVM rexExecution, string logEvent, string identifier, string message, string logType)
        {
            var logEntity = new LogEntity()
            {
                PartitionKey = rexExecution.CompanyName,
                RowKey = $"{rexExecution.ExecutionDateTime}_{identifier}_{Guid.NewGuid()}",
                LogType = logType,
                Event = logEvent,
                Item = LogItem.ABSENCE,
                Identifier = identifier,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            Log(logEntity);
            return logEntity;
        }
        public static LogEntity TimeOff(RexExecutionVM rexExecution, string logEvent, string identifier, string message, string logType)
        {
            var logEntity = new LogEntity()
            {
                PartitionKey = rexExecution.CompanyName,
                RowKey = $"{rexExecution.ExecutionDateTime}_{identifier}_{Guid.NewGuid()}",
                LogType = logType,
                Event = logEvent,
                Item = LogItem.TIMEOFF,
                Identifier = identifier,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            Log(logEntity);
            return logEntity;
        }
        public static LogEntity Planning(RexExecutionVM rexExecution, string logEvent, string identifier, string message, string logType)
        {
            var logEntity = new LogEntity()
            {
                PartitionKey = rexExecution.CompanyName,
                RowKey = $"{rexExecution.ExecutionDateTime}_{identifier}_{Guid.NewGuid()}",
                LogType = logType,
                Event = logEvent,
                Item = LogItem.PLANNING,
                Identifier = identifier,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            Log(logEntity);
            return logEntity;
        }
    }
}
