using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dock_Examples.Interrogator
{
	/* 
		The hyperion command request protocol requires an 8-byte header + binary content. The message and content of the response
		can be individually suppressed to reduce the transmission bandwidth	if only one of the response forms is required.

			Offset	Size (in bytes)	Description
			0				1		Options (see below)
			1				1		Unused
			2				2		Name Length (in bytes) Nn
			4				4		Argument Length (in bytes) Na

		The total length of a raw request to a command is 8 (header legnth) + Nn (name length) + Na (argument length).

		The Options are
			0 - None (default)
			1 - Suppress Message
			2 - Suppress Content
			4 - Use Compression
	*/

	public class CommandRequest
	{
		#region -- Constants --

		internal const int HeaderSizeInBytes = 8;

		internal const int HeaderOptionOffsetInBytes = 0;
		internal const int HeaderUnusedOffsetInBytes = 1;
		internal const int HeaderNameLengthOffsetInBytes = 2;
		internal const int HeaderContentLengthOffsetInBytes = 4;

		#endregion
	}
}
