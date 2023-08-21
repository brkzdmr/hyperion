using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dock_Examples.Interrogator
{
	/* 
		The hyperion command response protocol returns an 8-byte header + human readable message + binary computer
		parsable content. The message and content can be individually suppressed to reduce the transmission bandwidth
		if only one of the response forms is required.

			Offset	Size (in bytes)	Description
			0				1		Status (see below)
			1				1		Options (echoed from request - see below)
			2				2		Message Length (in bytes) Nm
			4				4		Content Length (in bytes) Nc

		The total length of a raw response to a command is 8 (header legnth) + Nm (message length) + Nc (content length).

		The status values are
			0 - Success
			1 - Error

		The Options are
			0 - None (default)
			1 - Suppress Message
			2 - Suppress Content
			4 - Use Compression
	*/

	public class CommandResponse
	{
		#region -- Constants --

		internal const int HeaderSizeInBytes = 8;

		internal const int HeaderStatusOffsetInBytes = 0;
		internal const int HeaderOptionOffsetInBytes = 1;
		internal const int HeaderMessageLengthOffsetInBytes = 2;
		internal const int HeaderContentLengthOffsetInBytes = 4;

		#endregion


		#region -- Constructors --

		internal CommandResponse( CommandStatus status, CommandOptions options, string message, byte[] content )
		{
			Status = status;
			Options = options;
			Message = message;
			Content = content;
		}

		#endregion


        #region -- Public Properties --

        public CommandStatus Status { get; }

		public CommandOptions Options { get; }

		public string Message { get; }

		public byte[] Content { get; }

		#endregion
	}
}
