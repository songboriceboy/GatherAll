using System;
using System.Text;
using System.Text.RegularExpressions;

namespace BlogGather
{
    /// <summary>
    /// 过滤类
    /// </summary>
    public class Filter
    {
        /// <summary>
        /// 需要过滤的字符（多个以|相隔）
        /// </summary>
        public static String keyWord = "";
        /// <summary>
        /// 需要过滤的字符（多个以|相隔）
        /// </summary>
        public static String KeyWord
        {
            get { return keyWord; }
            set { keyWord = value; }
        }
        /// <summary>
        /// 过滤 javascript
        /// </summary>
        /// <param name="content">需过滤文本内容</param>
        /// <returns></returns>
        public static String FilterScript(String content)
        {
            String commentPattern = @"(?'comment'<!--.*?--[ \n\r]*>)";
            String embeddedScriptComments = @"(\/\*.*?\*\/|\/\/.*?[\n\r])";
            String scriptPattern = String.Format(@"(?'script'<[ \n\r]*script[^>]*>(.*?{0}?)*<[ \n\r]*/script[^>]*>)", embeddedScriptComments);
            String pattern = String.Format(@"(?s)({0}|{1})", commentPattern, scriptPattern);
            return StripScriptAttributesFromTags(Regex.Replace(content, pattern, String.Empty, RegexOptions.IgnoreCase));
        }
        /// <summary>
        /// 过滤javascript属性值（如onclick等）
        /// </summary>
        /// <param name="content">需过滤文本内容</param>
        /// <returns></returns>
        private static String StripScriptAttributesFromTags(String content)
        {
            String eventAttribs = @"on(blur|c(hange|lick)|dblclick|focus|keypress|(key|mouse)(down|up)|(un)?load
                    |mouse(move|o(ut|ver))|reset|s(elect|ubmit))";

            String pattern = String.Format(@"(?inx)
                \<(\w+)\s+
                    (
                        (?'attribute'
                        (?'attributeName'{0})\s*=\s*
                        (?'delim'['""]?)
                        (?'attributeValue'[^'"">]+)
                        (\3)
                    )
                    |
                    (?'attribute'
                        (?'attributeName'href)\s*=\s*
                        (?'delim'['""]?)
                        (?'attributeValue'javascript[^'"">]+)
                        (\3)
                    )
                    |
                    [^>]
                )*
            \>", eventAttribs);
            Regex re = new Regex(pattern);
            // 使用MatchEvaluator的委托
            return re.Replace(content, new MatchEvaluator(StripAttributesHandler));
        }
        /// <summary>
        /// 取得属性值
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private static String StripAttributesHandler(Match m)
        {
            if (m.Groups["attribute"].Success)
            {
                return m.Value.Replace(m.Groups["attribute"].Value, "");
            }
            else
            {
                return m.Value;
            }
        }
        /// <summary>
        /// 去掉javascript（scr链接方式）
        /// </summary>
        /// <param name="content">需过滤文本内容</param>
        /// <returns></returns>
        public static String FilterAHrefScript(String content)
        {
            String newstr = FilterScript(content);
            String regexstr = @" href[ ^=]*= *[\s\S]*script *:";
            return Regex.Replace(newstr, regexstr, String.Empty, RegexOptions.IgnoreCase);
        }
        /// <summary>
        /// 去掉链接文件
        /// </summary>
        /// <param name="content">需过滤文本内容</param>
        /// <returns></returns>
        public static String FilterSrc(String content)
        {
            String newstr = FilterScript(content);
            String regexstr = @" src *= *['""]?[^\.]+\.(js|vbs|asp|aspx|php|jsp)['""]";
            return Regex.Replace(newstr, regexstr, @"", RegexOptions.IgnoreCase);
        }
        /// <summary>
        /// 过滤HTML
        /// </summary>
        /// <param name="content">需过滤文本内容</param>
        /// <returns></returns>
        public static String FilterHtml(String content)
        {
            String newstr = FilterScript(content);
            String regexstr = @"<[^>]*>";
            return Regex.Replace(newstr, regexstr, String.Empty, RegexOptions.IgnoreCase);
        }
        /// <summary>
        /// 过滤 OBJECT
        /// </summary>
        /// <param name="content">需过滤文本内容</param>
        /// <returns></returns>
        public static String FilterObject(String content)
        {
            String regexstr = @"(?i)<Object([^>])*>(\w|\W)*</Object([^>])*>";
            return Regex.Replace(content, regexstr, String.Empty, RegexOptions.IgnoreCase);
        }
        /// <summary>
        /// 过滤iframe
        /// </summary>
        /// <param name="content">需过滤文本内容</param>
        /// <returns></returns>
        public static String FilterIframe(String content)
        {
            String regexstr = @"(?i)<Iframe([^>])*>(\w|\W)*</Iframe([^>])*>";
            return Regex.Replace(content, regexstr, String.Empty, RegexOptions.IgnoreCase);
        }
        /// <summary>
        /// 过滤frameset
        /// </summary>
        /// <param name="content">需过滤文本内容</param>
        /// <returns></returns>
        public static String FilterFrameset(String content)
        {
            String regexstr = @"(?i)<Frameset([^>])*>(\w|\W)*</Frameset([^>])*>";
            return Regex.Replace(content, regexstr, String.Empty, RegexOptions.IgnoreCase);
        }
        /// <summary>
        /// 移除非法或不友好字符
        /// </summary>
        /// <param name="content">关键字列表,多个以 | 分隔</param>
        /// <returns></returns>
        public static String FilterBadWords(String content)
        {
            //这里的非法和不友好字符由你任意加，用“|”分隔，支持正则表达式,由于本Blog禁止贴非法和不友好字符，所以这里无法加上。
            if (content == "")
                return "";
            String[] bwords = keyWord.Split('|');
            if (bwords.Length < 1) return content;
            int i, j;
            String str;
            StringBuilder sb = new StringBuilder();
            for (i = 0; i < bwords.Length; i++)
            {
                str = bwords[i].ToString().Trim();
                String regStr, toStr;
                regStr = str;
                Regex r = new Regex(regStr, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline);
                Match m = r.Match(content);
                if (m.Success)
                {
                    j = m.Value.Length;
                    sb.Insert(0, "*", j);
                    toStr = sb.ToString();
                    content = Regex.Replace(content, regStr, toStr, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline);
                }
                sb.Remove(0, sb.Length);
            }
            return content;
        }
        /// <summary>
        /// 过滤以上所有
        /// </summary>
        /// <param name="content">需过滤文本内容</param>
        /// <returns></returns>
        public static String FilterAll(String content)
        {
            //content = FilterHtml(content);
            content = FilterScript(content);
            content = FilterAHrefScript(content);
            content = FilterObject(content);
            content = FilterIframe(content);
            content = FilterFrameset(content);
            //content = FilterSrc(content);
            content = FilterBadWords(content);
            return content;
        }
    }
}
