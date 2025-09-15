using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HeatingPipeV5
{
    
        public static class StringHelpers
        {
            // Нормализует строку: заменяет все пробельные последовательности на один обычный пробел,
            // убирает неразрывные пробелы, обрезает края и приводит к одному регистру.
            private static string Normalize(string s)
            {
                if (s == null) return string.Empty;
                // Заменяем неразрывные пробелы и подобные на обычный пробел
                s = s.Replace('\u00A0', ' ');
                // Заменяем любые пробельные последовательности на один пробел
                s = Regex.Replace(s, @"\s+", " ");
                // Обрезаем и в нижний регистр для регистронезависимого сравнения
                return s.Trim().ToLowerInvariant();
            }

            // Проверяет, содержится ли needle как отдельный элемент в haystack.
            // Элементы разделяются запятой, точкой с запятой или просто пробелами.
            public static bool ContainsElement(string haystack, string needle)
            {
                var h = Normalize(haystack);
                var n = Normalize(needle);
                if (string.IsNullOrEmpty(n)) return false;

                // Разделим на элементы по запятой/точке с запятой или по " , " с удалением пустых.
                var parts = Regex.Split(h, @"\s*[,;]\s*")
                                 .SelectMany(p => p.Split(' '))
                                 .Where(p => !string.IsNullOrWhiteSpace(p))
                                 .Select(p => p.Trim())
                                 .ToArray();

                // Сравниваем элементы и также проверяем подстроку среди элементов,
                // чтобы учитывать варианты "Т15 1" как часть элемента "Т15 1"
                foreach (var p in parts)
                {
                    if (p.Equals(n, StringComparison.Ordinal)) return true;
                    // проверка на совпадение как содержимого элемента (если элемент может содержать пробелы)
                    if (p.Contains(n)) return true;
                }

                // Дополнительно проверка вхождения всей нормализованной строки (на всякий случай)
                return h.Contains(n);
            }
        }
    
}
