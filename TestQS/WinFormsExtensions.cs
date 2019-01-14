using System;
using System.Windows.Forms;

namespace TestQS
{
    /// <summary>
    /// My the class helper to WinForm controls
    /// </summary>
    public static class WinFormsExtensions
    {
        /// <summary>
        /// Extending textbox control. Add line method.
        /// </summary>
        /// <param name="source">TextBox</param>
        /// <param name="text">New line to text</param>
        public static void AddLine(this TextBox source, string text)
        {
            if (source.Text.Length == 0)
                source.Text = text;
            else
                source.AppendText(Environment.NewLine + text);
        }

        /// <summary>
        /// HtmlElement search method by class name.
        /// </summary>
        /// <param name="source">WebBrowser</param>
        /// <param name="className">Class name</param>
        /// <returns>HtmlElement or null</returns>
        public static HtmlElement FindElementByClass(this WebBrowser source, string className)
        {
            foreach (HtmlElement element in source.Document.All)
            {
                if (element.GetAttribute("className") == className)
                    return element;
            }
            return null;
        }
    }
}
