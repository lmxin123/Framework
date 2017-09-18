using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Xml;
using System.Collections.Generic;

namespace Framework.Common.Extensions
{
    /// <summary>
    /// StringUtil ��ժҪ˵����
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// ��ȡ�ַ�������
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetLength(this string value)
        {
            byte[] mybyte = Encoding.UTF8.GetBytes(value);
            return mybyte.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentException">����Ϊ��</exception>
        /// <exception cref="ArgumentNullException">Ŀ���ַ���Ϊ�ܿ�</exception>
        /// <returns></returns>
        public static bool StartsWith(this string[] array, string value)
        {
            if (array.Length == 0)
                throw new ArgumentException("����Ϊ�գ�");
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException("Ŀ���ַ���Ϊ�ܿգ�");

            foreach (string str in array)
            {
                if (value.ToLower().StartsWith(str.ToLower())) return true;
            }
            return false;
        }

        /// <summary>
        /// ����ִ��е�HTML����
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ClearHtmlCode(this string value)
        {
            return value.ClearHtmlCode(false);
        }

        /// <summary>
        /// ����ִ��е�HTML���룬�Ƿ������ݼ򵥱�ǩ
        /// </summary>
        /// <param name="value"></param>
        /// <param name="retainSimple"></param>
        /// <returns></returns>
        public static string ClearHtmlCode(this string value, bool retainSimple)
        {
            if (!retainSimple)
                value = Regex.Replace(value, "<br[^>]*?>|<p[^>]*?>", System.Environment.NewLine, (RegexOptions)25);
            if (retainSimple)
                value = Regex.Replace(value, @"<(?!(img|/?strong|/?b|/?br|/?p|/?embed)\W*).*?>", string.Empty, (RegexOptions)25);
            else
                value = Regex.Replace(value, "<.*?>", string.Empty, (RegexOptions)25);
            value = Regex.Replace(value, "&nbsp;", " ", (RegexOptions)25);
            return value;
        }

        /// <summary>
        /// ����Script�ű�
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ClearScriptCode(this string value)
        {
            value = Regex.Replace(value, "<script.*?>.*?</script>", string.Empty, (RegexOptions)25);
            value = Regex.Replace(value, @"on\w*=("".*?""|'.*?'|\w*)", string.Empty, (RegexOptions)25);
            return value;
        }

        /// <summary>
        /// ����A��ǩ�����ӣ�
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ClearATag(this string value)
        {
            value = Regex.Replace(value, "<a[^>]*?>(.*?)</a>", "$1", (RegexOptions)25);
            return value;
        }

        /// <summary>
        /// ����Img��ǩ��ͼƬ��
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ClearImgTag(this string value)
        {
            value = Regex.Replace(value, "<img [^<]*>", string.Empty, (RegexOptions)25);
            return value;
        }

        /// <summary>
        /// �� �ַ��� ���� HTML ����
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string HtmlEncode(this string value)
        {
            value = value.Replace("  ", "&nbsp;&nbsp;");
            value = Regex.Replace(value, "\r\n", "<br />");
            value = Regex.Replace(value, "\r", "<br />");
            value = Regex.Replace(value, "\n", "<br />");
            return value;
        }
        /// <summary>
        /// ���MD5֧������
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string MD5(this string value)
        {
            byte[] b = Encoding.Default.GetBytes(value);
            b = new MD5CryptoServiceProvider().ComputeHash(b);
            string ret = "";
            for (int i = 0; i < b.Length; i++)
                ret += b[i].ToString("x").PadLeft(2, '0');
            return ret;
        }

        public static string SHA1Encrypt(this string value)
        {
            byte[] strRes = Encoding.UTF8.GetBytes(value);
            strRes = SHA1.Create().ComputeHash(strRes);
            StringBuilder sb = new StringBuilder();
            foreach (byte iByte in strRes)
            {
                sb.AppendFormat("{0:x2}", iByte);
            }
            return sb.ToString();
        }
        private static byte[] AESKeys = { 0x41, 0x72, 0x65, 0x79, 0x6F, 0x75, 0x6D, 0x79, 0x53, 0x6E, 0x6F, 0x77, 0x6D, 0x61, 0x6E, 0x3F };
        public static string AESKey = "";

        /// <summary>
        /// ��int[]ת����string[]
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static string[] IntArrayToStringArray(int[] array)
        {
            string[] strArr = new string[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                strArr[i] = array[i].ToString();
            }

            return strArr;
        }

        /// <summary>
        /// �� string[] ת���� int[]
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static int[] StringArrayToIntArray(string[] array)
        {
            int[] array1 = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array1[i] = Convert.ToInt32(array[i]);
            }
            return array1;
        }

        /// <summary>
        /// ����ִ��е�HTML����
        /// </summary>
        /// <param name="str">�ַ���</param>
        /// <returns>���HTML����֮����ַ���</returns>
        public static string ClearHtmlCode(object input)
        {
            return ClearHtmlCode(input, false);
        }

        /// <summary>
        /// ����ִ��е�HTML���룬�Ƿ������ݱ�ǩ
        /// </summary>
        /// <param name="str">�ַ���</param>
        /// <returns>���HTML����֮����ַ���</returns>
        public static string ClearHtmlCode(object input, bool retainSimple)
        {
            string str = Convert.ToString(input);
            if (!retainSimple)
                str = Regex.Replace(str, "<br[^>]*?>|<p[^>]*?>", System.Environment.NewLine, (RegexOptions)25);
            if (retainSimple)
                str = Regex.Replace(str, @"<(?!(img|/?strong|/?b|/?br|/?p|/?embed)\W*).*?>", string.Empty, (RegexOptions)25);
            else
                str = Regex.Replace(str, "<.*?>", string.Empty, (RegexOptions)25);
            str = Regex.Replace(str, "&nbsp;", " ", (RegexOptions)25);
            return str;
        }


        /// <summary>
        /// ����A��ǩ�����ӣ�
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ClearLinks(string input)
        {
            input = Regex.Replace(input, "<a[^>]*?>(.*?)</a>", "$1", (RegexOptions)25);
            return input;
        }

        /// <summary>
        /// ����Img��ǩ��ͼƬ��
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ClearImgs(string input)
        {
            input = Regex.Replace(input, "<img [^<]*>", string.Empty, (RegexOptions)25);
            return input;
        }

        /// <summary>
        /// ��������ʽ��ʾ����
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static string FormatDate(DateTime datetime)
        {
            DateTime today = DateTime.Now;

            TimeSpan ts = today.Subtract(datetime);
            if (ts.TotalMinutes < 60)
            {
                if (ts.TotalMinutes < 1)
                    return "����һ����ǰ";
                else
                    return string.Format("{0}����ǰ", ts.Minutes);
            }
            if (ts.TotalHours < 5)
            {
                if (ts.TotalHours - ts.Hours < 0.5)
                    return string.Format("{0}��Сʱǰ", ts.Hours);
                else
                    return string.Format("{0}����Сʱǰ", ts.Hours);
            }

            ts = today.Date.Subtract(datetime.Date);

            if (ts.Days == 0)
            {
                return string.Format("����{0}��{1}��", datetime.Hour, datetime.Minute);
            }
            if (ts.Days == 1)
            {
                return string.Format("����{0}��{1}��", datetime.Hour, datetime.Minute);
            }
            if (ts.Days == 2)
            {
                return string.Format("ǰ��{0}��{1}��", datetime.Hour, datetime.Minute);
            }
            //if (ts.Days == 3)
            //{
            //    return string.Format("��ǰ��{0}��{1}��", dt.Hour, dt.Minute);
            //}
            return datetime.ToString("yyyy-MM-dd hh:mm");
        }

        public static IEnumerable<string> SplitByLength(this string str, int maxLength)
        {
            for (int index = 0; index < str.Length; index += maxLength)
            {
                yield return str.Substring(index, Math.Min(maxLength, str.Length - index));
            }
        }

    }
}
