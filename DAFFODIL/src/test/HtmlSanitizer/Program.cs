using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vereyon.Web
{
    public class Program
    {
        static void Main(string[] args)
        {
            var sanitizer = HtmlSanitizer.SimpleHtml5Sanitizer();

            string input = @"<h1>Heading</h1>
<p onclick=""alert('gotcha!')"">Some comments<span></span></p>
<script type=""text/javascript"">I'm illegal for sure</script>
<p><a href=""http://www.vereyon.com/"">Nofollow legal link</a> and here's another one: <a href=""javascript:alert('test')"">Obviously I'm illegal</a></p>";
            string expected = @"<h1>Heading</h1>
<p>Some comments</p>

<p><a href=""http://www.vereyon.com/"" target=""_blank"" rel=""nofollow"">Nofollow legal link</a> and here&#39;s another one: Obviously I&#39;m illegal</p>";

            var output = sanitizer.Sanitize(input);
            if (expected == output) Console.WriteLine("Successful");
        }
    }
}
