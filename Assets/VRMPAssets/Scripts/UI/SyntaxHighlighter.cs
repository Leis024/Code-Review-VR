using System.Text.RegularExpressions;

public static class SyntaxHighlighter
{
    public static string Highlight(string code, Language lang)
    {
        switch (lang)
        {
            case Language.CSharp:
                // Highlight C# keywords
                code = Regex.Replace(code, @"\b(public|private|protected|class|void|int|string|float|double|bool|static|readonly|new|return|if|else|for|foreach|while|switch|case|break|continue|try|catch|finally|throw|using)\b", 
                    "<color=#569CD6>$1</color>");  // Blue keywords (VS style)
                
                // Highlight C# types
                code = Regex.Replace(code, @"\b(bool|byte|char|decimal|double|float|int|long|object|sbyte|short|string|uint|ulong|ushort)\b", 
                    "<color=#4EC9B0>$1</color>");  // Light teal for types (VS style)

                // Highlight C# function declarations (method names followed by parentheses)
                code = Regex.Replace(code, @"\b(\w+)\s*(?=\()", 
                    "<color=#DCDCAA>$1</color>");  // Light yellow for function names

                // Highlight C# strings
                code = Regex.Replace(code, "\"([^\"]*)\"", 
                    "<color=#D69D85>\"$1\"</color>");  // Light orange for strings

                // Highlight C# comments
                code = Regex.Replace(code, @"//.*$", 
                    "<color=#57A64A>$0</color>", RegexOptions.Multiline);  // Green for comments

                // Highlight C# numbers
                code = Regex.Replace(code, @"\b\d+\b", 
                    "<color=#B5CEA8>$0</color>");  // Light green for numbers
                break;

            case Language.JavaScript:
                // Highlight JavaScript keywords
                code = Regex.Replace(code, @"\b(function|var|let|const|if|else|for|while|return|break|continue|try|catch|finally|switch|case)\b", 
                    "<color=#569CD6>$1</color>");  // Blue keywords

                // Highlight JavaScript types
                code = Regex.Replace(code, @"\b(Number|String|Boolean|Array|Object|Date|RegExp)\b", 
                    "<color=#4EC9B0>$1</color>");  // Light teal for types

                // Highlight JavaScript function names
                code = Regex.Replace(code, @"\b(\w+)\s*(?=\()", 
                    "<color=#DCDCAA>$1</color>");  // Light yellow for function names

                // Highlight JavaScript strings
                code = Regex.Replace(code, "\"([^\"]*)\"", 
                    "<color=#D69D85>\"$1\"</color>");  // Light orange for strings

                // Highlight JavaScript comments
                code = Regex.Replace(code, @"//.*$", 
                    "<color=#57A64A>$0</color>", RegexOptions.Multiline);  // Green for comments

                // Highlight JavaScript numbers
                code = Regex.Replace(code, @"\b\d+\b", 
                    "<color=#B5CEA8>$0</color>");  // Light green for numbers
                break;

            case Language.Python:
                // Highlight Python keywords
                code = Regex.Replace(code, @"\b(def|class|return|if|else|elif|for|while|try|except|finally|break|continue|import|from|as|with|pass)\b", 
                    "<color=#569CD6>$1</color>");  // Blue keywords

                // Highlight Python function names
                code = Regex.Replace(code, @"\b(\w+)\s*(?=\()", 
                    "<color=#DCDCAA>$1</color>");  // Light yellow for function names

                // Highlight Python strings
                code = Regex.Replace(code, "\"([^\"]*)\"", 
                    "<color=#D69D85>\"$1\"</color>");  // Light orange for strings

                // Highlight Python comments
                code = Regex.Replace(code, @"#.*$", 
                    "<color=#57A64A>$0</color>", RegexOptions.Multiline);  // Green for comments

                // Highlight Python numbers
                code = Regex.Replace(code, @"\b\d+\b", 
                    "<color=#B5CEA8>$0</color>");  // Light green for numbers
                break;

            case Language.HTML:
                // Highlight HTML tags and attributes
                code = Regex.Replace(code, @"<(\w+)(\s[^>]*)?>", "<color=#569CD6><$1$2></color>"); // Blue for tags
                code = Regex.Replace(code, @"</(\w+)>", "<color=#569CD6></$1></color>"); // Blue for closing tags

                // Highlight HTML attributes
                code = Regex.Replace(code, @"\b(\w+)=", "<color=#9CDCFE>$1</color>="); // Light blue for attributes

                // Highlight attribute values (strings)
                code = Regex.Replace(code, "\"([^\"]*)\"", "<color=#D69D85>\"$1\"</color>"); // Light orange for attribute values
                break;

            case Language.CSS:
                // Highlight CSS selectors (IDs, classes, element names)
                code = Regex.Replace(code, @"(#\w+|\.\w+|\w+)\s*{", "<color=#DCDCAA>$1</color>{"); // Light yellow for selectors

                // Highlight CSS properties
                code = Regex.Replace(code, @"\b(color|background|font-size|margin|padding|border|width|height|display|position|top|left|right|bottom|z-index|flex|grid|align-items|justify-content)\b", "<color=#569CD6>$1</color>:"); // Blue for properties

                // Highlight CSS property values (strings, numbers, and common units)
                code = Regex.Replace(code, @"(:\s*)(#[0-9a-fA-F]{3,6}|\b\d+(px|em|rem|%)?)", "$1<color=#B5CEA8>$2</color>"); // Light green for values and hex codes
                code = Regex.Replace(code, "\"([^\"]*)\"", "<color=#D69D85>\"$1\"</color>"); // Light orange for string values

                // Highlight CSS comments
                code = Regex.Replace(code, @"/\*.*?\*/", "<color=#57A64A>$0</color>", RegexOptions.Singleline); // Green for comments
                break;

            default:
                // No syntax highlighting, treat it as plain text
                return code;
        }

        return code;
    }
}

public enum Language
{
    CSharp,
    JavaScript,
    Python,
    HTML,
    CSS,
    PlainText,  // Fallback for unknown file types
}
