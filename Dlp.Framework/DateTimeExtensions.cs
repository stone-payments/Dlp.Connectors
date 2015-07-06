using System;
using System.Collections.Generic;
using System.Globalization;

namespace Dlp.Framework {

    /// <summary>
    /// DateTime extension methods.
    /// </summary>
    public static class DateTimeExtensions {

        /// <summary>
        /// Converts a DateTime object to the specified TimeZoneId. The converted date considers the Daylight Saving Time automatically.
        /// </summary>
        /// <param name="source">DateTime object to be converted.</param>
        /// <param name="sourceTimeZoneId">TimeZoneId for the current DateTime object.</param>
        /// <param name="targetTimeZoneId">Target TimeZoneId to witch the DateTime will be converted.</param>
        /// <returns>Return a new DateTime object with the specified target TimeZoneId.</returns>
        /// <include file='Samples/DateTimeExtensions.xml' path='Docs/Members[@name="ChangeTimeZone"]/*'/>
        public static DateTime ChangeTimeZone(this DateTime source, string sourceTimeZoneId, string targetTimeZoneId) {

            // Converte a data.
            DateTime dateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(source, sourceTimeZoneId, targetTimeZoneId);
            
            return dateTime;
        }

        /// <summary>
        /// Converts a DateTime object to a ISO8601 string. Very useful for REST operation contracts.
        /// </summary>
        /// <param name="source">DateTime object to be converted.</param>
        /// <returns>Return the date and time in ISO8601 format.</returns>
        /// <include file='Samples/DateTimeExtensions.xml' path='Docs/Members[@name="ToIso8601String"]/*'/>
        public static string ToIso8601String(this DateTime source) {

            // Converte a data para uma string no padrão internacional.
            return source.ToString("s", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Get all the availables TimeZones, with its Id and Display Name. Useful for display in ComboBoxes or ListBoxes.
        /// </summary>
        /// <returns>Return a dictionaty with the Id as the Key and the Display Name as the value.</returns>
        public static IDictionary<string, string> SystemTimeZones() {

            // Dicionário que conterá a lista.
            IDictionary<string, string> timeZoneDictionary = new Dictionary<string, string>();

            // Obtém as informações de cada fuso horário do sistema.
            foreach (TimeZoneInfo timeZoneInfo in TimeZoneInfo.GetSystemTimeZones()) {

                timeZoneDictionary.Add(timeZoneInfo.Id, timeZoneInfo.DisplayName);
            }

            return timeZoneDictionary;
        }
    }
}