
using Cysharp.Threading.Tasks;
using Doozy.Runtime.Common.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using TMPro;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Tim.GeneralUtility
{
    public static class GeneralUtility
    {
        public static DataTable DataTable = new DataTable();

        public static IEnumerator ChangeValueOverTime(float from, float to, float duration, Action<float> OnValueChange, Action OnFinish = null)
        {
            float counter = 0f;

            if (from.Equals(to))
            {
                OnValueChange?.Invoke(to);
                yield return new WaitForSeconds(duration);
            }
            else
            {
                while (counter < duration)
                {
                    if (Time.timeScale == 0) counter += Time.unscaledDeltaTime;
                    else counter += Time.deltaTime;

                    float val = Mathf.Lerp(from, to, counter / duration);

                    OnValueChange?.Invoke(val);

                    yield return null;
                }
            }

            OnFinish?.Invoke();
        }

        public static double LerpDouble(double a, double b, double t)
        {
            return a + (b - a) * t;
        }


        public static IEnumerator ChangeValueOverTime(double from, double to, float duration, Action<double> OnValueChange, Action OnFinish = null)
        {
            float counter = 0f;

            if (from.Equals(to))
            {
                OnValueChange?.Invoke(to);
                yield return new WaitForSeconds(duration);
            }
            else
            {
                while (counter < duration)
                {
                    if (Time.timeScale == 0) counter += Time.unscaledDeltaTime;
                    else counter += Time.deltaTime;

                    double val = LerpDouble(from, to, counter / duration);

                    OnValueChange?.Invoke(val);

                    yield return null;
                }
            }

            OnFinish?.Invoke();
        }

        public static async UniTask StartCountDown(this TimeSpan timespan, TMP_Text text, CancellationToken cancelToken, Action<TimeSpan> OnUpdated = null, string prefix = null)
        {
            while (timespan.TotalSeconds >= 0 && !cancelToken.IsCancellationRequested)
            {
                text.text = $"{prefix ?? string.Empty}{timespan.ToReadableString()}";

                await UniTask.Delay(1000);

                timespan = timespan.Subtract(TimeSpan.FromSeconds(1));

                OnUpdated?.Invoke(timespan);
            }

            timespan = TimeSpan.Zero;
            text.text = $"{prefix ?? string.Empty}{timespan.ToReadableString()}";
        }




        /// <summary>
        /// Appends a space before all capital letters in a sentence, except the first character.
        /// </summary>
        /// <param name="text">Enumeration or text value that is formated like "AllRightsReserved".</param>
        /// <returns>Input text with a space infront of all capital letters..</returns>
        public static string AddSpaceBetweenCapitalLetters(string text)
        {
            StringBuilder str = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                if (i > 0 && char.IsUpper(text[i]))
                {
                    str.Append(" ");
                }

                str.Append(text[i]);
            }

            return str.ToString();
        }
        public static string AbbreviateNumber(this double number)
        {
            if (Math.Abs(number) < 1000)
                return number.ToString("0.##");

            string[] suffixes = { "", "K", "M", "B", "T" }; // Thousand, Million, Billion, Trillion
            int suffixIndex = 0;
            double absNumber = Math.Abs(number); // Work with absolute value

            while (absNumber >= 1000 && suffixIndex < suffixes.Length - 1)
            {
                absNumber /= 1000;
                suffixIndex++;
            }

            // Preserve the sign of the number
            return (number < 0 ? "-" : "") + absNumber.ToString("0.##") + suffixes[suffixIndex];
        }

        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    var domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static IEnumerator PerformActionEverySecondOverTime(double duration, Action<double> action, Action OnRemainingZero = null)
        {
            double remaining = duration;

            while (remaining >= 0)
            {
                action?.Invoke(remaining);

                if (remaining > 0)
                    yield return new WaitForSecondsRealtime(1f);

                remaining--;
            }

            OnRemainingZero?.Invoke();
        }

        public static string ReplaceExact(this string originalString, string stringToReplace, string replacementString)
        {
            if (originalString == null)
                throw new ArgumentNullException(nameof(originalString));
            if (stringToReplace == null)
                throw new ArgumentNullException(nameof(stringToReplace));
            if (replacementString == null)
                throw new ArgumentNullException(nameof(replacementString));

            // Split the string into words based on spaces
            var words = originalString.Split(' ');

            // Replace exact matches
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i] == stringToReplace)
                {
                    words[i] = replacementString;
                }
            }

            // Join the words back into a string
            return string.Join(' ', words);
        }
        public static IEnumerator PerformActionWithRepetition(int repetition, float interval, Action<int> action, Action OnFinished = null, bool TimeScaleIndependent = false)
        {
            // zero (0) repetition means endless

            int currentRepetition = 0;

            bool forever = repetition <= 0;

            while (forever || currentRepetition < repetition)
            {
                currentRepetition++;

                action?.Invoke(currentRepetition);

                if (TimeScaleIndependent) yield return new WaitForSecondsRealtime(interval);
                else yield return new WaitForSeconds(interval);
            }

            OnFinished?.Invoke();
        }

        public static double GetElapsedSeconds(DateTime startTime)
        {
            double elapsedTime = 0;
            DateTime now = DateTime.Now;
            TimeSpan duration = now - startTime;

            //Debug.Log($"Now: {now.ToString()}");
            //Debug.Log($"Now: {now.ToString()}");
            //Debug.Log($"duration: { duration.ToReadableString()}");

            duration = duration.Duration();
            elapsedTime = duration.TotalSeconds;
            return elapsedTime;
        }

        public static IEnumerator DelayedAction(float delay, Action action, bool IgnoreTimeScale = false)
        {
            if (IgnoreTimeScale) yield return new WaitForSecondsRealtime(delay);
            else yield return new WaitForSeconds(delay);

            action?.Invoke();
        }

        public static IEnumerator WaitForFrame(int frames, Action action)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return new WaitForEndOfFrame();
            }

            action?.Invoke();
        }

        public static List<int> StringToIntList(string str)
        {

            List<int> result = new List<int>();

            if (string.IsNullOrEmpty(str))
                return result;

            foreach (var s in str.Split(','))
            {
                if (int.TryParse(s, out int num)) result.Add(num);
            }

            return result;
        }

        public static bool HaveChance(this float chance)
        {
            return chance >= Random.Range(0f, 1f);
        }

        public static string GetOrdinalSuffix(int number)
        {
            // Special case for 11, 12, 13
            if (number % 100 >= 11 && number % 100 <= 13)
            {
                return "th";
            }

            // Check last digit for ordinal suffix
            return (number % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th"
            };
        }

        public static string MiddleEllipses(this string text, int maxCharacters)
        {
            if (text.Length > maxCharacters)
            {
                int halfLength = maxCharacters / 2;
                string start = text.Substring(0, halfLength); // First part
                string end = text.Substring(text.Length - halfLength); // Last part
                return $"{start}...{end}"; // Combine with ellipses in the middle
            }
            else
            {
                return text;
            }
        }


#if UNITY_EDITOR
        public static int GetCurrentAmountOfScriptableObject<T>() where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);  //FindAssets uses tags check documentation for more info
            return guids.Length;
        }
        #endif

        #region Extension Method
        public static string CapitalizeEachWord(this string target)
        {
            string titleCase = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(target.ToLower());
            return titleCase;
        }

        public static Texture2D ToTexture(this Sprite sprite)
        {
            if (sprite.rect.width != sprite.texture.width)
            {
                Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                             (int)sprite.textureRect.y,
                                                             (int)sprite.textureRect.width,
                                                             (int)sprite.textureRect.height);
                newText.SetPixels(newColors);
                newText.Apply();
                return newText;
            }
            else return sprite.texture;
        }

        public static DateTime GetCurrentTime()
        {
            return DateTime.Now;
        }

        public static IEnumerator CountdownDuration(DateTime endTime, Action<string> readableTime, Action OnFinish = null)
        {
            TimeSpan durationLeft = endTime - GetCurrentTime();

            while (durationLeft.TotalSeconds >= 0)
            {

                readableTime?.Invoke(durationLeft.ToReadableString());

                yield return new WaitForSecondsRealtime(1);

                durationLeft = endTime - GetCurrentTime();
            }


            OnFinish?.Invoke();
        }

        public static bool TryParseJson<T>(this string json, out T result)
        {
            bool success = true;
            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) => { success = false; args.ErrorContext.Handled = true; },
                MissingMemberHandling = MissingMemberHandling.Error
            };
            result = JsonConvert.DeserializeObject<T>(json, settings);
            return success;
        }

        public static string ToReadableString(this TimeSpan span)
        {
            string result = "";

            span = span.Duration();

            result = span.ToString();

            if (span.TotalDays >= 1)
            {
                result = string.Format("{0:dd}d {0:hh\\:mm\\:ss}", span);
                //Debug.Log("TotalDays > 1");
            }
            else if (span.Hours >= 1)
            {
                result = string.Format("{0:hh\\:mm\\:ss}", span);
                //Debug.Log("HOurs > 1");
            }
            else
            {
                result = string.Format("{0:mm\\:ss}", span);
                //Debug.Log("Else");
            }

            if (span.TotalSeconds <= 0)
            {
                result = "00:00";
            }

            //Debug.Log($"Timespan: {result}");

            return result;
        }

        public static void ClearAllChildren(this Transform target)
        {
            for (int i = target.childCount - 1; i >= 0; i--)
            {
                Destroy(target.GetChild(i).gameObject);
            }
        }

        public static void ClearChildrenExceptTheFirstElement(this Transform target, int skip = 1)
        {
            for (int i = target.childCount-1; i > skip - 1; i--)
            {
                Debug.Log("Destroying: " + target.GetChild(i).gameObject.name);
                Destroy(target.GetChild(i).gameObject);
            }
        }

        public static void DestroyAllChildrensExcept(this Transform target, List<GameObject> excepts)
        {
            for (int i = target.childCount - 1; i >= 0; i--)
            {
                if (excepts.Contains(target.GetChild(i).gameObject)) continue;
                Destroy(target.GetChild(i).gameObject);
            }
        }

        public static void DestroyAllChildrensHasComponentAndExcept<T>(this Transform target, List<GameObject> excepts = null)
        {
            for (int i = target.childCount - 1; i >= 0; i--)
            {
                if (target.GetChild(i).gameObject.GetComponent<T>() == null) continue;
                if (excepts.Contains(target.GetChild(i).gameObject)) continue;
                Destroy(target.GetChild(i).gameObject);
            }
        }

        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

                    if (attr != null) return attr.Description;
                }
            }
            return null;
        }

        public static IEnumerable<T> Mode<T>(this IEnumerable<T> input)
        {
            var dict = input.ToLookup(x => x);
            if (dict.Count == 0)
                return Enumerable.Empty<T>();
            var maxCount = dict.Max(x => x.Count());
            return dict.Where(x => x.Count() == maxCount).Select(x => x.Key);
        }

        public static string WithColor(this string text, string hex)
        {
            return $"<color={hex}>{text}</color>";
        }

        public static string WithColor(this string text, Color color)
        {
            return WithColor(text, $"#{ColorUtility.ToHtmlStringRGB(color)}");
        }

        public static IEnumerable<T> GetPage<T>(this IList<T> list, int page, int pageSize)
        {
            return list.Skip((page - 1) * pageSize).Take(pageSize);
        }

        public static void Print(this object obj)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.ContractResolver = new IgnorePropertiesResolver(new List<string> { "name", "hideFlags", "normalized", "magnitude", "sqrMagnitude" });

            if (obj is string objString) Debug.Log(objString);
            else Debug.Log(JsonConvert.SerializeObject(obj, settings));
        }

        public static void Print<T>(this T exception) where T : Exception
        {
            Debug.LogError($"<b>Message:</b> {exception.Message}\n\n" +
                $"StackTrace:\n{exception.StackTrace}");
        }

        public static void PrintAsError(this object obj)
        {
            if (obj is string objString) Debug.LogError(objString);
            else Debug.LogError(JsonConvert.SerializeObject(obj));
        }

        public static string Serialize(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T GetEnumValueFromDescription<T>(this string description) where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field,
                typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }

            throw new ArgumentException("Not found.", nameof(description));
            // Or return default(T);
        }

        /// <summary>
        /// Set pivot without changing the position of the element
        /// </summary>
        public static void SetPivot(this RectTransform rectTransform, Vector2 pivot)
        {
            Vector3 deltaPosition = rectTransform.pivot - pivot;    // get change in pivot
            deltaPosition.Scale(rectTransform.rect.size);           // apply sizing
            deltaPosition.Scale(rectTransform.localScale);          // apply scaling
            deltaPosition = rectTransform.rotation * deltaPosition; // apply rotation

            rectTransform.pivot = pivot;                            // change the pivot
            rectTransform.localPosition -= deltaPosition;           // reverse the position change
        }

        public static float Remap(this float from, float fromMin, float fromMax, float toMin, float toMax)
        {
            var fromAbs = from - fromMin;
            var fromMaxAbs = fromMax - fromMin;

            var normal = fromAbs / fromMaxAbs;

            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;

            var to = toAbs + toMin;

            return to;
        }

        public static T Next<T>(this IList<T> list, T item)
        {
            if (item == null) return list[0];

            var nextIndex = list.IndexOf(item) + 1;

            if (nextIndex == list.Count)
            {
                return list[0];
            }

            return list[nextIndex];
        }

        public static T Next<T>(this IList<T> list, T item, bool loop = true)
        {
            if (item == null) return list[0];

            var nextIndex = list.IndexOf(item) + 1;

            if (nextIndex == list.Count)
            {
                if(loop)
                return list[0];
                else return list[list.Count-1];
            }

            return list[nextIndex];
        }

        public static T Previous<T>(this IList<T> list, T item)
        {
            var prevIndex = list.IndexOf(item) - 1;

            if (prevIndex < 0)
            {
                return list.Last();
            }

            return list[prevIndex];
        }

        public static T RandomElement<T>(this IEnumerable<T> items)
        {
            // Return a random item.
            if (items.Count() <= 0) return default;

            return items.ElementAt(UnityEngine.Random.Range(0, items.Count()));
        }

        public static IEnumerable<T> TakeRandomElements<T>(this IEnumerable<T> items, int count)
        {
            // Return some random items.
            System.Random random = new System.Random();

            return items.OrderBy(x => random.Next()).Take(count).ToList();
        }

        public static T CloneJson<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (ReferenceEquals(source, null)) return default;

            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }

        public static T DeepClone<T>(this T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new System.ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (UnityEngine.Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            System.IO.Stream stream = new System.IO.MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, System.IO.SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }

        }

        public static bool CompareArray3DBool(this bool[,,] data1, bool[,,] data2)
        {
            if (data1.Rank != data2.Rank) return false;

            for (int i = 0; i < data1.GetLength(0); i++)
            {
                for (int j = 0; j < data1.GetLength(1); j++)
                {
                    for (int k = 0; k < data1.GetLength(2); k++)
                    {
                        if (data1[i, j, k] != data2[i, j, k]) return false;
                    }
                }
            }

            return true;
        }

        public static void Shuffle<T>(this IList<T> ts)
        {
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = Random.Range(i, count);
                var tmp = ts[i];
                ts[i] = ts[r];
                ts[r] = tmp;
            }
        }
        #endregion
    }

}