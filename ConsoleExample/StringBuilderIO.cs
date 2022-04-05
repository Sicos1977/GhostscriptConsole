using System;
using System.IO;
using System.Text;
using Ghostscript.NET;

namespace ConsoleExample
{
    internal class StringBuilderIO : GhostscriptStdIO
    {
        #region Fields
        private readonly Stream _logStream;

        /// <summary>
        ///     An unique id that can be used to identify the logging of the converter when
        ///     calling the code from multiple threads and writing all the logging to the same file
        /// </summary>
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable 649
        private static string _instanceId;
#pragma warning restore 649
#pragma warning restore IDE0044 // Add readonly modifier

        private bool _writePrefix = true;
        #endregion

        #region Properties
        /// <summary>
        /// Returns the standard output by GhostScript
        /// </summary>
        public StringBuilder StandardOutput { get; }
        
        /// <summary>
        /// Returns the errors output by GhostScript
        /// </summary>
        public StringBuilder StandardError { get; }

        /// <summary>
        ///     An unique id that can be used to identify the logging of the converter when
        ///     calling the code from multiple threads and writing all the logging to the same file
        /// </summary>
        public string InstanceId { get; set; }

        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the Ghostscript.NET.GhostscriptStdIO class.
        /// </summary>
        /// <param name="handleStdIn">Whether or not to handle Ghostscript standard input.</param>
        /// <param name="handleStdOut">Whether or not to handle Ghostscript standard output.</param>
        /// <param name="handleStdErr">Whether or not to handle Ghostscript standard errors.</param>
        /// <param name="standardOutputStream"></param>
        public StringBuilderIO(
            bool handleStdIn, 
            bool handleStdOut, 
            bool handleStdErr,
            Stream standardOutputStream) : base(handleStdIn, handleStdOut, handleStdErr)
        {
            _logStream = standardOutputStream;
            StandardOutput = new StringBuilder();
            StandardError = new StringBuilder();
        }
        #endregion

        #region Overrides
        /// <summary>Abstract standard input method.</summary>
        /// <param name="input">Input data.</param>
        /// <param name="count">Expected size of the input data.</param>
        public override void StdIn(out string input, int count)
        {
            input = null;
        }

        /// <summary>Abstract standard output method.</summary>
        /// <param name="output">Output data.</param>
        public override void StdOut(string output)
        {
            WriteToLog(output);
            StandardOutput.Append(output);
        }

        /// <summary>Abstract standard error method.</summary>
        /// <param name="error">Error data.</param>
        public override void StdError(string error)
        {
            StandardError.Append(error);
        }
        #endregion
        
        #region WriteToLog
        /// <summary>
        ///     Writes a line and linefeed to the <see cref="_logStream" />
        /// </summary>
        /// <param name="message">The message to write</param>
        private void WriteToLog(string message)
        {
            try
            {
                if (_logStream == null || !_logStream.CanWrite) return;
                var prefix = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff") +
                             (_instanceId != null ? " - " + _instanceId : string.Empty) + " - ";

                string line;

                if (!_writePrefix)
                    line = message;
                else
                    line = prefix + message;

                _writePrefix = message.EndsWith("\n");

                var sb = new StringBuilder();

                for (var i = 0; i < line.Length; i++)
                {
                    var c = line[i];

                    if (c == '\n' && i != line.Length - 1)
                        sb.Append($"\n{prefix}");
                    else
                        sb.Append(c);
                }

                line = sb.ToString();

                var bytes = Encoding.UTF8.GetBytes(line);
                _logStream.Write(bytes, 0, bytes.Length);
                _logStream.Flush();
            }
            catch (ObjectDisposedException)
            {
                // Ignore
            }
        }
        #endregion
    }
}
