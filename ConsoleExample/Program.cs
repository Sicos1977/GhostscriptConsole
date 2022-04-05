using System;
using System.Collections.Generic;
using System.IO;
using Ghostscript.NET.Processor;

namespace ConsoleExample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var logStream = new MemoryStream())
            using (var ghostscriptProcessor = new GhostscriptProcessor())
            {
                var io = new StringBuilderIO(false, true, true, logStream);
                var arguments = new List<string>();

                arguments.Add("-dNOPAUSE");
                arguments.Add("-dBATCH");
                //arguments.Add("--permit-file-all=\"D:/test/*;c:/windows/fonts\"");
                arguments.Add("--permit-file-all=\"D:/test/*\"");
                //arguments.Add("-sFONTPATH=\"c:/windows/fonts\"");
                //arguments.Add("-ID:/test");
                arguments.Add("-dNEWPDF=false");
                arguments.Add("-sPAPERSIZE=a4");
                //arguments.Add("-dEmbedAllFonts=true");
                //arguments.Add("-dSubsetFonts=true");
                arguments.Add("-sDEVICE=pdfwrite");
                //arguments.Add("-r300");
                arguments.Add("-dPDFSETTINGS=/printer");
                arguments.Add("-dCompatibilityLevel=1.6");
                //arguments.Add("-dFastWebView");
                arguments.Add("-sOutputFile=\"D:/test/testoutput.pdf\"");
                //arguments.Add("-dDownsampleMonoImages=true");
                //arguments.Add("-dDownsampleGrayImages=true");
                //arguments.Add("-dGrayImageDownsampleThreshold=1.5");
                //arguments.Add("-dGrayImageDownsampleType=/Bicubic");
                //arguments.Add("-dGrayImageResolution=300");
                //arguments.Add("-dDownsampleColorImages=true");
                //arguments.Add("-dColorImageDownsampleThreshold=1.5");
                //arguments.Add("-dColorImageDownsampleType=/Bicubic");
                //arguments.Add("-dColorImageResolution=150");
                //arguments.Add("-sCIDFMAP=D:/test/cidfmap");
                arguments.Add("d:/test/test.pdf");

                try
                {
                    ghostscriptProcessor.Process(arguments.ToArray(), io);
                    var log = io.StandardOutput.ToString();

                }
                catch (Exception exception)
                {
                    var log = io.StandardOutput.ToString();
                    Console.Write(log);
                }
            }
        }
    }
}
